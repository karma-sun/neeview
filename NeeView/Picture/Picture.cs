﻿using NeeLaboratory.ComponentModel;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// エントリに対応する表示画像
    /// </summary>
    public class Picture : BindableBase
    {
        #region Fields

        /// <summary>
        /// リサイズパラメータのハッシュ。
        /// リサイズが必要かの判定に使用される
        /// </summary>
        private int _resizeHashCode;

        /// <summary>
        /// このPictureが使用されなくなったときのキャンセル通知
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// ロックオブジェクト
        /// </summary>
        private object _lock = new object();

        #endregion

        #region Constructors

        public Picture(PictureSource source)
        {
            PictureSource = source;

            _resizeHashCode = GetEnvironmentoHashCode();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        #endregion

        #region Properties

        public PictureSource PictureSource { get; private set; }


        /// <summary>
        /// 画像情報
        /// </summary>
        public PictureInfo PictureInfo => PictureSource.PictureInfo;

        /// <summary>
        /// 表示する画像
        /// </summary>
        private BitmapSource _bitmapSource;
        public BitmapSource BitmapSource
        {
            get { return _bitmapSource; }
            set { if (_bitmapSource != value) { _bitmapSource = value; RaisePropertyChanged(); } }
        }

        #endregion

        #region Methods

        public long GetMemorySize()
        {
            return _bitmapSource != null ? (_bitmapSource.Format.BitsPerPixel / 8) * _bitmapSource.PixelWidth * _bitmapSource.PixelHeight : 0;
        }

        /// <summary>
        /// このPictureの使用停止
        /// </summary>
        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        // 画像生成に影響する設定のハッシュ値取得
        private int GetEnvironmentoHashCode()
        {
            return ImageFilter.Current.GetHashCode() ^ PictureProfile.Current.CustomSize.GetHashCodde();
        }

        // Bitmapが同じサイズであるか判定
        private bool IsEqualBitmapSizeMaybe(Size size, bool keepAspectRatio)
        {
            if (this.BitmapSource == null) return false;

            size = size.IsEmpty ? this.PictureInfo.Size : size;

            const double margin = 1.1;
            if (keepAspectRatio)
            {
                // アスペクト比固定のため、PixelHeightのみで判定
                return Math.Abs(size.Height - this.BitmapSource.PixelHeight) < margin;
            }
            else
            {
                return Math.Abs(size.Height - this.BitmapSource.PixelHeight) < margin && Math.Abs(size.Width - this.BitmapSource.PixelWidth) < margin;
            }
        }

        // 初期化
        // TODO: 初回作成だが、これでいいのか？
        public void Initialize(CancellationToken token)
        {
            BitmapSource = PictureSource.CreateBitmapSource(PictureInfo.Size, new BitmapCreateSetting(), token);
        }

        // リサイズ
        public bool Resize(Size size)
        {
            if (this.BitmapSource == null) return false;

            size = size.IsEmpty ? this.PictureInfo.Size : size;

            // TODO: PictureSourceレベルで処理
            if (PictureSource is PdfPictureSource)
            {
                size = PdfArchiverProfile.Current.CreateFixedSize(size);
            }
            else
            {
                var maxWixth = Math.Max(this.PictureInfo.Size.Width, PictureProfile.Current.MaximumSize.Width);
                var maxHeight = Math.Max(this.PictureInfo.Size.Height, PictureProfile.Current.MaximumSize.Height);
                var maxSize = new Size(maxWixth, maxHeight);
                size = size.Limit(maxSize);
            }

            // 規定サイズ判定
            if (!this.PictureInfo.IsLimited && size.IsEqualMaybe(this.PictureInfo.Size))
            {
                size = Size.Empty;
            }

            // アスペクト比固定?
            var cutomSize = PictureProfile.Current.CustomSize;
            var keepAspectRatio = size.IsEmpty || !cutomSize.IsEnabled || cutomSize.IsUniformed;

            int filterHashCode = GetEnvironmentoHashCode();
            bool isDartyResizeParameter = _resizeHashCode != filterHashCode;
            if (!isDartyResizeParameter && IsEqualBitmapSizeMaybe(size, keepAspectRatio)) return false;

            ////var nowSize = new Size(this.BitmapSource.PixelWidth, this.BitmapSource.PixelHeight);
            ////Debug.WriteLine($"Resize: {isDartyResizeParameter}: {nowSize.Truncate()} -> {size.Truncate()}");

            if (_cancellationTokenSource.IsCancellationRequested)
            {
                return false;
            }

            try
            {
                var bitmap = CreateBitmapSource(size, keepAspectRatio, _cancellationTokenSource.Token);
                if (bitmap == null)
                {
                    return false;
                }

                lock (_lock)
                {
                    _resizeHashCode = filterHashCode;
                    this.BitmapSource = bitmap;
                }
            }
            catch (OperationCanceledException)
            {
                return false;
            }

            return true;
        }

        private BitmapSource CreateBitmapSource(Size size, bool keepAspectRatio, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var setting = new BitmapCreateSetting();

            if (!size.IsEmpty)
            {
                setting.IsKeepAspectRatio = keepAspectRatio;
                if (PictureProfile.Current.IsResizeFilterEnabled)
                {
                    setting.Mode = BitmapCreateMode.HighQuality;
                    setting.ProcessImageSettings = ImageFilter.Current.CreateProcessImageSetting();
                }
            }

            return PictureSource.CreateBitmapSource(size, setting, token);
        }


        #endregion
    }
}
