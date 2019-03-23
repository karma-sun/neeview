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

        public DefaultPictureSource(ArchiveEntry entry, PictureSourceCreateOptions createOptions) : base(entry, createOptions)
        {
        }

        public override long GetMemorySize()
        {
            return _rawData != null ? _rawData.Length : 0;
        }

        public override void InitializePictureInfo(CancellationToken token)
        {
            if (this.PictureInfo != null) return;

            token.ThrowIfCancellationRequested();

            var pictureInfo = new PictureInfo(ArchiveEntry);

            using (var stream = CreateStream(token))
            {
                token.ThrowIfCancellationRequested();

                var streamCanceller = new StreamCanceller(stream, token);
                try
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    _bitmapInfo = BitmapInfo.Create(stream);
                    var originalSize = _bitmapInfo.IsTranspose ? _bitmapInfo.GetPixelSize().Transpose() : _bitmapInfo.GetPixelSize();
                    pictureInfo.OriginalSize = originalSize;

                    var maxSize = _bitmapInfo.IsTranspose ? PictureProfile.Current.MaximumSize.Transpose() : PictureProfile.Current.MaximumSize;
                    var size = (PictureProfile.Current.IsLimitSourceSize && !maxSize.IsContains(originalSize)) ? originalSize.Uniformed(maxSize) : Size.Empty;
                    pictureInfo.Size = size.IsEmpty ? originalSize : size;

                    pictureInfo.Decoder = _decoder ?? ".Net BitmapImage";
                    pictureInfo.Exif = _bitmapInfo.Metadata != null ? new BitmapExif(_bitmapInfo.Metadata) : null;

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

            if (_createOptions.HasFlag(PictureSourceCreateOptions.IgnoreImageCache))
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
            var sw = Stopwatch.StartNew();

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


                    sw.Stop();
                    Debug.WriteLine($"PictureSource.CreateBitmapSource: {ArchiveEntry.EntryLastName}, {sw.ElapsedMilliseconds}ms");

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
                return CreateImage(stream, size, setting, format, quality, token);
            }
        }

        private byte[] CreateImage(Stream stream, Size size, BitmapCreateSetting setting, BitmapImageFormat format, int quality, CancellationToken token)
        {
            var streamCanceller = new StreamCanceller(stream, token);
            try
            {
                using (var outStream = new MemoryStream())
                {
                    _bitmapFactory.CreateImage(stream, _bitmapInfo, outStream, size, format, quality, setting, token);
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


        public override byte[] CreateThumbnail(ThumbnailProfile profile, CancellationToken token)
        {
            using (var stream = CreateStream(token))
            {
                Size size;
                if (PictureInfo != null)
                {
                    size = PictureInfo.OriginalSize;
                }
                else
                {
                    var bitmapInfo = BitmapInfo.Create(stream);
                    size = bitmapInfo.IsTranspose ? bitmapInfo.GetPixelSize().Transpose() : bitmapInfo.GetPixelSize();

                    token.ThrowIfCancellationRequested();
                    stream.Seek(0, SeekOrigin.Begin);
                }

                size = profile.GetThumbnailSize(size);
                var setting = profile.CreateBitmapCreateSetting();
                return CreateImage(stream, size, setting, profile.Format, profile.Quality, token);
            }
        }
    }
}
