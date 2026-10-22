using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_core_my_lib.UI
{
    public class フォーカスすると全選択する日付対応テキストボックス : フォーカスすると全選択するテキストボックス
    {
        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);

            if (string.IsNullOrWhiteSpace(this.Text))
            {
                // 未入力は許容
                return;
            }

            if (!DateTime.TryParse(this.Text, out var dt))
            {
                MessageBoxW.単純なエラーメッセージボックスを表示する("日付の形式が正しくありません。");
                this.Focus();
                this.SelectAll();
                return;
            }

            // 正常なら yyyy/MM/dd に整形
            this.Text = dt.ToString("yyyy/MM/dd");
        }
    }
}
