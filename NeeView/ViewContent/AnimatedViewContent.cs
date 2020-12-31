using NeeView.Collections.Generic;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// Animated ViewContent
    /// </summary>
    public class AnimatedViewContent : BitmapViewContent
    {
        private static ObjectPool<MediaElement> _mediaElementPool = new ObjectPool<MediaElement>(2);

        private TextBlock _errorMessageTextBlock;
        private VisualBrush _brush;


        public AnimatedViewContent(MainViewComponent viewComponent, ViewContentSource source) : base(viewComponent, source)
        {
        }


        public new void Initialize()
        {
            // binding parameter
            var parameter = CreateBindingParameter();

            // create view
            this.View = new ViewContentControl(CreateView(this.Source, parameter));

            // content setting
            var animatedContent = this.Content as AnimatedContent;
            this.Color = animatedContent.Color;
            this.FileProxy = animatedContent.FileProxy;
        }

        /// <summary>
        /// アニメーションビュー生成
        /// </summary>
        private new FrameworkElement CreateView(ViewContentSource source, ViewContentParameters parameter)
        {
#pragma warning disable CS0618 // 型またはメンバーが旧型式です
            var uri = new Uri(((AnimatedContent)Content).FileProxy.Path, true);
#pragma warning restore CS0618 // 型またはメンバーが旧型式です

            var image = base.CreateView(source, parameter);

            var brush = new VisualBrush();
            brush.Stretch = Stretch.Fill;
            brush.Viewbox = source.GetViewBox();
            _brush = brush;

            var rectangle = new Rectangle();
            rectangle.Fill = brush;
            rectangle.SetBinding(Rectangle.VisibilityProperty, parameter.AnimationPlayerVisibility);

            var mediaGrid = new Grid();
            mediaGrid.Children.Add(rectangle);
            mediaGrid.Visibility = Visibility.Hidden;

            // inner.
            async void OnMediaOpened(object s_, RoutedEventArgs e_)
            {
                // NOTE: 動画再生開始時に黒画面が一瞬表示されることがある現象の対策。表示開始を遅延させる
                await Task.Delay(16);
                mediaGrid.Visibility = Visibility.Visible;
            }

            rectangle.Loaded += (s, e) =>
            {
                var media = CreateMediaElement(uri, parameter.BitmapScalingMode);
                media.MediaOpened += OnMediaOpened;
                brush.Visual = media;
            };

            rectangle.Unloaded += (s, e) =>
            {
                if (brush.Visual is MediaElement media)
                {
                    brush.Visual = null;
                    media.MediaOpened -= OnMediaOpened;
                    ReleaseMediaElement(media);
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
            grid.UseLayoutRounding = true;
            if (image != null) grid.Children.Add(image);
            grid.Children.Add(mediaGrid);
            grid.Children.Add(_errorMessageTextBlock);

            return grid;
        }

        private MediaElement CreateMediaElement(Uri uri, Binding bitmapScalingMode)
        {
            var media = _mediaElementPool.Allocate();
            media.LoadedBehavior = MediaState.Manual;
            media.UnloadedBehavior = MediaState.Manual;
            media.MediaEnded += Media_MediaEnded;
            media.MediaFailed += Media_MediaFailed;
            media.SetBinding(RenderOptions.BitmapScalingModeProperty, bitmapScalingMode);
            media.Source = uri;
            media.Play();
            return media;
        }

        private void ReleaseMediaElement(MediaElement media)
        {
            media.Stop();
            media.MediaEnded -= Media_MediaEnded;
            media.MediaFailed -= Media_MediaFailed;
            BindingOperations.ClearBinding(media, RenderOptions.BitmapScalingModeProperty);

            // NOTE: 一瞬黒い画像が表示されるのを防ぐために開放タイミングをずらす
            int count = 0;
            CompositionTarget.Rendering += OnRendering;
            void OnRendering(object sender, EventArgs e)
            {
                if (++count >= 3)
                {
                    CompositionTarget.Rendering -= OnRendering;
                    media.Close();
                    _mediaElementPool.Release(media);
                }
            }
        }

        private void Media_MediaEnded(object sender, RoutedEventArgs e)
        {
            var media = (MediaElement)sender;
            media.Position = TimeSpan.FromMilliseconds(1);
        }

        private void Media_MediaFailed(object sender, ExceptionRoutedEventArgs e)
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


        public new static AnimatedViewContent Create(MainViewComponent viewComponent, ViewContentSource source)
        {
            var viewContent = new AnimatedViewContent(viewComponent, source);
            viewContent.Initialize();
            return viewContent;
        }

    }
}
