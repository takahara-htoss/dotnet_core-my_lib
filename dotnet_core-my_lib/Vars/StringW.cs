using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace dotnet_core_my_lib.Vars
{
    public class StringW
    {
        /// <summary>
        /// 主にパスワードのハッシュ化に使う文字列の長さを返すと思いますので以下を参考にしてください。
        /// 
        /// DTOのプロパティにRegularExpressionを実装することも検討してください。（DTOの可読性が向上します）
        /// 
        /// ＜参考＞
        /// 結果が64文字を要求する場合
        /// 半角文字のみでplainTextは16文字だと64文字になります。
        /// 全角だとオーバーフローする。
        /// 半角文字31文字まではギリギリだが非推奨。
        /// 16文字以上にする場合は、保存先のカラムの型を計算する必要があります。
        /// AIに相談してください。
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool 半角英数字記号のみか(string text)
        {
            return Regex.IsMatch(text, 正規表現.半角英数字記号のみ);
        }

        public class 正規表現
        {
            public const string 半角英数字記号のみ = @"^[ -~]+$";
        }
    }
}
