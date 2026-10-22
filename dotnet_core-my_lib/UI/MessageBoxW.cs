namespace dotnet_core_my_lib.UI
{
    public class MessageBoxW
    {
        /// <summary>
        /// 警告音が出ます。
        /// </summary>
        /// <param name="message"></param>
        public static void 単純なエラーメッセージボックスを表示する(string message)
        {
            MessageBox.Show(message, "エラー",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

        }

        /// <summary>
        /// 警告音が出ません。
        /// </summary>
        /// <param name="message"></param>
        public static void 単純な情報メッセージボックスを表示する(string message)
        {
            MessageBox.Show(message, "情報",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        /// <summary>
        /// 警告音が出ます。
        /// </summary>
        /// <param name="message"></param>
        public static void 単純な警告メッセージボックスを表示する(string message)
        {
            MessageBox.Show(message, "警告",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        /// <summary>
        /// 警告音は出ません。
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool 単純な二択の確認メッセージボックスを表示する(string message)
        {
            DialogResult result = MessageBox.Show(message, "確認",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            return result == DialogResult.Yes;
        }

        /// <summary>
        /// 警告音が出ます。
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool 単純な二択の警告メッセージボックスを表示する(string message)
        {
            DialogResult result = MessageBox.Show(message, "警告",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            return result == DialogResult.Yes;
        }
    }
}
