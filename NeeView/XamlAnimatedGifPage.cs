using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using XamlAnimatedGif;

namespace NeeView
{
    // あとまわし
#if false

    public class XamlAnimatedGifPage : Page
    {
        string _Path;
        public override string Path { get { return _Path; } }

        string _ArchiveFileName;

        public override string ToString()
        {
            return System.IO.Path.GetFileName(_Path);
        }

        public XamlAnimatedGifPage(string path, string archiveFileName = null)
        {
            _Path = path;
            _ArchiveFileName = archiveFileName;

            Setting(); // ##
        }

        FileReaderType _FileReaderType;
        BitmapLoaderType _BitmapLoaderType;

        MemoryStream _MemoryStream;

        // 設定テスト
        public void Setting()
        {
            if (_ArchiveFileName != null)
            {
                _FileReaderType = FileReaderType.ZipArchiveEntry;
            }
            else
            {
                _FileReaderType = FileReaderType.FileOnLoad;
            }

            _BitmapLoaderType = BitmapLoaderType.Default;
        }

        byte[] _Buffer;

        //
        private BitmapSource Load()
        {
            using (var reader = FileReaderFactory.OpenRead(_FileReaderType, _Path, _ArchiveFileName))
            {
                _Buffer = ((MemoryStream)reader.Stream).ToArray();
                reader.Stream.Seek(0, SeekOrigin.Begin);

                var bitmapLoader = BitmapLoaderFactory.Create(_BitmapLoaderType);
                return bitmapLoader.Load(reader.Stream, _Path);
            }
        }

        //
        public async Task<Image> CreateMediaElementAsync()
        {
            SetMessage("Phase. Source ...");
            var source = Load();
            if (source == null) throw new ApplicationException("cannot load by BitmapImge.");

            Width = source.PixelWidth;
            Height = source.PixelHeight;

            SetMessage("Phase. Image (AnimGIF)...");

            Image image = null;
            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                SetMessage("Phase. New Image (AnimGIF) ...");

                Debug.Assert(_MemoryStream == null);

                //_FileReader = FileReaderFactory.OpenRead(_FileReaderType, _Path, _ArchiveFileName);
                //_MemoryStream = new MemoryStream(_Buffer);
                //_FileReader.Stream.CopyTo(_MemoryStream);
                //_MemoryStream.Seek(0, SeekOrigin.Begin);

                image = new Image();
                //RenderOptions.SetBitmapScalingMode(media, BitmapScalingMode.HighQuality);

                //var stream = new FileStream(@"E:\Pictures\画像\1276421936562.gif", FileMode.Open, FileAccess.Read);
                AnimationBehavior.SetSourceUri(image, new Uri(_Path));
                //AnimationBehavior.SetSourceStream(image, _FileReader.Stream);
                //AnimationBehavior.SetSourceStream(image, _MemoryStream);

                //AnimationBehavior.SetRepeatBehavior(image, RepeatBehavior.Forever);
                //AnimationBehavior.SetAutoStart(image, true);

                AnimationBehavior.AddLoadedHandler(image, AnimationBehavior_OnLoaded);
                AnimationBehavior.AddErrorHandler(image, AnimationBehavior_OnError);

                SetMessage("Phase. New Image (AnimGIF) Done.");
            });

            //_image = image; // ##

            return image;

#if false
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
#endif
        }

        //Image _image;
        //Animator _animator;

        private void AnimationBehavior_OnLoaded(object sender, RoutedEventArgs e)
        {
#if false
            if (_animator != null)
            {
                //_animator.CurrentFrameChanged -= CurrentFrameChanged;
                _animator.AnimationCompleted -= AnimationCompleted;
            }

            var _animator = AnimationBehavior.GetAnimator(_image);

            if (_animator != null)
            {
                //_animator.CurrentFrameChanged += CurrentFrameChanged;
                _animator.AnimationCompleted += AnimationCompleted;
                //sldPosition.Value = 0;
                //sldPosition.Maximum = _animator.FrameCount - 1;
                //SetPlayPauseEnabled(_animator.IsPaused || _animator.IsComplete);
            }
#endif
        }


        public override void ReOpen()
        {
#if false
            if (_Content == null || !(_Content is Image)) return;

            //if (_FileReader.Stream.CanSeek) return;
            if (_MemoryStream.CanSeek) return;

            //_FileReader = FileReaderFactory.OpenRead(_FileReaderType, _Path, _ArchiveFileName);
            //_MemoryStream = new MemoryStream();
            //_FileReader.Stream.CopyTo(_MemoryStream);
            //_MemoryStream.Seek(0, SeekOrigin.Begin);

            _MemoryStream = new MemoryStream(_Buffer);

            //image = new Image();
            var image = new Image();
            AnimationBehavior.SetSourceStream(image, _MemoryStream);
            AnimationBehavior.AddLoadedHandler(image, AnimationBehavior_OnLoaded);
            AnimationBehavior.AddErrorHandler(image, AnimationBehavior_OnError);
            //_image = image; // ##

            _Content = image;
#endif
        }

        private void AnimationCompleted(object sender, EventArgs e)
        {
            Debug.WriteLine("GIF Completed");
        }

        private void AnimationBehavior_OnError(DependencyObject d, AnimationErrorEventArgs e)
        {
            Debug.WriteLine($"An error occurred ({e.Kind}): {e.Exception}");
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            var media = (MediaElement)sender;
            media.Position = TimeSpan.FromMilliseconds(1);
        }


        protected override async Task<object> OpenContentAsync()
        {
            try
            {
                return await CreateMediaElementAsync();
            }
            catch (Exception e)
            {
                SetMessage("Exception: " + e.Message);
                TextBlock textBlock = null;
                await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    textBlock = new TextBlock();
                    textBlock.Text = $"{_Path}\n{e.Message}";
                    textBlock.VerticalAlignment = VerticalAlignment.Center;
                    textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                });

                Width = 0.0;
                Height = 0.0;

                return textBlock;
            }
        }

        protected override void CloseContent()
        {
            if (_Buffer != null)
            {
                _Buffer = null;
            }
            if (_MemoryStream != null)
            {
                _MemoryStream.Dispose();
                _MemoryStream = null;
            }

        }
    }
#endif
}
