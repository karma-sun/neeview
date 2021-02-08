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
        private static ObjectPool<MediaPlayer> _mediaPlayerPool = new ObjectPool<MediaPlayer>();

        private MediaPlayer _player;
        private TextBlock _errorMessageTextBlock;
        private DrawingBrush _brush;
        private VideoDrawing _videoDrawing;
        private Rectangle _rectangle;

        public MediaViewContent(MainViewComponent viewComponent, ViewContentSource source) : base(viewComponent, source)
        {
        }


        private void Initialize()
        {
            // binding parameter
            var parameter = CreateBindingParameter();

            // create view
            this.View = new ViewContentControl(CreateMediaView(this.Source, parameter));

            // content setting
            var animatedContent = this.Content as MediaContent;
            this.Color = animatedContent.Color;
            this.FileProxy = animatedContent.FileProxy;
        }

        public override void OnAttached()
        {
            base.OnAttached();

            OnDetached();

#pragma warning disable CS0618 // 型またはメンバーが旧型式です
            var uri = new Uri(((MediaContent)Content).FileProxy.Path, true);
#pragma warning restore CS0618 // 型またはメンバーが旧型式です

            var isLastStart = this.Source.IsLastStart;

            _player = _mediaPlayerPool.Allocate();
            _player.MediaFailed += Media_MediaFailed;
            _player.MediaOpened += Media_MediaOpened;

            _videoDrawing.Player = _player;

            MediaControl.Current.RiaseContentChanged(this, new MediaPlayerChanged(_player, uri, isLastStart));
        }

        public override void OnDetached()
        {
            base.OnDetached();

            if (_player is null) return;

            _videoDrawing.Player = null;

            _player.MediaFailed -= Media_MediaFailed;
            _player.MediaOpened -= Media_MediaOpened;
            _player.Stop();
            _player.Close();
            _mediaPlayerPool.Release(_player);
            _player = null;

            MediaControl.Current.RiaseContentChanged(this, new MediaPlayerChanged());

            _rectangle.Visibility = Visibility.Hidden;
        }


        /// <summary>
        /// アニメーションビュー生成
        /// </summary>
        private FrameworkElement CreateMediaView(ViewContentSource source, ViewContentParameters parameter)
        {
            _videoDrawing = new VideoDrawing()
            {
                Rect = new Rect(this.Content.Size),
            };

            _brush = new DrawingBrush()
            {
                Drawing = _videoDrawing,
                Stretch = Stretch.Fill,
                Viewbox = source.GetViewBox()
            };

            _rectangle = new Rectangle()
            {
                Fill = _brush
            };
            _rectangle.SetBinding(RenderOptions.BitmapScalingModeProperty, parameter.BitmapScalingMode); // 効果なし

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
            grid.Children.Add(_rectangle);
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

            this.ViewComponent.ContentCanvas.UpdateContentSize();
            this.ViewComponent.ContentCanvas.ResetTransformRaw(true, false, false, 0.0, false);
            this.ViewComponent.DragTransformControl.SnapView();
            FileInformation.Current.Update();
        }

        private void Media_MediaFailed(object sender, ExceptionEventArgs e)
        {
            _errorMessageTextBlock.Text = e.ErrorException != null ? e.ErrorException.Message : Properties.Resources.Notice_PlayFailed;
            _errorMessageTextBlock.Visibility = Visibility.Visible;
        }


        public override bool Rebuild(double scale)
        {
            return true;
        }

        public override void UpdateViewBox()
        {
            if (_brush != null)
            {
                _brush.Viewbox = Source.GetViewBox();
            }
        }


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


        public new static MediaViewContent Create(MainViewComponent viewComponent, ViewContentSource source)
        {
            var viewContent = new MediaViewContent(viewComponent, source);
            viewContent.Initialize();
            return viewContent;
        }
    }
}
