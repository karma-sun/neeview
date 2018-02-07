using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// サムネイル寿命管理.
    /// サムネイルを使用するタイミングで随時追加。
    /// 容量を越えたら古いものからクリア処理を行う。
    /// </summary>
    public class ThumbnailPool
    {
        /// <summary>
        /// 管理ユニット
        /// </summary>
        public class ThumbnailUnit
        {
            /// <summary>
            /// サムネイル
            /// </summary>
            private Thumbnail _thumbnail;

            /// <summary>
            /// 寿命シリアル番号
            /// </summary>
            private int _lifeSerial;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            /// <param name="thumbnail"></param>
            public ThumbnailUnit(Thumbnail thumbnail)
            {
                _thumbnail = thumbnail;
                _lifeSerial = thumbnail.LifeSerial;
            }

            /// <summary>
            /// 有効判定.
            /// 寿命シリアル番号が一致すれば有効
            /// </summary>
            public bool IsValid => _lifeSerial == _thumbnail.LifeSerial;

            /// <summary>
            /// サムネイルクリア
            /// </summary>
            public void Clear()
            {
                //Debug.WriteLine($"TC: {_lifeSerial}");
                _thumbnail.Clear();
            }
        }


        /// <summary>
        /// サムネイルユニット群
        /// </summary>
        private List<ThumbnailUnit> _collection = new List<ThumbnailUnit>();

        /// <summary>
        /// 寿命シリアル番号生成用
        /// </summary>
        private int _serial;

        /// <summary>
        /// サムネイル保証数
        /// </summary>
        public virtual int Limit { get; } = 1000;

        /// <summary>
        /// 廃棄処理 part1 許容値
        /// </summary>
        private int _tolerance1 => (Limit * 150 / 100); // 150%

        /// <summary>
        /// 廃棄処理 part2 許容値
        /// </summary>
        private int _tolerance2 => (Limit * 120 / 100); // 120%

        //
        private object _lock = new object();

        /// <summary>
        /// 管理にサムネイル登録
        /// 「使用する」タイミングで随時追加
        /// </summary>
        /// <param name="thumbnail"></param>
        public void Add(Thumbnail thumbnail)
        {
            lock (_lock)
            {
                _serial = (_serial + 1) & 0x7fffffff;
                thumbnail.LifeSerial = _serial;

                _collection.Add(new ThumbnailUnit(thumbnail));
                Cleanup();
            }
        }

        /// <summary>
        /// 廃棄処理
        /// </summary>
        /// <returns></returns>
        private bool Cleanup()
        {
            // 1st path.
            if (_collection.Count < _tolerance1) return false;

            //Debug.WriteLine($"TP Clean: 1st... {_collection.Count}");

            var count = _collection.Count;

            _collection = _collection
                .Where(e => e.IsValid)
                .ToList();

            Debug.WriteLine($"ThumbnailPool Clean: Level.1: {count} -> {_collection.Count}: No.{_serial}");

            // 2nd path.
            if (_collection.Count < _tolerance2) return false;

            int erase = _collection.Count - Limit;

            _collection
                .Take(erase)
                .ForEach(e => e.Clear());

            _collection = _collection
                .Skip(erase)
                .ToList();

            Debug.WriteLine($"ThumbnailPool Clean: Level.2: {_collection.Count}");

            return true;
        }

    }

}
