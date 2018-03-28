using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// Animated ViewContent
    /// </summary>
    public class AnimatedViewContent : BitmapViewContent
    {
        private TextBlock _errorMessageTextBlock;

        #region Constructors

        public AnimatedViewContent(ViewPage source, ViewContent old) : base(source, old)
        {
        }

        #endregion

        #region Methods

        //
        public new void Initialize()
        {
            // binding parameter
            var parameter = CreateBindingParameter();

            // create view
            this.View = CreateView(this.Source, parameter);

            // content setting
            var animatedContent = this.Content as AnimatedContent;
            this.Color = animatedContent.Color;
            this.FileProxy = animatedContent.FileProxy;
        }


        /// <summary>
        /// アニメーションビュー生成
        /// </summary>
        /// <param name="source"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private new FrameworkElement CreateView(ViewPage source, ViewContentParameters parameter)
        {
            //
            var image = base.CreateView(source, parameter);
            image?.SetBinding(Rectangle.VisibilityProperty, parameter.AnimationImageVisibility);

            //
            var media = new MediaElement();
            media.MediaEnded += (s, e_) => media.Position = TimeSpan.FromMilliseconds(1);
            media.MediaFailed += Media_MediaFailed;
            media.SetBinding(RenderOptions.BitmapScalingModeProperty, parameter.BitmapScalingMode);
            media.Source = new Uri(((AnimatedContent)Content).FileProxy.Path);

            var brush = new VisualBrush();
            brush.Visual = media;
            brush.Stretch = Stretch.Fill;
            brush.Viewbox = source.GetViewBox();

            var canvas = new Rectangle();
            canvas.Fill = brush;
            canvas.SetBinding(Rectangle.VisibilityProperty, parameter.AnimationPlayerVisibility);

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
            grid.Children.Add(canvas);
            grid.Children.Add(_errorMessageTextBlock);

            return grid;
        }

        //
        public override bool Rebuild(double scale)
        {
            return true;
        }

        //
        private void Media_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            _errorMessageTextBlock.Text = e.ErrorException != null ? e.ErrorException.Message : Properties.Resources.NotifyPlayFailed;
            _errorMessageTextBlock.Visibility = Visibility.Visible;
        }

        #endregion

        #region Static Methods

        public new static AnimatedViewContent Create(ViewPage source, ViewContent oldViewContent)
        {
            var viewContent = new AnimatedViewContent(source, oldViewContent);
            viewContent.Initialize();
            return viewContent;
        }

        #endregion
    }
}
