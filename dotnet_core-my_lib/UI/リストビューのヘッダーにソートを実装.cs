using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_core_my_lib.UI
{
    [Obsolete("VB6の移植で作ってしまいましたが、これを実装するならDataGridViewを使用してください。", false)]
    public class リストビューのヘッダーにソートを実装
    {
        private int sortColumn = -1;
        private SortOrder sortOrder = SortOrder.None;
        private ListView listView;
        private Form form;

        /// <summary>
        /// リストビューのヘッダーの列にソート機能を実装します。
        /// フォームロードで一度だけ宣言してください。
        /// コンストラクタの場合はInitializeComponent();後に宣言してください。
        /// 例）
        /// _ = new リストビューのヘッダーにソートを実装(this, listView社員情報); // アンダーバーはDiscardの意味で、戻り値は使わず破棄するという意味です、昔は右辺のみを定義していましたがIDEが警告を出しますので対処です。
        /// </summary>
        /// <param name="form"></param>
        /// <param name="listView"></param>
        public リストビューのヘッダーにソートを実装(Form form, ListView listView)
        {
            this.form = form;
            this.listView = listView;
            // カラムヘッダークリックイベントを設定
            listView.ColumnClick -= ListView_ColumnClick;   // 本来はフォームのロード時に一度だけ設定するべきですが、間違えて複数回設定してしまった場合の防衛です、念のため既存のイベントハンドラを解除してから追加します。
            listView.ColumnClick += ListView_ColumnClick;
        }

        private void ListView_ColumnClick(object? sender, ColumnClickEventArgs e)
        {
            Debug.WriteLine($"ListView_ColumnClick: ColumnIndex={e.Column}");

            form.Enabled = false;
            try
            {
                // 同じカラムがクリックされた場合は昇順/降順を切り替え
                if (e.Column == sortColumn)
                {
                    sortOrder = sortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
                }
                else
                {
                    // 異なるカラムがクリックされた場合は昇順でソート
                    sortColumn = e.Column;
                    sortOrder = SortOrder.Ascending;
                }

                // カスタムソーターを設定してソート実行
                listView.ListViewItemSorter = new ListViewItemComparer(e.Column, sortOrder);
                listView.Sort();
            }
            finally
            {
                form.Enabled = true;
            }
        }

        /// <summary>
        /// ListViewItem用のカスタムコンパレータークラス
        /// </summary>
        public class ListViewItemComparer : IComparer
        {
            private int col;
            private SortOrder order;

            public ListViewItemComparer()
            {
                col = 0;
                order = SortOrder.Ascending;
            }

            public ListViewItemComparer(int column, SortOrder order)
            {
                col = column;
                this.order = order;
            }

            public int Compare(object? x, object? y)
            {
                if (x == null || y == null) return 0;

                var itemX = (ListViewItem)x;
                var itemY = (ListViewItem)y;

                string strX = col == 0 ? itemX.Text : itemX.SubItems[col].Text;
                string strY = col == 0 ? itemY.Text : itemY.SubItems[col].Text;

                int returnVal = string.Compare(strX, strY, StringComparison.OrdinalIgnoreCase);

                // 降順の場合は結果を反転
                if (order == SortOrder.Descending)
                    returnVal *= -1;

                return returnVal;
            }
        }
    }
}
