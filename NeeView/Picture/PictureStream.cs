﻿using NeeLaboratory.ComponentModel;
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
    public sealed class NamedStream : IDisposable
    {
        public Stream Stream { get; set; }
        public string Name { get; set; }
        public byte[] RawData { get; set; }

        public NamedStream(Stream stream, string name, byte[] rawData)
        {
            this.Stream = stream;
            this.Name = name;
            this.RawData = rawData;
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
            Config.Current.Susie.AddPropertyChanged(nameof(SusieConfig.IsEnabled),
                (s, e) => UpdateOrderList());
            Config.Current.Susie.AddPropertyChanged(nameof(SusieConfig.IsFirstOrderSusieImage),
                (s, e) => UpdateOrderList());

            UpdateOrderList();
        }

        #endregion

        #region Methods

        // 適用する画像ストリームの順番を更新
        private void UpdateOrderList()
        {
            if (!Config.Current.Susie.IsEnabled)
            {
                _orderList = new List<IPictureStream>() { _default };
            }
            else if (Config.Current.Susie.IsFirstOrderSusieImage)
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

            throw exception ?? new IOException(Properties.Resources.ImageLoadFailedException_Message);
        }

        #endregion
    }

}
