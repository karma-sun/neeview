using NeeView.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// MediaElementでのアニメーション画像表示
    /// </summary>
    public class AnimatedView
    {
        private static ObjectPool<MediaElement> _mediaElementPool = new ObjectPool<MediaElement>(2);

        private ViewContentSource _source;
        private Uri _uri;
        private ViewContentParameters _parameter;
        private VisualBrush _brush;
        private Grid _mediaGrid;
        private TextBlock _errorMessageTextBlock;


        public AnimatedView(ViewContentSource source, ViewContentParameters parameter, Uri uri, FrameworkElement alternativeElement)
        {
            _source = source;
            _uri = uri;
            _parameter = parameter;

            this.View = CreateAnimatedView(parameter, alternativeElement);
        }

        public FrameworkElement View { get; }

        /// <summary>
        /// アニメーションビュー生成
        /// </summary>
        private FrameworkElement CreateAnimatedView(ViewContentParameters parameter, FrameworkElement alternativeElement)
        {
            _brush = new VisualBrush();
            _brush.Stretch = Stretch.Fill;
            _brush.Viewbox = _source.GetViewBox();

            var rectangle = new Rectangle();
            rectangle.Fill = _brush;
            rectangle.SetBinding(Rectangle.VisibilityProperty, parameter.AnimationPlayerVisibility);

            _mediaGrid = new Grid();
            _mediaGrid.Children.Add(rectangle);
            _mediaGrid.Visibility = Visibility.Hidden;

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
            if (alternativeElement != null) grid.Children.Add(alternativeElement);
            grid.Children.Add(_mediaGrid);
            grid.Children.Add(_errorMessageTextBlock);

            return grid;
        }


        public void OnAttached()
        {
            var media = CreateMediaElement(_uri, _parameter.BitmapScalingMode);
            media.MediaOpened += OnMediaOpened;
            _brush.Visual = media;
        }

        public void OnDetached()
        {
            if (_brush.Visual is MediaElement media)
            {
                _brush.Visual = null;
                media.MediaOpened -= OnMediaOpened;
                ReleaseMediaElement(media);
            }
        }

        private async void OnMediaOpened(object s, RoutedEventArgs e)
        {
            // NOTE: 動画再生開始時に黒画面が一瞬表示されることがある現象の対策。表示開始を遅延させる
            await Task.Delay(16);
            _mediaGrid.Visibility = Visibility.Visible;
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

        public void UpdateViewBox()
        {
            if (_brush != null)
            {
                _brush.Viewbox = _source.GetViewBox();
            }
        }
    }

}
