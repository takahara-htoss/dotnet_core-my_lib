using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_core_my_lib.UI
{
    public class フォーカスすると全選択するテキストボックス : System.Windows.Forms.TextBox
    {
        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);
            this.SelectAll();
            BackColor = Color.PaleGreen;
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);
            BackColor = SystemColors.Window;
        }

        // フォーカスが無い状態でクリックしたとき、OnEnter → SelectAll を優先させるために、クリックイベントを無視するようにしています。
        // これがないとマウスのクリックでキャレットが移動してしまい、SelectAllの効果がなくなってしまいます。
        protected override void WndProc(ref Message m)
        {
            const int WM_LBUTTONDOWN = 0x0201;

            if (m.Msg == WM_LBUTTONDOWN && !Focused)
            {
                // まだフォーカスが無い時のクリックは無視して
                // OnEnter → SelectAll を優先させる
                Focus();
                return;
            }

            base.WndProc(ref m);
        }

    }
}
