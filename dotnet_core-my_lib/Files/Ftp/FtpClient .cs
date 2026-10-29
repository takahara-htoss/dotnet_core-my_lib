using FluentFTP;
using System.Diagnostics;

namespace dotnet_core_my_lib.Files.Ftp
{
    /// <summary>
    /// 20260408_AsyncFtpClientは不要と判断し作り直しました。
    /// FluentFTPを使用したFTPクライアントのラッパークラス
    /// このクラスは1セッションでメソッドの呼び出しを想定しており、接続の再利用とエラーハンドリングを強化しています。
    /// usingを使ってインスタンス化すればDispose()で接続が自動的に切断されるようになっています。
    /// 使わない場合は明示的にDisconnect()を呼び出すか、Dispose()を呼び出してリソースを解放してください。
    /// </summary>
    public class FtpClient : IDisposable
    {
        private readonly FluentFTP.FtpClient _ftpClient;
        private bool _disposed = false;

        public FtpClient(string server, string username, string password)
        {
            _ftpClient = new FluentFTP.FtpClient(server, username, password);
            
            // タイムアウト設定を調整
            _ftpClient.Config.ConnectTimeout = 30000;  // 接続タイムアウト: 30秒
            _ftpClient.Config.ReadTimeout = 30000;     // 読み取りタイムアウト: 30秒
            _ftpClient.Config.DataConnectionConnectTimeout = 30000;  // データ接続タイムアウト: 30秒
            _ftpClient.Config.DataConnectionReadTimeout = 30000;     // データ読み取りタイムアウト: 30秒
            
            // パッシブモードを有効にする（ファイアウォールを通過しやすい）
            _ftpClient.Config.DataConnectionType = FtpDataConnectionType.PASV;
            
            // 接続の再試行設定
            _ftpClient.Config.RetryAttempts = 3;
        }

        /// <summary>
        /// 複数のアップロードを一つのアップロードフォルダにまとめて行う
        /// 未テストなのでワンセッションで送るかどうかは不明です。
        /// </summary>
        /// <param name="localFiles"></param>
        /// <param name="remoteDir"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void UploadFiles(IEnumerable<FileInfo> localFiles, string remoteDir)
        {
            if (!_ftpClient.IsConnected)
            {
                _ftpClient.AutoConnect();
            }

            List<FtpResult> results = _ftpClient.UploadFiles(localFiles, remoteDir, FtpRemoteExists.Overwrite);
            List<string> failedMsgs = [];
            foreach (FtpResult? result in results)
            {                
                if (result != null)
                {
                    if (result.IsFailed)
                    {
                        failedMsgs.Add($"ファイル: {result.LocalPath} のアップロードに失敗しました。ファイルパス: {result.LocalPath}");
                    }
                    else if (result.IsSkipped)
                    {
                        failedMsgs.Add($"ファイル: {result.LocalPath} のアップロードはスキップされました。ファイルパス: {result.LocalPath}");
                    }
                    else if (!result.IsSuccess)
                    {
                        failedMsgs.Add($"ファイル: {result.LocalPath} のアップロードは成功以外でした。ファイルパス: {result.LocalPath}");
                    }
                }
            }
            if (failedMsgs.Count > 0)
            {
                throw new InvalidOperationException($"FTPアップロードに失敗しました。詳細: {string.Join(", ", failedMsgs)}");
            }
        }

        /// <summary>
        /// ファイルをアップロードする
        /// </summary>
        public void UploadFile(string localFilePath, string remoteFilePath)
        {
            if (!_ftpClient.IsConnected)
            {
                _ftpClient.AutoConnect();
            }
            
            var result = _ftpClient.UploadFile(localFilePath, remoteFilePath, FtpRemoteExists.Overwrite);            
            if (result != FtpStatus.Success)
            {
                throw new InvalidOperationException($"FTPアップロードに失敗しました。ステータス: {result}");
            }
        }

