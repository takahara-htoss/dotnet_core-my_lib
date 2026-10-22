using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_core_my_lib.UI
{
    public class ControlW
    {
        /// <summary>
        /// 指定されたコントロールを複製し、名前にサフィックスを追加します。子コントロールも再帰的にコピーされます。
        /// </summary>
        /// <param name="original"></param>
        /// <param name="suffix"></param>
        /// <param name="fixEnabledTrue">
        /// TabPagerをコピーする際はなぜかコピー元のEnabledがfalseになりコピーされてしまうため、Enabledプロパティを強制的にtrueにする場合はtrueを指定します。
        /// </param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static Control CloneControl(Control original, string suffix, bool fixEnabledTrue = false)
        {
            Control? copy = (Control?)Activator.CreateInstance(original.GetType());
            if (copy == null)
                throw new Exception("コントロールのコピーに失敗しました。");

            copy.Name = original.Name + suffix;
            copy.Text = original.Text;
            copy.Size = original.Size;
            copy.Location = original.Location;
            copy.Anchor = original.Anchor;
            copy.Dock = original.Dock;
            copy.Font = original.Font;
            copy.BackColor = original.BackColor;
            copy.ForeColor = original.ForeColor;
            copy.Enabled = fixEnabledTrue ? true : original.Enabled;
            copy.Visible = original.Visible;

            // 子コントロールも再帰的にコピー
            foreach (Control child in original.Controls)
            {
                copy.Controls.Add(CloneControl(child, suffix, fixEnabledTrue));
            }

            return copy;
        }
    }
}
