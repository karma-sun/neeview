// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// BitmapLoader例外
    /// 複数の例外をまとめる
    /// </summary>
    public class BitmapLoaderException : AggregateException
    {
        public BitmapLoaderException()
        {
        }

        public BitmapLoaderException(string message)
            : base(message)
        {
        }

        public BitmapLoaderException(IEnumerable<Exception> inners)
            : base(inners)
        {
        }

        public BitmapLoaderException(string message, IEnumerable<Exception> inners)
            : base(message, inners)
        {
        }

        public override string Message
        {
            get
            {
                if (InnerExceptions != null && InnerExceptions.Count > 0)
                {
                    return string.Join("\n", this.InnerExceptions.Select(e => e.Message));
                }
                else
                {
                    return base.Message;
                }
            }
        }
    }

    /// <summary>
    /// BitmapLoader
    /// </summary>
    public class BitmapLoader
    {
        private ArchiveEntry _entry;
        private bool _allowExifOrientation;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="allowExifOrientation"></param>
        public BitmapLoader(ArchiveEntry entry, bool allowExifOrientation)
        {
            _entry = entry;
            _allowExifOrientation = allowExifOrientation;
        }

        /// <summary>
        /// 読込
        /// </summary>
        /// <returns></returns>
        public BitmapContent Load()
        {
            try
            {
                return LoadCore();
            }
            catch (OutOfMemoryException)
            {
                // nop.
            }

            // OutOfMemoryの場合はGC後に再実行
            Debug.WriteLine("!!!! GC !!!! by empty memory");
            MemoryControl.Current.GarbageCollect(true);
            return LoadCore();
        }


        /// <summary>
        /// ローダーの優先順位に従って読込
        /// </summary>
        /// <returns></returns>
        private BitmapContent LoadCore()
        {
            var exceptions = new List<Exception>();

            foreach (var loaderType in ModelContext.BitmapLoaderManager.OrderList)
            {
                try
                {
                    var bitmapLoader = BitmapLoaderManager.Create(loaderType);
                    if (!bitmapLoader.IsEnabled) continue;

                    BitmapContent bmp;
                    if (_entry.IsFileSystem)
                    {
                        bmp = bitmapLoader.LoadFromFile(_entry.GetFileSystemPath(), _entry, _allowExifOrientation);
                    }
                    else
                    {
                        using (var stream = _entry.OpenEntry())
                        {
                            bmp = bitmapLoader.Load(stream, _entry, _allowExifOrientation);
                        }
                    }

                    if (bmp != null)
                    {
                        if (bmp.Info != null) bmp.Info.Archiver = _entry.Archiver.ToString();
                        return bmp;
                    }
                }
                catch (OutOfMemoryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"{e.Message}\nat '{_entry.EntryName}' by {loaderType}");
                    exceptions.Add(e);
                }
            }

            if (!exceptions.Any()) exceptions.Add(new IOException("画像の読み込みに失敗しました。"));

            throw new BitmapLoaderException(exceptions);
        }


        /// <summary>
        /// ローダー非同期(予定)
        /// TODO: 画像読み込みとキャンセル。どうする？
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<BitmapContent> LoadAsync(CancellationToken token)
        {
            try
            {
                return LoadCore();
            }
            catch (OutOfMemoryException)
            {
            }

            // OutOfMemoryの場合はGC後に再実行
            Debug.WriteLine("!!!! GC !!!! by empty memory");
            MemoryControl.Current.GarbageCollect(true);

            await Task.Yield(); // ##

            return LoadCore();
        }
    }
}
