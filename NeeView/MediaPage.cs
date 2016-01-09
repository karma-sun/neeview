using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace NeeView
{

    public class MediaPage : Page
    {
        string _Path;
        public override string Path { get { return _Path; } }

        public override string FullPath { get { return _Path; } }

        //string _ArchiveFileName;
        Archiver _Archiver;

        public override string ToString()
        {
            return System.IO.Path.GetFileName(_Path);
        }

        public MediaPage(string path, Archiver archiver = null)
        {
            _Path = path;
            _Archiver = archiver;

            Setting(); // ##
        }

        //FileReaderType _FileReaderType;
        BitmapLoaderType _BitmapLoaderType;

        // 設定テスト
        public void Setting()
        {
            //if (_Archiver != null)
            //{
            //    _FileReaderType = FileReaderType.ZipArchiveEntry;
            //}
            //else
            //{
            //    _FileReaderType = FileReaderType.File;
            //}

            _BitmapLoaderType = BitmapLoaderType.Default;
        }


        //
        private BitmapSource Load()
        {
            //using (var reader = FileReaderFactory.OpenReadRetry(_FileReaderType, _Path, _Archiver))
            using (var stream = _Archiver.OpenEntry(_Path))
            {
                var bitmapLoader = BitmapLoaderManager.Create(_BitmapLoaderType);
                return bitmapLoader.Load(stream, _Path);
            }
        }

#if false
        //
        public async Task<MediaElement> CreateMediaElementAsync()
        {
            SetMessage("Phase. Source ...");
            var source = Load();
            if (source == null) throw new ApplicationException("cannot load by BitmapImge.");

            Width = source.PixelWidth;
            Height = source.PixelHeight;

            SetMessage("Phase. MediaElement...");

            MediaElement media = null;
            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                SetMessage("Phase. New MediaElement ...");

                media = new MediaElement();
                media.Source = new Uri(_Path);
                media.MediaEnded += MediaElement_MediaEnded;
                //RenderOptions.SetBitmapScalingMode(media, BitmapScalingMode.HighQuality);

                SetMessage("Phase. New MediaElement Done.");
            });

            SetMessage("Phase. Done.");

            return media;
        }
#endif

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            var media = (MediaElement)sender;
            media.Position = TimeSpan.FromMilliseconds(1);
        }


        protected override object OpenContent()
        {
            try
            {
                var source = Load();
                if (source == null) throw new ApplicationException("cannot load by BitmapImge.");

                Width = source.PixelWidth;
                Height = source.PixelHeight;

                //await CreateMediaElementAsync();

                return new Uri(_Path);
            }
            catch (Exception e)
            {
                SetMessage("Exception: " + e.Message);
                Width = 512.0;
                Height = 512.0;
                return $"{System.IO.Path.GetFileName(_Path)}\n{e.Message}";
            }
        }

    }
}
