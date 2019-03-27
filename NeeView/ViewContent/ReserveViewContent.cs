using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// Reserve ViewContent
    /// </summary>
    public class ReserveViewContent : ViewContent
    {
        #region Fields

        private Rectangle _reserveRectangle;

        #endregion

        #region Constructors

        public ReserveViewContent(ViewPage source, ViewContent old) : base(source, old)
        {
        }

        #endregion

        #region Methods

        public void Initialize()
        {
            // binding parameter
            var parameter = CreateBindingParameter();

            // create view
            this.View = CreateView(this.Source, parameter);

            // content setting
            if (BookProfile.Current.IsLoadingPageVisible)
            {
                if (this.Reserver != null)
                {
                    this.Size = this.Reserver.Size;
                    this.Color = this.Reserver.Color;
                }
                else
                {
                    this.Size = new Size(480, 680);
                    this.Color = Colors.Black;
                }
            }
            else
            {
                this.Size = this.Size.IsEmptyOrZero() ? new Size(480, 680) : this.Size;
                this.Color = this.Content is BitmapContent bitmapContent ? bitmapContent.Color : Colors.Black;
            }
        }

        /// <summary>
        /// 読み込み中ビュー生成
        /// </summary>
        /// <param name="source"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private FrameworkElement CreateView(ViewPage source, ViewContentParameters parameter)
        {
            var grid = new Grid();

            var rectangle = new Rectangle();
            rectangle.Fill = source.CreateReserveBrush(this.Reserver);
            RenderOptions.SetBitmapScalingMode(rectangle, BitmapScalingMode.HighQuality);
            grid.Children.Add(rectangle);

            _reserveRectangle = rectangle;

            var textBlock = new TextBlock();
            textBlock.Text = LoosePath.GetFileName(source.Page.EntryFullName);
            textBlock.Foreground = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC));
            textBlock.FontSize = 20;
            textBlock.Margin = new Thickness(10);
            textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock.VerticalAlignment = VerticalAlignment.Center;

            grid.Children.Add(textBlock);

            return grid;
        }

        public override bool IsBitmapScalingModeSupported() => false;

        public override Brush GetViewBrush()
        {
            return _reserveRectangle?.Fill;
        }

        #endregion

        #region Static Methods

        public static ReserveViewContent Create(ViewPage source, ViewContent oldViewContent)
        {
            var viewContent = new ReserveViewContent(source, oldViewContent);
            viewContent.Initialize();
            return viewContent;
        }

        #endregion
    }
}
