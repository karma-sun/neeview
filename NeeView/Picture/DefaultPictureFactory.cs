using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// Default Picture Factory
    /// </summary>
    public class DefaultPictureFactory : IPictureFactory
    {
        //
        private PictureStream _pictureStream = new PictureStream();

        //
        private BitmapFactory _bitmapFactory = new BitmapFactory();


        /// <summary>
        /// Pictureの生成
        /// </summary>
        public async Task<Picture> CreateAsync(ArchiveEntry entry, PictureCreateOptions options, CancellationToken token)
        {
            var picture = new Picture(entry);

            MemoryStream stream = null;
            CancellationTokenRegistration? tokenRegistration = null;
            try
            {
                stream = new MemoryStream();

                string decoder = null;

                // raw data
                using (var namedStream = _pictureStream.Create(entry))
                {
                    token.ThrowIfCancellationRequested();
                    await namedStream.Stream.CopyToAsync(stream, 81920, token);
                    picture.RawData = stream.ToArray();
                    decoder = namedStream.Name;
                }

                // info
                token.ThrowIfCancellationRequested();
                stream.Seek(0, SeekOrigin.Begin);
                var info = BitmapInfo.Create(stream);
                var originalSize = info.IsTranspose ? info.GetPixelSize().Transpose() : info.GetPixelSize();
                picture.PictureInfo.OriginalSize = originalSize;

                var maxSize = info.IsTranspose ? PictureProfile.Current.MaximumSize.Transpose() : PictureProfile.Current.MaximumSize;
                var size = (PictureProfile.Current.IsLimitSourceSize && !maxSize.IsContains(originalSize)) ? originalSize.Uniformed(maxSize) : Size.Empty;
                picture.PictureInfo.Size = size.IsEmpty ? originalSize : size;

                token.ThrowIfCancellationRequested();

                // regist CancellationToken Callback
                tokenRegistration = token.Register(() => stream?.Dispose());

                // bitmap
                if (options.HasFlag(PictureCreateOptions.CreateBitmap) || picture.PictureInfo.Size.IsEmpty)
                {
                    var bitmapSource = _bitmapFactory.Create(stream, info, size, new BitmapCreateSetting(), token);

                    picture.PictureInfo.Exif = info.Metadata != null ? new BitmapExif(info.Metadata) : null;
                    picture.PictureInfo.Decoder = decoder ?? ".Net BitmapImage";
                    picture.PictureInfo.SetPixelInfo(bitmapSource, size.IsEmpty ? size : originalSize);

                    picture.BitmapSource = bitmapSource;
                }

                // thumbnail
                if (options.HasFlag(PictureCreateOptions.CreateThumbnail))
                {
                    using (var ms = new MemoryStream())
                    {
                        var thumbnailSize = ThumbnailProfile.Current.GetThumbnailSize(picture.PictureInfo.Size);
                        var setting = new BitmapCreateSetting();
                        _bitmapFactory.CreateImage(stream, info, ms, thumbnailSize, ThumbnailProfile.Current.Format, ThumbnailProfile.Current.Quality, ThumbnailProfile.Current.CreateBitmapCreateSetting());
                        picture.Thumbnail = ms.ToArray();

                        ////Debug.WriteLine($"Thumbnail: {picture.Thumbnail.Length / 1024}KB");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // キャンセル時は null を返す
                return null;
            }
            catch (Exception)
            {
                if (token.IsCancellationRequested)
                {
                    return null;
                }
                throw;
            }
            finally
            {
                stream?.Dispose();
                stream = null;

                tokenRegistration?.Dispose();
                tokenRegistration = null;
            }

            // RawData: メモリ圧縮のためにBMPはPNGに変換 (非同期)
            if (picture.RawData != null && picture.RawData[0] == 'B' && picture.RawData[1] == 'M')
            {
                var background = Task.Run(() =>
                {
                    ////var sw = Stopwatch.StartNew();
                    ////var oldLength = picture.RawData.Length;

                    try
                    {
                        using (var inStream = new MemoryStream(picture.RawData))
                        using (var outStream = new MemoryStream())
                        {
                            var encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(inStream));
                            encoder.Save(outStream);
                            picture.RawData = outStream.ToArray();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }

                    ////sw.Stop();
                    ////Debug.WriteLine($"{entry.EntryLastName}: BMP to PNG: {sw.ElapsedMilliseconds}ms, {oldLength/1024}KB -> {picture.RawData.Length / 1024}KB");
                });
            }

            return picture;
        }

        //
        private Stream CreateStream(ArchiveEntry entry, byte[] raw)
        {
            if (raw != null)
            {
                return new MemoryStream(raw);
            }
            else
            {
                return _pictureStream.Create(entry).Stream;
            }
        }

        //
        public BitmapSource CreateBitmapSource(ArchiveEntry entry, byte[] raw, Size size, bool keepAspectRatio, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (keepAspectRatio && !size.IsEmpty)
            {
                size = new Size(0, size.Height);
            }

            var setting = new BitmapCreateSetting();

            if (PictureProfile.Current.IsResizeFilterEnabled && !size.IsEmpty)
            {
                setting.Mode = BitmapCreateMode.HighQuality;
                setting.ProcessImageSettings = ImageFilter.Current.CreateProcessImageSetting();
            }

            Stream stream = null;
            CancellationTokenRegistration? tokenRegistration = null;
            try
            {
                stream = CreateStream(entry, raw);

                // regist CancellationToken Callback
                tokenRegistration = token.Register(() => stream?.Dispose());

                return _bitmapFactory.Create(stream, null, size, setting, token);
            }
            catch (OperationCanceledException)
            {
                // キャンセル時は null を返す
                return null;
            }
            catch (Exception)
            {
                if (token.IsCancellationRequested)
                {
                    return null;
                }
                throw;
            }
            finally
            {
                stream?.Dispose();
                stream = null;

                tokenRegistration?.Dispose();
                tokenRegistration = null;
            }
        }

        //
        public byte[] CreateImage(ArchiveEntry entry, byte[] raw, Size size, BitmapImageFormat format, int quality, BitmapCreateSetting setting)
        {
            using (var stream = CreateStream(entry, raw))
            using (var ms = new MemoryStream())
            {
                _bitmapFactory.CreateImage(stream, null, ms, size, format, quality, setting);
                return ms.ToArray();
            }
        }
    }
}
