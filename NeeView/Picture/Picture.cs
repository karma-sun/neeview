// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    //
    public class Picture : BindableBase
    {
        private PictureSourceBase _source;

        //
        public Picture(ArchiveEntry entry)
        {
            _source = PictureSourceFactory.Create(entry);
        }

        //
        public void Load()
        {
            var pictureFile = PictureLoaderManager.Current.Load(_source.ArchiveEntry);

            _source.RawData = pictureFile.Raw;
            _source.PictureInfo = pictureFile.PictureInfo;

            // ##
            this.BitmapSource = pictureFile.BitmapSource;

            RaisePropertyChanged(nameof(PictureInfo));
        }

        //
        public async Task LoadAsync()
        {
            await Task.Run(() => Load());
        }

        [Obsolete]
        public PictureInfo PictureInfo => _source.PictureInfo;

        // こちらを使う
        public PictureInfo PictureInfo2 { get; set; }


        /// <summary>
        /// BitmapSource property.
        /// </summary>
        private BitmapSource _bitmapSource;
        public BitmapSource BitmapSource
        {
            get { return _bitmapSource; }
            set { if (_bitmapSource != value) { _bitmapSource = value; RaisePropertyChanged(); } }
        }


        private Size _size = Size.Empty;


        // TODO: OutOfMemory時のリトライ
        public BitmapSource CreateBitmap(Size size)
        {
            if (_source.PictureInfo == null)
            {
                Load();
            }

            if (_bitmapSource != null && size == _size) return _bitmapSource;

            size = _source.CreateFixedSize(size);
            if (_bitmapSource != null && size == _size) return _bitmapSource;

            this.BitmapSource = _source.CreateBitmap(size);
            _size = size;

            if (!_source.PictureInfo.IsPixelInfoEnabled)
            {
                try
                {
                    _source.PictureInfo.SetPixelInfo(_bitmapSource);
                    RaisePropertyChanged(nameof(PictureInfo));
                }
                catch (Exception)
                {
                    // この例外では停止させない
                }
            }

            return _bitmapSource;
        }

        //
        public async Task<BitmapSource> CreateBitmapAsync(Size size)
        {
            return await Task.Run(() => CreateBitmap(size));
        }


        //
        public void ClearBitmap()
        {
            this.BitmapSource = null;
            _size = Size.Empty;
        }


        private Size? _request;
        private bool _isBusy;
        private object _lock = new object();

        //
        public void RequestCreateBitmap(Size size)
        {
            lock (_lock)
            {
                _request = size;
            }

            if (!_isBusy)
            {
                _isBusy = true;
                Task.Run(() => CreateBitmapTask());
            }
        }

        //
        public void CreateBitmapTask()
        {
            try
            {
                while (_request != null)
                {
                    var size = (Size)_request;
                    lock (_lock)
                    {
                        _request = null;
                    }
                    CreateBitmap(size);
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
            }
            finally
            {
                _isBusy = false;
            }
        }

    }

}
