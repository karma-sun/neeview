using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Collections.Generic;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace NeeView
{
    public class MediaViewContent : BitmapViewContent
    {
        #region Fields

        private static ObjectPool<MediaPlayer> _mediaPlayerPool = new ObjectPool<MediaPlayer>();

        private MediaPlayer _player;
        private TextBlock _errorMessageTextBlock;

        #endregion

        #region Constructors

        public MediaViewContent(ViewPage source) : base(source)
        {
        }

        #endregion

        #region Methods

        public new void Initialize()
        {
            // binding parameter
            var parameter = CreateBindingParameter();

            // create view
            this.View = new ViewContentControl(CreateView(this.Source, parameter));

            // content setting
            var animatedContent = this.Content as MediaContent;
            this.Color = animatedContent.Color;
            this.FileProxy = animatedContent.FileProxy;
        }


        /// <summary>
        /// アニメーションビュー生成
        /// </summary>
        private new FrameworkElement CreateView(ViewPage source, ViewContentParameters parameter)
        {
            var uri = new Uri(((MediaContent)Content).FileProxy.Path);
            var isLastStart = source.IsLastStart;

            var videoDrawing = new VideoDrawing()
            {
                Rect = new Rect(this.Content.Size),
            };
            var brush = new DrawingBrush()
            {
                Drawing = videoDrawing,
                Stretch = Stretch.Fill,
                Viewbox = source.GetViewBox()
            };
            var rectangle = new Rectangle()
            {
                Fill = brush
            };
            rectangle.SetBinding(RenderOptions.BitmapScalingModeProperty, parameter.BitmapScalingMode); // 効果なし

            rectangle.Loaded += (s, e) =>
            {
                _player = _mediaPlayerPool.Allocate();
                _player.MediaFailed += Media_MediaFailed;
                _player.MediaOpened += Media_MediaOpened;

                videoDrawing.Player = _player;

                MediaControl.Current.RiaseContentChanged(this, new MediaPlayerChanged(_player, uri, isLastStart));
            };

            rectangle.Unloaded += (s, e) =>
            {
                if (videoDrawing.Player is MediaPlayer player)
                {
                    videoDrawing.Player = null;

                    player.MediaFailed -= Media_MediaFailed;
                    player.MediaOpened -= Media_MediaOpened;
                    player.Stop();
                    player.Close();
                    _mediaPlayerPool.Release(player);
                    _player = null;

                    MediaControl.Current.RiaseContentChanged(this, new MediaPlayerChanged());

                    rectangle.Visibility = Visibility.Hidden;
                }
            };

            _errorMessageTextBlock = new TextBlock()
            {
                Background = Brushes.Black,
                Foreground = Brushes.White,
                Padding = new Thickness(40, 20, 40, 20),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 20,
                TextWrapping = TextWrapping.Wrap,
                Visibility = Visibility.Collapsed,
            };

            var grid = new Grid();
            grid.Children.Add(rectangle);
            grid.Children.Add(_errorMessageTextBlock);

            return grid;
        }

        private void Media_MediaOpened(object sender, EventArgs e)
        {
            var content = this.Content as MediaContent;
            if (content == null) return;

            var player = (MediaPlayer)sender;

            var size = new Size(player.NaturalVideoWidth, player.NaturalVideoHeight);
            Size = size;
            content.SetSize(size);

            ContentCanvas.Current.UpdateContentSize();
            FileInformation.Current.Flush();
        }

        private void Media_MediaFailed(object sender, ExceptionEventArgs e)
        {
            _errorMessageTextBlock.Text = e.ErrorException != null ? e.ErrorException.Message : Properties.Resources.NotifyPlayFailed;
            _errorMessageTextBlock.Visibility = Visibility.Visible;
        }


        public override bool Rebuild(double scale)
        {
            return true;
        }

        #endregion

        #region IDisposable Support

        protected override void Dispose(bool disposing)
        {
            if (_player != null)
            {
                _player?.Stop();
                _player?.Close();
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Static Methods

        public new static MediaViewContent Create(ViewPage source)
        {
            var viewContent = new MediaViewContent(source);
            viewContent.Initialize();
            return viewContent;
        }

        #endregion
    }
}
