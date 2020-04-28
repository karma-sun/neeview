using NeeView.Data;
using NeeView.Threading;
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
        public ThumbnailCacheHeader(string name, long length, string appendix, int generateHasn)
        {
            Key = appendix != null ? name + ":" + appendix : name;
            Size = length;
            AccessTime = DateTime.Now;
            GenerateHash = generateHasn;
        }

        /// <summary>
        /// キャッシュのキー(ファイルパス)
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// ファイルサイズ
        /// </summary>
        public long Size { get; private set; }

        /// <summary>
        /// アクセス日付
        /// </summary>
        public DateTime AccessTime { get; private set; }

        /// <summary>
        /// サムネイル画像生成パラメータ一致チェック用ハッシュ
        /// </summary>
        public int GenerateHash { get; private set; }
    }


    /// <summary>
    /// 保存キュー用
    /// </summary>
    public class ThumbnailCacheItem
    {
        public ThumbnailCacheItem(ThumbnailCacheHeader header, byte[] body)
        {
            Header = header;
            Body = body;
        }

        public ThumbnailCacheHeader Header { get; set; }
        public byte[] Body { get; set; }
    }


    /// <summary>
    /// サムネイルキャッシュ.
    /// SQLiteを使用しています.
    /// </summary>
    public class ThumbnailCache : IDisposable
    {
        public const string _format = "2.0";

        static ThumbnailCache() => Current = new ThumbnailCache();
        public static ThumbnailCache Current { get; }


        private const string FileName = "Cache.db";
        private string _filename;
        private SQLiteConnection _connection;
        private object _lock = new object();

        private Dictionary<string, ThumbnailCacheItem> _saveQueue;
        private Dictionary<string, ThumbnailCacheHeader> _updateQueue;
        private DelayAction _delaySaveQueue;
        private object _lockSaveQueue = new object();


        private ThumbnailCache()
        {
            _saveQueue = new Dictionary<string, ThumbnailCacheItem>();
            _updateQueue = new Dictionary<string, ThumbnailCacheHeader>();
            _delaySaveQueue = new DelayAction(App.Current.Dispatcher, TimeSpan.FromSeconds(0.5), SaveQueue, TimeSpan.FromSeconds(2.0));
        }


        /// <summary>
        /// キャッシュ有効フラグ
        /// </summary>
        public bool IsEnabled => Config.Current.Thumbnail.IsCacheEnabled;

        /// <summary>
        /// キャッシュファイルの場所
        /// </summary>
        public string CacheFolderPath { get; private set; }

        /// <summary>
        /// キャッシュファイルの場所(既定)
        /// </summary>
        public static string CacheFolderPathDefault => Environment.LocalApplicationDataPath;


        /// <summary>
        /// キャッシュファイルの場所の指定
        /// </summary>
        /// <param name="path"></param>
        public string SetDirectory(string path)
        {
            CacheFolderPath = path ?? CacheFolderPathDefault;

            if (CacheFolderPath != CacheFolderPathDefault)
            {
                if (!Directory.Exists(CacheFolderPath))
                {
                    ToastService.Current.Show(new Toast(string.Format(Properties.Resources.NotifyCacheErrorDirectoryNotFound, CacheFolderPath), Properties.Resources.NotifyCacheErrorTitle, ToastIcon.Error));
                    CacheFolderPath = CacheFolderPathDefault;
                }
            }

            _filename = Path.Combine(CacheFolderPath, FileName);

            return CacheFolderPath;
        }

        /// <summary>
        /// DBファイルサイズを取得
        /// </summary>
        public long GetCaheDatabaseSize()
        {
            if (_filename == null) throw new InvalidOperationException();

            var fileinfo = new FileInfo(_filename);
            if (fileinfo.Exists)
            {
                return fileinfo.Length;
            }
            else
            {
                return 0L;
            }
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

                OpenInner();

                // if wrong format, then recreate
                if (!IsSupportFormat())
                {
                    Debug.WriteLine($"ThumbnailCache.ReCreate!!");
                    Remove();
                    OpenInner();
                }

                CreateThumbsTable();
            }

            void OpenInner()
            {
                if (_connection != null) return;

                _connection = new SQLiteConnection($"Data Source={_filename}");
                _connection.Open();

                InitializePragma();
                CreatePropertyTable();
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
        /// フォーマットチェック
        /// </summary>
        private bool IsSupportFormat()
        {
            var format = LoadProperty("format");

            var result = format == null || format == _format;
            Debug.WriteLine($"ThumbnailCache.Format: {format}: {result}");

            return result;
        }


        /// <summary>
        /// 初期化：PRAGMA設定
        /// </summary>
        private void InitializePragma()
        {
            using (SQLiteCommand command = _connection.CreateCommand())
            {
                command.CommandText = "PRAGMA auto_vacuum = full";
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 初期化：プロパティテーブルの作成
        /// </summary>
        private void CreatePropertyTable()
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
            }
        }

        /// <summary>
        /// 初期化：データテーブルの作成
        /// </summary>
        private void CreateThumbsTable()
        {
            using (SQLiteCommand command = _connection.CreateCommand())
            {
                // thumbnails 
                command.CommandText = "CREATE TABLE IF NOT EXISTS thumbs ("
                            + "key TEXT NOT NULL PRIMARY KEY,"
                            + "size INTEGER,"
                            + "date DATETIME,"
                            + "ghash INTEGER,"
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
        internal string LoadProperty(string key)
        {
            using (SQLiteCommand command = _connection.CreateCommand())
            {
                command.CommandText = "SELECT value FROM property WHERE key = @key";
                command.Parameters.Add(new SQLiteParameter("@key", key));

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return reader.GetString(0);
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// 古いサムネイルを削除
        /// </summary>
        /// <param name=""></param>
        internal void Delete(TimeSpan limitTime)
        {
            if (!IsEnabled) return;

            Open();

            var limitDateTime = DateTime.Now - limitTime;
            Debug.WriteLine($"ThumbnailCache.Delete: before {limitDateTime}");

            using (SQLiteCommand command = _connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM thumbs WHERE date < @date";
                command.Parameters.Add(new SQLiteParameter("@date", limitDateTime));
                int count = command.ExecuteNonQuery();

                Debug.WriteLine($"ThumbnailCache.Delete: {count}");
            }
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
                Save(command, header, data);
            }
        }


        /// <summary>
        /// サムネイルの保存
        /// </summary>
        /// <param name="command"></param>
        /// <param name="header"></param>
        /// <param name="data"></param>
        private void Save(SQLiteCommand command, ThumbnailCacheHeader header, byte[] data)
        {
            command.CommandText = "REPLACE INTO thumbs (key, size, date, ghash, value) VALUES (@key, @size, @date, @ghash, @value)";
            command.Parameters.Add(new SQLiteParameter("@key", header.Key));
            command.Parameters.Add(new SQLiteParameter("@size", header.Size));
            command.Parameters.Add(new SQLiteParameter("@date", header.AccessTime));
            command.Parameters.Add(new SQLiteParameter("@ghash", header.GenerateHash));
            command.Parameters.Add(new SQLiteParameter("@value", data));
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// アクセス日時を更新
        /// </summary>
        /// <param name="command"></param>
        /// <param name="header"></param>
        private void UpdateDate(SQLiteCommand command, ThumbnailCacheHeader header)
        {
            command.CommandText = "UPDATE thumbs SET date = @date WHERE key = @key";
            command.Parameters.Add(new SQLiteParameter("@key", header.Key));
            command.Parameters.Add(new SQLiteParameter("@date", header.AccessTime));
            command.ExecuteNonQuery();
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

            var key = header.Key;
            var size = header.Size;
            var ghash = header.GenerateHash;

            using (SQLiteCommand command = _connection.CreateCommand())
            {
                command.CommandText = "SELECT value, date FROM thumbs WHERE key = @key AND size = @size AND ghash = @ghash";
                command.Parameters.Add(new SQLiteParameter("@key", key));
                command.Parameters.Add(new SQLiteParameter("@size", size));
                command.Parameters.Add(new SQLiteParameter("@ghash", ghash));

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        if (reader["date"] is DateTime date)
                        {
                            // 1日以上古い場合は更新する
                            if ((header.AccessTime - date).TotalDays > 1.0)
                            {
                                EntryUpdateQueue(header);
                            }
                        }

                        return reader["value"] as byte[];
                    }
                }
            }

            // SaveQueueからも探す
            lock (_lockSaveQueue)
            {
                if (_saveQueue.TryGetValue(key, out ThumbnailCacheItem item))
                {
                    return item.Body;
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
                _saveQueue[header.Key] = new ThumbnailCacheItem(header, data);
            }

            _delaySaveQueue.Request();
        }

        /// <summary>
        /// 日付更新の予約
        /// </summary>
        /// <param name="header"></param>
        internal void EntryUpdateQueue(ThumbnailCacheHeader header)
        {
            if (!IsEnabled) return;

            lock (_lockSaveQueue)
            {
                _updateQueue[header.Key] = header;
            }

            _delaySaveQueue.Request();
        }

        /// <summary>
        /// サムネイルの保存予約実行
        /// </summary>
        private void SaveQueue()
        {
            if (!IsEnabled) return;

            var saveQueue = _saveQueue;
            var updateQueue = _updateQueue;
            lock (_lockSaveQueue)
            {
                _saveQueue = new Dictionary<string, ThumbnailCacheItem>();
                _updateQueue = new Dictionary<string, ThumbnailCacheHeader>();
            }

            Debug.WriteLine($"ThumbnailCache.Save: {saveQueue.Count},{updateQueue.Count} ..");

            Open();

            using (var transaction = _connection.BeginTransaction())
            {
                using (SQLiteCommand command = _connection.CreateCommand())
                {
                    foreach (var item in saveQueue.Values)
                    {
                        ////Debug.WriteLine($"ThumbnailCache.Save: {item.Header.Key}");
                        Save(command, item.Header, item.Body);
                    }

                    foreach (var item in updateQueue.Values)
                    {
                        ////Debug.WriteLine($"ThumbnailCache.Update: {item.Key}");
                        UpdateDate(command, item);
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

                    if (Config.Current.Thumbnail.CacheLimitSpan != default)
                    {
                        Delete(Config.Current.Thumbnail.CacheLimitSpan);
                    }

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
