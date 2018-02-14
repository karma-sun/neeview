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

namespace NeeView.Configure
{
    /// <summary>
    /// SettingItemCollectionControl.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingItemCollectionControl : UserControl
    {
        public SettingItemCollectionControl()
        {
            InitializeComponent();
        }


        public StringCollection Collection
        {
            get { return (StringCollection)GetValue(CollectionProperty); }
            set { SetValue(CollectionProperty, value); }
        }

        public static readonly DependencyProperty CollectionProperty =
            DependencyProperty.Register("Collection", typeof(StringCollection), typeof(SettingItemCollectionControl), new PropertyMetadata(null));


        public string AddDialogTitle { get; set; }
        public string AddDialogHeader { get; set; }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddParameterDialog();
            dialog.Title = AddDialogTitle ?? "項目の追加";
            dialog.Header = AddDialogHeader;
            dialog.Owner = Window.GetWindow(this);
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();
            if (result == true)
            {
                this.Collection.Add(dialog.Input);
                this.CollectionListBox.Items.Refresh();
                this.CollectionListBox.SelectedItem = dialog.Input;
                this.CollectionListBox.ScrollIntoView(dialog.Input);
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            var item = this.CollectionListBox.SelectedItem as string;
            if (item == null)
            {
                return;
            }

            this.Collection.Remove(item);
        }

    }
}
