using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dotnet_core_my_lib.Forms
{
    /*
     使い方の例
     コール元で
        private async Task TreeViewの子ノードを再生成する(RadioButton radio)
        {
            listView社員情報.Items.Clear(); // ツリービューの子ノードを再生成する前に、ListViewの社員情報をクリアします。

            ProgressDialog pDlg = new(this);
            List<Dictionary<string, string>> dictionaryList = await pDlg.ShowAsync(
                async (cancellationToken, progress) =>
                {
                    return await Task.Run(() =>
                    {
                        // タスクメソッドをサブスレッドで実行します。
                        return RadioButtonによりTreeViewのデータを取得する(radio, progress, cancellationToken);
                    });
                },
                "データを取得中...",
                allowCancel: true,
                title: "ツリーの再生成"
            );

            ...
        }

        そしてタスクメソッド（処理）の中身
        private List<Dictionary<string, string>> RadioButtonによりTreeViewのデータを取得する(RadioButton radio, IProgress<string> progress, CancellationToken token)
        {
            var dictionaryList = new List<Dictionary<string, string>>();

            int skub = ラジオボタンから社員区分を取得する(radio);
            string 追加の検索条件 = 追加の検索条件を取得して返す();

            progress.Report("カナ順を作成中");
            // コール元でdictionaryListの配列要素順序を利用するので、ここで順序を意識して追加する必要があります。
            dictionaryList.Add(Mew1No2.社員区分からカナ順の五十音行コードと五十音順行文字セットを取得する(skub, false, 追加の検索条件));
            if (token.IsCancellationRequested)
                // ユーザーがキャンセルボタンを押しましたので途中の状態で抜けます。
                return dictionaryList;

            progress.Report("店舗カナ順を作成中");
            bool 店舗カナ順 = true;
            dictionaryList.Add(Mew1No2.社員区分からカナ順の五十音行コードと五十音順行文字セットを取得する(skub, 店舗カナ順, 追加の検索条件));
            if (token.IsCancellationRequested)
                return dictionaryList;

            progress.Report("HPID順を作成中");
            var dictHpid = Mew1No2.社員区分からHPID順を昇順で取得する(skub, 追加の検索条件);
            dictionaryList.Add(dictHpid);
            if (token.IsCancellationRequested)
                return dictionaryList;

            progress.Report("アドレス順を作成中");
            var dictAddress = Mew1No2.社員区分からアドレス順を昇順で取得する(skub, 追加の検索条件);
            dictionaryList.Add(dictAddress);
            if (token.IsCancellationRequested)
                return dictionaryList;

            progress.Report("iモードアドレス順を作成中");
            var dictImodeAddress = Mew1No2.社員区分からiモードアドレス順を昇順で取得する(skub, 追加の検索条件);
            dictionaryList.Add(dictImodeAddress);
            if (token.IsCancellationRequested)
                return dictionaryList;

            return dictionaryList;
        }

        ちなみに親フォーム画面のEnable制御はこのクラスで実装しているので親が実装する必要はありません。
        ただし、もし親で実装してしまった場合は親のタスク側でawaitで待たないと、メインメソッド（UIメソッド）が
        先に終了してしまい、親フォームがEnable状態のままになってしまうので注意してください。
        例）親のイベントハンドラ
        private async void radio社員区分_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                Enabled = false;

                RadioButton radio = sender as RadioButton ?? throw new InvalidOperationException("Senderはラジオボタンではありません");
                if (!radio.Checked)
                {
                    return;
                }

                // ここをawaitしないとUIスレッドが素通りしてしまいます。
                await TreeViewの子ノードを再生成する(radio);
            }
            finally
            {
                Enabled = true;
            }
        }
        そしてタスクメソッド
        private async Task TreeViewの子ノードを再生成する(RadioButton radio){}
        は戻り値の利用をしませんが、Taskを返すようにします。
        ここをvoidで返すとコール元はawaitできません、.netCoreの仕様です。
     */

    public partial class ProgressDialog : Form
    {
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _runningTask; // 非同期処理のタスクを保持するためのフィールド（コール元の処理です。）

        private Form? _parentForm;

        public ProgressDialog(Form? parentForm)
        {
            InitializeComponent();
            InitializeDialog();
            _parentForm = parentForm;
        }

        private void InitializeDialog()
        {
            // フォームの設定
            Text = "処理中...";
            Size = new Size(400, 150);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ShowInTaskbar = false;
            
            // フォームを閉じさせない（キャンセル以外）
            FormClosing += (s, e) =>
            {
                // ここではイベントの登録を行うだけで、実際のキャンセル処理はBtnCancel_Clickで行います。

                // キャンセルダイアログの場合は閉じることを許可
                if (DialogResult == DialogResult.Cancel)
                {
                    return;
                }
                
                // 処理中の場合は閉じることを拒否
                if (_runningTask != null && !_runningTask.IsCompleted)
                {
                    e.Cancel = true;
                }
            };
        }

        private void InitializeComponent()
        {
            progressBar = new ProgressBar();
            lblStatus = new Label();
            btnCancel = new Button();
            SuspendLayout();
            // 
            // progressBar
            // 
            progressBar.Location = new Point(20, 30);
            progressBar.MarqueeAnimationSpeed = 30;
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(340, 23);
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.TabIndex = 0;
            // 
            // lblStatus
            // 
            lblStatus.Location = new Point(20, 10);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(340, 15);
            lblStatus.TabIndex = 1;
            lblStatus.Text = "処理を実行中...";
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(285, 65);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "キャンセル";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += BtnCancel_Click;
            // 
            // ProgressDialog
            // 
            ClientSize = new Size(380, 100);
            ControlBox = false;
            Controls.Add(progressBar);
            Controls.Add(lblStatus);
            Controls.Add(btnCancel);
            Name = "ProgressDialog";
            ResumeLayout(false);
        }

        private ProgressBar progressBar = null!;
        private Label lblStatus = null!;
        private Button btnCancel = null!;

        /// <summary>
        /// ステータステキストを更新
        /// </summary>
        public void UpdateStatus(string message)
        {
            if (InvokeRequired)
            {
                Invoke(() => lblStatus.Text = message);
            }
            else
            {
                lblStatus.Text = message;
            }
        }

        /// <summary>
        /// キャンセルボタンの表示/非表示を設定
        /// </summary>
        public void SetCancelButtonVisible(bool visible)
        {
            if (InvokeRequired)
            {
                Invoke(() => btnCancel.Visible = visible);
            }
            else
            {
                btnCancel.Visible = visible;
            }
        }

        private async void BtnCancel_Click(object? sender, EventArgs e)
        {
            Text = "キャンセル中...";     // こちらの方が何の処理中にキャンセルしたかわかりやすい（開発者向け）
            //UpdateStatus("キャンセル中...");

            // キャンセルトークンを発行
            _cancellationTokenSource?.Cancel();
            btnCancel.Enabled = false; // キャンセルボタンを無効化して、二重キャンセルを防ぐ

            if (_runningTask != null)
            {
                await _runningTask; // 親タスクの完了を待つ
            }

            // ダイアログを閉じる
            DialogResult = DialogResult.Cancel;
            Close();
        }
        public async Task<T> ShowAsync<T>(
            Func<CancellationToken, IProgress<string>, Task<T>> asyncOperation,
            string initialMessage = "処理を実行中...",
            bool allowCancel = false,
            string title = "処理中...")
        {
            CancellationTokenSource? cancellationTokenSource = null;

            if (_parentForm == null)
            {
                throw new InvalidOperationException("親フォームが設定されていません");
            }

            if (_parentForm.Enabled == true)
            {
                _parentForm.Enabled = false; // 親フォームを無効化して、ユーザーが操作できないようにする
            }

            try
            {
                // UIスレッドでダイアログを作成・表示
                var taskCompletionSource = new TaskCompletionSource<T>();
                var dialogReady = new TaskCompletionSource<bool>();

                _parentForm.Invoke(() =>
                {
                    Text = title;
                    UpdateStatus(initialMessage);
                    SetCancelButtonVisible(allowCancel);

                    cancellationTokenSource = new CancellationTokenSource();
                    _cancellationTokenSource = cancellationTokenSource;

                    // キャンセル時の追加処理
                    if (allowCancel)
                    {
                        FormClosed += (s, e) =>
                        {
                            if (DialogResult == DialogResult.Cancel)
                            {
                                cancellationTokenSource?.Cancel();
                            }
                        };
                    }

                    // ダイアログ準備完了を通知
                    dialogReady.SetResult(true);

                    // ダイアログを表示（ShowDialogはここでスレッドを止めるので使ってはいけない）
                    Show(_parentForm);
                    
                    // プログレスを中央に配置
                    CenterToParent();
                });

                // ダイアログ準備完了を待つ
                await dialogReady.Task;

                if (this == null || cancellationTokenSource == null)
                {
                    throw new InvalidOperationException("ダイアログの初期化に失敗しました");
                }

                var progress = new Progress<string>(message => 
                {
                    if (this != null && !IsDisposed)
                    {
                        UpdateStatus(message);
                    }
                });

                // 非同期処理を実行
                var operationTask = asyncOperation(cancellationTokenSource.Token, progress);
                _runningTask = operationTask;   // ← フォームに登録
                var result = await operationTask;

                // ダイアログを閉じる
                _parentForm.Invoke(() =>
                {
                    if (this != null && !IsDisposed)
                    {
                        Close();
                    }
                });

                return result;
            }
            finally
            {
                try
                {
                    cancellationTokenSource?.Dispose();

                    // 確実にダイアログを閉じて破棄
                    if (_parentForm.InvokeRequired)
                    {
                        _parentForm.Invoke(() =>
                        {
                            if (this != null && !IsDisposed)
                            {
                                Close();
                                Dispose();
                            }
                        });
                    }
                    else
                    {
                        if (this != null && !IsDisposed)
                        {
                            Close();
                            Dispose();
                        }
                    }
                }
                finally
                {
                    // 全ての処理が確実に終わった後に親フォームを再度有効化
                    _parentForm.Enabled = true;
                    _parentForm.Activate(); // 理由は不明ですが、親のフォーカスが外れるので親をアクティブにする
                }
            }
        }
    }
}
