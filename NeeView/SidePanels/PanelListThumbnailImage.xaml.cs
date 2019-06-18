using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// PanelListThumbnailImage.xaml の相互作用ロジック
    /// </summary>
    public partial class PanelListThumbnailImage : UserControl
    {
        public PanelListThumbnailImage()
        {
            InitializeComponent();
            this.Root.DataContext = this;
        }


        public IThumbnail Thumbnail
        {
            get { return (IThumbnail)GetValue(ThumbnailProperty); }
            set { SetValue(ThumbnailProperty, value); }
        }

        public static readonly DependencyProperty ThumbnailProperty =
            DependencyProperty.Register("Thumbnail", typeof(IThumbnail), typeof(PanelListThumbnailImage), new PropertyMetadata(null));
    }
}
