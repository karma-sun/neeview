using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    public class DefaultPictureSource : PictureSource
    {
        private PictureStream _pictureStream = new PictureStream();
        private BitmapFactory _bitmapFactory = new BitmapFactory();

        private byte[] _rawData;
        private string _decoder;
        private BitmapInfo _bitmapInfo;

        public DefaultPictureSource(ArchiveEntry entry, bool ignoreImageCache) : base(entry, ignoreImageCache)
        {
        }

        public override void Initialize(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            this.PictureInfo = new PictureInfo(ArchiveEntry);

            using (var stream = CreateStream(token))
            {
                token.ThrowIfCancellationRequested();

                var streamCanceller = new StreamCanceller(stream, token);
                try
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    _bitmapInfo = BitmapInfo.Create(stream);
                    var originalSize = _bitmapInfo.IsTranspose ? _bitmapInfo.GetPixelSize().Transpose() : _bitmapInfo.GetPixelSize();
                    this.PictureInfo.OriginalSize = originalSize;

                    var maxSize = _bitmapInfo.IsTranspose ? PictureProfile.Current.MaximumSize.Transpose() : PictureProfile.Current.MaximumSize;
                    var size = (PictureProfile.Current.IsLimitSourceSize && !maxSize.IsContains(originalSize)) ? originalSize.Uniformed(maxSize) : Size.Empty;
                    this.PictureInfo.Size = size.IsEmpty ? originalSize : size;

                    this.PictureInfo.Decoder = _decoder ?? ".Net BitmapImage";
                    this.PictureInfo.Exif = _bitmapInfo.Metadata != null ? new BitmapExif(_bitmapInfo.Metadata) : null;
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

            CompressRawDataAsync();
        }

        // RawData: メモリ圧縮のためにBMPはPNGに変換 (非同期)
        private void CompressRawDataAsync()
        {
            if (_rawData == null) return;
            if (_rawData[0] != 'B' || _rawData[1] != 'M') return;

            var background = Task.Run(() =>
            {
                try
                {
                    using (var inStream = new MemoryStream(_rawData))
                    using (var outStream = new MemoryStream())
                    {
                        var encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(inStream));
                        encoder.Save(outStream);
                        _rawData = outStream.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            });
        }


        private Stream CreateStream(CancellationToken token)
        {
            Stream stream = null;

            if (_ignoreImageCache)
            {
                var namedStream = _pictureStream.Create(ArchiveEntry);
                stream = namedStream.Stream;
                _decoder = namedStream.Name;
            }
            else if (_rawData != null)
            {
                stream = new MemoryStream(_rawData);
            }
            else
            {
                stream = new MemoryStream();
                using (var namedStream = _pictureStream.Create(ArchiveEntry))
                {
                    try
                    {
                        namedStream.Stream.CopyToAsync(stream, 81920, token).Wait();
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
                    _rawData = ((MemoryStream)stream).ToArray();
                    _decoder = namedStream.Name;
                }
                stream.Seek(0, SeekOrigin.Begin);
            }

            return stream;
        }

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

                    var bitmapSource = _bitmapFactory.CreateBitmapSource(stream, _bitmapInfo, size, setting, token);

                    // TODO: ここで情報設定ってどうなの？
                    this.PictureInfo.SetPixelInfo(bitmapSource, size.IsEmpty ? size : this.PictureInfo.OriginalSize);

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

        public override byte[] CreateImage(Size size, BitmapCreateSetting setting, BitmapImageFormat format, int quality, CancellationToken token)
        {
            using (var stream = CreateStream(token))
            {
                var streamCanceller = new StreamCanceller(stream, token);
                try
                {
                    using (var outStream = new MemoryStream())
                    {
                        _bitmapFactory.CreateImage(stream, null, outStream, size, format, quality, setting, token);
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
    }
}
