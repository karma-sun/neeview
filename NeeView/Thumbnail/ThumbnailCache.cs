using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// サムネイルキャッシュ：ヘッダ
    /// </summary>
    public class ThumbnailCacheHeader
    {
        /// <summary>
        /// キーとなるハッシュ値
        /// </summary>
        public string Hash { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="name"></param>
        /// <param name="length"></param>
        /// <param name="lastUpdateTime"></param>
        /// <param name="appendix"></param>
        public ThumbnailCacheHeader(string name, long length, DateTime lastUpdateTime, string appendix)
        {
            string source = $"thumb://{name}:{length}:{lastUpdateTime}:{appendix}";
            ////Debug.WriteLine($"Cache: {source}");
            this.Hash = GetSha256(source);
        }

        /// <summary>
        /// 文字列から SHA256 のハッシュ値を取得
        /// </summary>
        private string GetSha256(string target)
        {
            SHA256 mySHA256 = SHA256Managed.Create();
            byte[] byteValue = Encoding.UTF8.GetBytes(target);
            byte[] hash = mySHA256.ComputeHash(byteValue);

            StringBuilder buf = new StringBuilder();

            for (int i = 0; i < hash.Length; i++)
            {
                buf.AppendFormat("{0:x2}", hash[i]);
            }

            return buf.ToString();
        }
    }


    /// <summary>
    /// サムネイルキャッシュ.
    /// SQLiteを使用しています.
    /// </summary>
    public class ThumbnailCache : IDisposable
    {
        public const string _format = "1.20";

        static ThumbnailCache() => Current = new ThumbnailCache();
        public static ThumbnailCache Current { get; }


        private const string FileName = "Cache.db";
        private string _filename;
        private SQLiteConnection _connection;
        private object _lock = new object();

        private Dictionary<string, byte[]> _saveQueue;
        private DelayAction _delaySaveQueue;
        private object _lockSaveQueue = new object();


        private ThumbnailCache()
        {
            _saveQueue = new Dictionary<string, byte[]>();
            _delaySaveQueue = new DelayAction(App.Current.Dispatcher, TimeSpan.FromSeconds(0.5), SaveQueue, TimeSpan.FromSeconds(2.0));
        }


        /// <summary>
        /// キャッシュ有効フラグ
        /// </summary>
        public bool IsEnabled => ThumbnailProfile.Current.IsCacheEnabled;

        /// <summary>
        /// キャッシュファイルの場所
        /// </summary>
        public string CacheFolderPath { get; private set; }

        /// <summary>
        /// キャッシュファイルの場所の指定
        /// </summary>
        /// <param name="path"></param>
        public string SetDirectory(string path)
        {
            CacheFolderPath = path ?? Config.Current.LocalApplicationDataPath;

            if (path != Config.Current.LocalApplicationDataPath)
            {
                if (!Directory.Exists(path))
                {
                    ToastService.Current.Show(new Toast(string.Format(Properties.Resources.NotifyCacheErrorDirectoryNotFound, path), Properties.Resources.NotifyCacheErrorTitle, ToastIcon.Error));
                    CacheFolderPath = Config.Current.LocalApplicationDataPath;
                }
            }

            _filename = Path.Combine(CacheFolderPath, FileName);

            return CacheFolderPath;
        }

        /// <summary>
        /// キャッシュファイルの場所の変更
        /// </summary>
        public void MoveDirectory(string sourcePath)
        {
            var sourceFileName = Path.Combine(sourcePath ?? Config.Current.LocalApplicationDataPath, FileName);

            if (_filename == sourceFileName)
            {
                return;
            }

            File.Move(sourceFileName, _filename);
        }

        /// <summary>
        /// DBを開く
        /// </summary>
        /// <param name="filename"></param>
        internal void Open()
        {
            lock (_lock)
            {
                if (_connection != null) return;

                _connection = new SQLiteConnection($"Data Source={_filename}");
                _connection.Open();

                CreateTable();
            }
        }

        /// <summary>
        /// DBを閉じる
        /// </summary>
        internal void Close()
        {
            lock (_lock)
            {
                if (_connection != null)
                {
                    _connection.Close();
                    _connection.Dispose();
                    _connection = null;
                }
            }
        }



        /// <summary>
        /// 初期化：テーブルの作成
        /// </summary>
        private void CreateTable()
        {
            using (SQLiteCommand command = _connection.CreateCommand())
            {
                // database property
                command.CommandText = "CREATE TABLE IF NOT EXISTS property ("
                            + "key TEXT NOT NULL PRIMARY KEY,"
                            + "value TEXT"
                            + ")";
                command.ExecuteNonQuery();

                // property.format
                SavePropertyIfNotExist("format", _format);

                // thumbnails 
                command.CommandText = "CREATE TABLE IF NOT EXISTS thumbs ("
                            + "key TEXT NOT NULL PRIMARY KEY,"
                            + "value BLOB"
                            + ")";
                command.ExecuteNonQuery();
            }
        }



        /// <summary>
        /// データ削除
        /// </summary>
        internal void Remove()
        {
            Close();

            if (System.IO.File.Exists(_filename))
            {
                System.IO.File.Delete(_filename);
            }
        }

        /// <summary>
        /// プロパティの保存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        internal void SaveProperty(string key, string value)
        {
            using (SQLiteCommand command = _connection.CreateCommand())
            {
                command.CommandText = $"REPLACE INTO property (key, value) VALUES (@key, @value)";
                command.Parameters.Add(new SQLiteParameter("@key", key));
                command.Parameters.Add(new SQLiteParameter("@value", value));
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// プロパティの保存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        internal void SavePropertyIfNotExist(string key, string value)
        {
            using (SQLiteCommand command = _connection.CreateCommand())
            {
                command.CommandText = "INSERT OR IGNORE INTO property (key, value) VALUES(@key, @value)";
                command.Parameters.Add(new SQLiteParameter("@key", key));
                command.Parameters.Add(new SQLiteParameter("@value", value));
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// プロパティの読込
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal async Task<string> LoadPropertyAsync(string key)
        {
            using (SQLiteCommand command = _connection.CreateCommand())
            {
                command.CommandText = $"SELECT value FROM property WHERE key = @key";
                command.Parameters.Add(new SQLiteParameter("@key", key));

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        return reader.GetString(0);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// サムネイルの保存
        /// </summary>
        /// <param name="header"></param>
        /// <param name="data"></param>
        internal void Save(ThumbnailCacheHeader header, byte[] data)
        {
            if (!IsEnabled) return;

            Open();

            using (SQLiteCommand command = _connection.CreateCommand())
            {
                command.CommandText = $"REPLACE INTO thumbs (key, value) VALUES (@key, @value)";
                command.Parameters.Add(new SQLiteParameter("@key", header.Hash));
                command.Parameters.Add(new SQLiteParameter("@value", data));
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// サムネイルの読込
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        internal byte[] Load(ThumbnailCacheHeader header)
        {
            if (!IsEnabled) return null;

            Open();

            var key = header.Hash;

            using (SQLiteCommand command = _connection.CreateCommand())
            {
                command.CommandText = $"SELECT value FROM thumbs WHERE key = @key";
                command.Parameters.Add(new SQLiteParameter("@key", key));

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader["value"] as byte[];
                    }
                }
            }

            // SaveQueueからも探す
            lock (_lockSaveQueue)
            {
                if (_saveQueue.TryGetValue(key, out byte[] data))
                {
                    return data;
                }
            }

            return null;
        }

        /// <summary>
        /// サムネイルの保存予約
        /// </summary>
        /// <param name="header"></param>
        /// <param name="data"></param>
        internal void EntrySaveQueue(ThumbnailCacheHeader header, byte[] data)
        {
            if (!IsEnabled) return;

            lock (_lockSaveQueue)
            {
                _saveQueue[header.Hash] = data;
            }

            _delaySaveQueue.Request();
        }

        /// <summary>
        /// サムネイルの保存予約実行
        /// </summary>
        private void SaveQueue()
        {
            if (!IsEnabled) return;

            var queue = _saveQueue;
            lock (_lockSaveQueue)
            {
                _saveQueue = new Dictionary<string, byte[]>();
            }

            Debug.WriteLine($"ThumbnailCache.SaveQueue({queue.Count})..");

            Open();

            using (var transaction = _connection.BeginTransaction())
            {
                using (SQLiteCommand command = _connection.CreateCommand())
                {
                    foreach (var item in queue)
                    {
                        ////Debug.WriteLine($"ThumbnailCache.Save: {item.Key.Substring(0, 8)}");
                        command.CommandText = $"REPLACE INTO thumbs (key, value) VALUES (@key, @value)";
                        command.Parameters.Add(new SQLiteParameter("@key", item.Key));
                        command.Parameters.Add(new SQLiteParameter("@value", item.Value));
                        command.ExecuteNonQuery();
                    }
                }
                transaction.Commit();
            }
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _delaySaveQueue.Flush();
                    _delaySaveQueue.Dispose();

                    Close();
                    _connection?.Dispose();
                    _connection = null;
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
