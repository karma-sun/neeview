using System;
using System.Collections.Generic;
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

namespace NeeView.Windows.Controls
{
    /// <summary>
    /// SearchBox.xaml の相互作用ロジック
    /// </summary>
    public partial class SearchBox : UserControl
    {
        public SearchBox()
        {
            InitializeComponent();
        }


        public string SearchKeyword
        {
            get { return (string)GetValue(SearchKeywordProperty); }
            set { SetValue(SearchKeywordProperty, value); }
        }

        public static readonly DependencyProperty SearchKeywordProperty =
            DependencyProperty.Register("SearchKeyword", typeof(string), typeof(SearchBox), new PropertyMetadata(null));



        private void SearchTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchBox_UpdateSource();
                e.Handled = true;
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchBox_UpdateSource();
        }

        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            this.SearchTextBox.Text = "";
            SearchBox_UpdateSource();
        }

        private void SearchBox_UpdateSource()
        {
            BindingExpression be = this.SearchTextBox.GetBindingExpression(TextBox.TextProperty);
            be.UpdateSource();
        }
    }
}