        /// <summary>
        /// ファイルをダウンロードする
        /// </summary>
        public void DownloadFile(string remoteFilePath, string localFilePath)
        {
            if (!_ftpClient.IsConnected)
            {
                _ftpClient.AutoConnect();
            }
            
            var result = _ftpClient.DownloadFile(localFilePath, remoteFilePath, FtpLocalExists.Overwrite);
            
            if (result != FtpStatus.Success)
            {
                throw new InvalidOperationException($"FTPダウンロードに失敗しました。ステータス: {result}");
            }
        }

        public List<string> ListDirectory(string directoryPath = "")
        {
            var fileList = new List<string>();

            // 接続を確実に行う
            if (!_ftpClient.IsConnected)
            {
                _ftpClient.AutoConnect();
            }

            // より具体的なパスを指定（空文字列の場合は "." を使用）
            var targetPath = string.IsNullOrEmpty(directoryPath) ? "." : directoryPath;

            var items = _ftpClient.GetListing(targetPath);

            foreach (var item in items)
            {
                if (item.Type == FtpObjectType.File)    // ファイルのみをリストに追加、ディレクトリは除外
                {
                    fileList.Add(item.Name);
                }
            }

            return fileList;
        }

        /// <summary>
        /// ファイルを削除する
        /// </summary>
        public void DeleteFile(string remoteFilePath)
        {
            throw new Exception("このシステムではCSVファイルの削除制御は無くなりました。");

            if (!_ftpClient.IsConnected)
            {
                _ftpClient.AutoConnect();
            }
            
            _ftpClient.DeleteFile(remoteFilePath);
        }

        /// <summary>
        /// FTPサーバーへの接続をテストする
        /// </summary>
        public void TestConnection()
        {
            _ftpClient.AutoConnect();
            
            if (!_ftpClient.IsConnected)
            {
                throw new InvalidOperationException("FTPサーバーへの接続に失敗しました");
            }
        }

        /// <summary>
        /// 接続を明示的に切断する
        /// </summary>
        public void Disconnect()
        {
            if (_ftpClient.IsConnected)
            {
                _ftpClient.Disconnect();
            }
        }

        /// <summary>
        /// ファイルをメモリ内に読み込む（バイト配列として取得）
        /// </summary>
        public byte[]? DownloadFileToBytes(string remoteFilePath)
        {
            try
            {
                if (!_ftpClient.IsConnected)
                {
                    _ftpClient.AutoConnect();
                }
                
                byte[] outBytes = [];
                if (!_ftpClient.DownloadBytes(out outBytes, remoteFilePath))
                {
                    return null;
                }

                return outBytes;
            }
            catch (TimeoutException ex)
            {
                System.Diagnostics.Debug.WriteLine($"FTPメモリ読み込みタイムアウトエラー: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FTPメモリ読み込みエラー: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// ファイルをメモリ内に読み込む（文字列として取得）
        /// </summary>
        public string? DownloadFileToString(string remoteFilePath, System.Text.Encoding? encoding = null)
        {
            try
            {
                var fileData =  DownloadFileToBytes(remoteFilePath);
                if (fileData == null) return null;
                
                encoding ??= System.Text.Encoding.UTF8;
                return encoding.GetString(fileData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FTP文字列変換エラー: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// ファイルをストリームとして読み込む（大きなファイル用）
        /// </summary>
        public Stream? DownloadFileToStream(string remoteFilePath)
        {
            try
            {
                if (!_ftpClient.IsConnected)
                {
                    _ftpClient.AutoConnect();
                }
                
                var stream = _ftpClient.OpenRead(remoteFilePath);
                return stream;
            }
            catch (TimeoutException ex)
            {
                System.Diagnostics.Debug.WriteLine($"FTPストリーム読み込みタイムアウトエラー: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FTPストリーム読み込みエラー: {ex.Message}");
                return null;
            }
        }

        #region ----------- Dispose -----------
        /// <summary>
        /// リソースを解放する
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    try
                    {
                        if (_ftpClient?.IsConnected == true)
                        {
                            _ftpClient.Disconnect();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"FTP切断エラー (Dispose): {ex.Message}");
                    }
                    finally
                    {
                        _ftpClient?.Dispose();
                    }
                }
                _disposed = true;
            }
        }

        #endregion
    }
}
