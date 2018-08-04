using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NeeView
{
    /// <summary>
    /// Thumbnail..xaml の相互作用ロジック
    /// </summary>
    public partial class PanelListContentImage : UserControl
    {
        public PanelListContentImage()
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
            DependencyProperty.Register("Thumbnail", typeof(IThumbnail), typeof(PanelListContentImage), new PropertyMetadata(null));
    }


}
