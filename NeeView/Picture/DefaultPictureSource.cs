using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class DefaultPictureSource : PictureSource
    {
        private static PictureStream _pictureStream = new PictureStream();
        private static BitmapFactory _bitmapFactory = new BitmapFactory();
        private byte[] _rawData;

        public DefaultPictureSource(ArchiveEntry entry, PictureInfo pictureInfo, PictureSourceCreateOptions createOptions) : base(entry, pictureInfo, createOptions)
        {
        }

        public override long GetMemorySize()
        {
            return _rawData != null ? _rawData.Length : 0;
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:DoNotDisposeObjectsMultipleTimes")]
        public override PictureInfo CreatePictureInfo(CancellationToken token)
        {
            if (this.PictureInfo != null) return this.PictureInfo;

            token.ThrowIfCancellationRequested();

            var pictureInfo = new PictureInfo(ArchiveEntry);

            var rawDataResult = CreateRawData(false, token);
            _rawData = rawDataResult.rawData;

            using (var stream = CreateStream(token))
            {
                token.ThrowIfCancellationRequested();

                var streamCanceller = new StreamCanceller(stream, token);
                try
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    var bitmapInfo = BitmapInfo.Create(stream);
                    pictureInfo.BitmapInfo = bitmapInfo;
                    var originalSize = bitmapInfo.IsTranspose ? bitmapInfo.GetPixelSize().Transpose() : bitmapInfo.GetPixelSize();
                    pictureInfo.OriginalSize = originalSize;

                    var maxSize = bitmapInfo.IsTranspose ? PictureProfile.Current.MaximumSize.Transpose() : PictureProfile.Current.MaximumSize;
                    var size = (PictureProfile.Current.IsLimitSourceSize && !maxSize.IsContains(originalSize)) ? originalSize.Uniformed(maxSize) : Size.Empty;
                    pictureInfo.Size = size.IsEmpty ? originalSize : size;

                    pictureInfo.Decoder = rawDataResult.decoder ?? ".Net BitmapImage";
                    pictureInfo.BitsPerPixel = bitmapInfo.BitsPerPixel;
                    pictureInfo.Exif = bitmapInfo.Exif;

                    this.PictureInfo = pictureInfo;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch
                {
                    token.ThrowIfCancellationRequested();
                    throw;
                }
                finally
                {
                    streamCanceller.Dispose();
                }
            }

            token.ThrowIfCancellationRequested();

            _rawData = CompressRawData(_rawData);

            return this.PictureInfo;
        }


        private (byte[] rawData, string decoder) CreateRawData(bool isCompressed, CancellationToken token)
        {
            var ms = new MemoryStream();
            using (var namedStream = _pictureStream.Create(ArchiveEntry))
            {
                try
                {
                    namedStream.Stream.CopyToAsync(ms, 81920, token).Wait();
                }
                catch (AggregateException ex)
                {
                    token.ThrowIfCancellationRequested();
                    throw ex.InnerException;
                }
                catch
                {
                    token.ThrowIfCancellationRequested();
                    throw;
                }

                var rawData = ms.ToArray();
                if (isCompressed)
                {
                    rawData = CompressRawData(rawData);
                }

                return (rawData, namedStream.Name);
            }
        }

        // RawData: メモリ圧縮のためにBMPはPNGに変換 
        private byte[] CompressRawData(byte[] source)
        {
            if (source == null || _createOptions.HasFlag(PictureSourceCreateOptions.IgnoreCompress)) return source;
            if (source[0] != 'B' || source[1] != 'M') return source;

            try
            {
                ////Debug.WriteLine($"Compress BMP to PNG.");
                using (var inStream = new MemoryStream(source))
                using (var outStream = new MemoryStream())
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(inStream));
                    encoder.Save(outStream);
                    return outStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return source;
            }
        }

        private Stream CreateStream(CancellationToken token)
        {
            if (_rawData == null)
            {
                _rawData = CreateRawData(true, token).rawData;
            }

            return new MemoryStream(_rawData);
        }

        [SuppressMessage("Microsoft.Usage", "CA2202:DoNotDisposeObjectsMultipleTimes")]
        public override BitmapSource CreateBitmapSource(Size size, BitmapCreateSetting setting, CancellationToken token)
        {
            using (var stream = CreateStream(token))
            {
                var streamCanceller = new StreamCanceller(stream, token);
                try
                {
                    if (setting.IsKeepAspectRatio && !size.IsEmpty)
                    {
                        size = new Size(0, size.Height);
                    }

                    var bitmapSource = _bitmapFactory.CreateBitmapSource(stream, PictureInfo?.BitmapInfo, size, setting, token);

                    // 色情報とBPP設定。
                    this.PictureInfo.SetPixelInfo(bitmapSource);

                    return bitmapSource;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch
                {
                    token.ThrowIfCancellationRequested();
                    throw;
                }
                finally
                {
                    streamCanceller.Dispose();
                }
            }
        }


        [SuppressMessage("Microsoft.Usage", "CA2202:DoNotDisposeObjectsMultipleTimes")]
        public override byte[] CreateImage(Size size, BitmapCreateSetting setting, BitmapImageFormat format, int quality, CancellationToken token)
        {
            using (var stream = CreateStream(token))
            {
                var streamCanceller = new StreamCanceller(stream, token);
                try
                {
                    using (var outStream = new MemoryStream())
                    {
                        _bitmapFactory.CreateImage(stream, PictureInfo?.BitmapInfo, outStream, size, format, quality, setting, token);
                        return outStream.ToArray();
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch
                {
                    token.ThrowIfCancellationRequested();
                    throw;
                }
                finally
                {
                    streamCanceller.Dispose();
                }
            }
        }


        public override byte[] CreateThumbnail(ThumbnailProfile profile, CancellationToken token)
        {
            Size size;
            if (PictureInfo != null)
            {
                size = PictureInfo.Size;
            }
            else
            {
                using (var stream = CreateStream(token))
                {
                    var bitmapInfo = BitmapInfo.Create(stream);
                    size = bitmapInfo.IsTranspose ? bitmapInfo.GetPixelSize().Transpose() : bitmapInfo.GetPixelSize();
                }
            }

            token.ThrowIfCancellationRequested();

            size = profile.GetThumbnailSize(size);
            var setting = profile.CreateBitmapCreateSetting();
            return CreateImage(size, setting, profile.Format, profile.Quality, token);
        }

        public override Size FixedSize(Size size)
        {
            Debug.Assert(PictureInfo != null);

            var maxWixth = Math.Max(this.PictureInfo.Size.Width, PictureProfile.Current.MaximumSize.Width);
            var maxHeight = Math.Max(this.PictureInfo.Size.Height, PictureProfile.Current.MaximumSize.Height);
            var maxSize = new Size(maxWixth, maxHeight);
            return size.Limit(maxSize);
        }

    }
}
