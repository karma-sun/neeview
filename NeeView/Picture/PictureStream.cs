// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using Susie;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// 名前とセットのストリーム
    /// </summary>
    public class NamedStream : IDisposable
    {
        public Stream Stream { get; set; }
        public string Name { get; set; }

        public NamedStream(Stream stream, string name)
        {
            this.Stream = stream;
            this.Name = name;
        }

        public void Dispose()
        {
            this.Stream?.Dispose();
        }
    }

    /// <summary>
    /// PictureStream Interface
    /// </summary>
    public interface IPictureStream
    {
        /// <summary>
        /// 画像ストリームを取得
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        NamedStream Create(ArchiveEntry entry);
    }

    /// <summary>
    /// 画像をストリームで取得
    /// </summary>
    public class PictureStream : BindableBase, IPictureStream
    {
        #region Fields

        //
        private DefaultPictureStream _default = new DefaultPictureStream();

        //
        private SusiePictureStream _susie = new SusiePictureStream();

        //
        private List<IPictureStream> _orderList;

        #endregion

        #region Constructors

        // コンストラクタ
        public PictureStream()
        {
            SusieContext.Current.AddPropertyChanged(nameof(SusieContext.IsEnabled),
                (s, e) => UpdateOrderList());
            SusieContext.Current.AddPropertyChanged(nameof(SusieContext.IsFirstOrderSusieImage),
                (s, e) => UpdateOrderList());

            UpdateOrderList();
        }

        #endregion

        #region Methods

        // 適用する画像ストリームの順番を更新
        private void UpdateOrderList()
        {
            var susieProfile = SusieContext.Current;

            if (!susieProfile.IsEnabled)
            {
                _orderList = new List<IPictureStream>() { _default };
            }
            else if (susieProfile.IsFirstOrderSusieImage)
            {
                _orderList = new List<IPictureStream>() { _susie, _default };
            }
            else
            {
                _orderList = new List<IPictureStream>() { _default, _susie };
            }
        }

        // 画像ストリームを取得
        public NamedStream Create(ArchiveEntry entry)
        {
            Exception exception = null;

            foreach (var pictureStream in _orderList)
            {
                try
                {
                    var stream = pictureStream.Create(entry);
                    if (stream != null) return stream;
                }
                catch (OutOfMemoryException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"{e.Message}\nat '{entry.EntryName}' by {pictureStream}");
                    exception = e;
                }
            }

            throw exception ?? new IOException("画像の読み込みに失敗しました。");
        }

        #endregion
    }

    /// <summary>
    /// 通常画像をストリームで取得
    /// </summary>
    class DefaultPictureStream : IPictureStream
    {
        public NamedStream Create(ArchiveEntry entry)
        {
            return new NamedStream(entry.OpenEntry(), null);
        }
    }

    /// <summary>
    /// Susie画像をストリームで取得
    /// </summary>
    class SusiePictureStream : IPictureStream
    {
        // 画像ストリーム取得。
        // 対象に応じてファイルからの読み込みかメモリからの読み込みかを変更
        public NamedStream Create(ArchiveEntry entry)
        {
            if (entry.IsFileSystem)
            {
                return Create(entry.GetFileSystemPath(), entry);
            }
            else
            {
                using (var stream = entry.OpenEntry())
                {
                    return Create(stream, entry);
                }
            }
        }

        // Bitmap読み込み(stream)
        private NamedStream Create(Stream stream, ArchiveEntry entry)
        {
            byte[] buff;
            if (stream is MemoryStream)
            {
                buff = ((MemoryStream)stream).GetBuffer();
            }
            else
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    buff = ms.GetBuffer();
                }
            }

            SusiePlugin susiePlugin = null;

            var bytes = SusieContext.Current.Susie?.GetPicture(entry.EntryName, buff, true, out susiePlugin);

            return new NamedStream(new MemoryStream(bytes), susiePlugin?.ToString());
        }


        // Bitmap読み込み(ファイル版)
        private NamedStream Create(string fileName, ArchiveEntry entry)
        {
            SusiePlugin susiePlugin = null;

            var bytes = SusieContext.Current.Susie?.GetPictureFromFile(fileName, true, out susiePlugin);

            return new NamedStream(new MemoryStream(bytes), susiePlugin?.ToString());
        }

    }
}
