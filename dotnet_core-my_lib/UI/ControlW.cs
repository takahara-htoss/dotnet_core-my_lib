using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_core_my_lib.UI
{
    public class ControlW
    {
        public static void 再帰的にコントロールの入力をクリアする(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                if (c is TextBox textBox)
                {
                    textBox.Text = string.Empty;
                }
                else if (c is ComboBox comboBox)
                {
                    comboBox.SelectedIndex = -1; // 選択を解除
                }
                else if (c is CheckBox checkBox)
                {
                    checkBox.Checked = false; // チェックを解除
                }
                else if (c is RadioButton radioButton)
                {
                    radioButton.Checked = false; // チェックを解除
                }
                else if (c is DateTimePicker dateTimePicker)
                {
                    dateTimePicker.Value = DateTime.Now; // デフォルト値に戻す
                }
                // 子コントロールがある場合は再帰的に呼び出す
                if (c.HasChildren)
                {
                    再帰的にコントロールの入力をクリアする(c);
                }
            }
        }

        /// <summary>
        /// 永遠の課題だったので作成しました。
        /// 感動するくらいに実績ありです。
        /// TabIndexの仕様として、コントロール毎に独立しています。
        /// 例）panel1とpanel2自体には親のフォームのTabIndexが割り振られますが、panel1の子コントロールとpanel2の子コントロールは独立してTabIndexが割り振られます。
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="丸めのpixel">コントロールの標準高さが20から24ピクセルなのでこの間で丸めると良い結果になります。
        /// 逆に0にすると人の手による少しの誤差でindexが崩れます。</param>
        public static void TabIndexを原点から横方向を優先とし次に縦方向に再帰的に割り振りなおす(Control parent, int 丸めのpixel = 24)
        {   
            var controls = parent.Controls
                .Cast<Control>()
                //.Where(c => c.TabStop)    // パネルが無視されますのでコメントアウト（ラジオボタンはパネルに入れるのが王道）
                .OrderBy(c => c.Top / 丸めのpixel)
                .ThenBy(c => c.Left);


            int index = 0;

            foreach (var c in controls)
            {
                c.TabIndex = index++;
                if (c.HasChildren)
                    TabIndexを原点から横方向を優先とし次に縦方向に再帰的に割り振りなおす(c);
            }
        }

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
