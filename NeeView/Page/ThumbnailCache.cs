// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
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
        public ThumbnailCacheHeader(string name, long length, DateTime? lastUpdateTime, string appendix)
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

        /// <summary>
        /// 現在のシステムオブジェクト
        /// </summary>
        public static ThumbnailCache Current { get; private set; }

        /// <summary>
        /// constructor
        /// </summary>
        public ThumbnailCache()
        {
            Current = this;
        }

        // Connection
        private SQLiteConnection _connection;

        /// <summary>
        /// キャッシュ有効フラグ
        /// </summary>
        public bool IsEnabled => Preference.Current.thumbnail_cache;

        /// <summary>
        /// データベースファイル名
        /// </summary>
        private string _filename { get; } = System.IO.Path.Combine(App.Config.LocalApplicationDataPath, "Cache.db");


        //
        public object _lock = new object();

        /// <summary>
        /// DBを開く
        /// </summary>
        /// <param name="filename"></param>
        internal void Open()
        {
            if (_connection != null) return;

            _connection = new SQLiteConnection($"Data Source={_filename}");
            _connection.Open();

            CreateTable();
        }

        /// <summary>
        /// DBを閉じる
        /// </summary>
        internal void Close()
        {
            if (_connection != null)
            {
                _connection.Close();
                _connection.Dispose();
                _connection = null;
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

            if (_connection == null) Open();

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

            if (_connection == null) Open();

            using (SQLiteCommand command = _connection.CreateCommand())
            {
                command.CommandText = $"SELECT value FROM thumbs WHERE key = @key";
                command.Parameters.Add(new SQLiteParameter("@key", header.Hash));

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader["value"] as byte[];
                    }
                }
            }

            return null;
        }


        #region IDisposable Support
        private bool _disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Close();
                    _connection?.Dispose();
                    _connection = null;
                }

                _disposedValue = true;
            }
        }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
