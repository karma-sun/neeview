using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
    /// SettingItemPreferenceControl.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingItemPreferenceControl : UserControl
    {
        public SettingItemPreferenceControl()
        {
            InitializeComponent();

            this.Root.DataContext = this;

            // ##
            var setting = SaveData.Current.CreateSetting();


            // 詳細設定一覧作成
            _propertyDocument = new PropertyDocument(new object[]
                {
                    setting.App,
                    setting.Memento.JobEngine,
                    setting.Memento.FileIOProfile,

                    setting.Memento.PictureProfile,
                    setting.Memento.ImageFilter,

                    setting.Memento.ArchiverManager,
                    setting.Memento.SevenZipArchiverProfile,
                    setting.Memento.PdfArchiverProfile,
                    setting.Memento.ThumbnailProfile,
                    setting.Memento.MainWindowModel,
                    setting.Memento.FolderList,
                    setting.Memento.SidePanelProfile,
                    setting.Memento.SidePanel,
                    setting.Memento.ThumbnailList,
                    setting.Memento.MenuBar,
                    setting.Memento.BookProfile,
                    setting.Memento.BookHub,

                    setting.Memento.MouseInput.Normal,
                    setting.Memento.MouseInput.Gesture,
                    setting.Memento.MouseInput.Loupe,

                    setting.Memento.TouchInput.Gesture,
                    setting.Memento.TouchInput.Drag,
                    setting.Memento.TouchInput.Drag.Manipulation,
                });
            PropertyCollection = new ObservableCollection<PropertyParam>();
            UpdatePropertyList();

            foreach(var item in PropertyCollection)
            {
                Debug.WriteLine($"    * {item.Name}[{item.TypeString}] : {item.Key}");
            }
        }

        #region Preference

        //
        private PropertyDocument _propertyDocument;

        // 詳細設定一覧用パラメータ
        public class PropertyParam
        {
            public PropertyMemberElement Source { get; set; }

            public string Key => Source.Path;
            public string Name => Source.Name;
            public string State => Source.HasCustomValue ? "ユーザ設定" : "初期設定値";
            public string TypeString => Source.GetValueTypeString();
            public string Value => Source.GetValueString();
            public string Tips => Source.Tips;
        }

        // 詳細設定一覧
        public ObservableCollection<PropertyParam> PropertyCollection { get; set; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PropertyEditButton_Click(object sender, RoutedEventArgs e)
        {
            var value = (PropertyParam)this.PropertyListView.SelectedValue;
            EnditProperty(value.Source, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetAllPropertyButton_Click(object sender, RoutedEventArgs e)
        {
            _propertyDocument.Reset();
            this.PropertyListView.Items.Refresh();
        }


        // 詳細一覧 更新
        private void UpdatePropertyList()
        {
            PropertyCollection.Clear();

            foreach (var element in _propertyDocument.PropertyMembers.Where(e => !e.IsObsolete))
            {
                if (element.Path.StartsWith("_")) continue;

                var item = new PropertyParam()
                {
                    Source = element,
                };
                PropertyCollection.Add(item);
            }
        }

        //
        private void PropertyListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // sender がダブルクリックされた項目
            ListViewItem targetItem = (ListViewItem)sender;

            // データバインディングを使っているなら、
            // DataContext からデータを取得できる
            PropertyParam p = (PropertyParam)targetItem.DataContext;
            EnditProperty(p.Source, true);
        }

        //
        private void EnditProperty(PropertyMemberElement param, bool isSimple)
        {
            if (isSimple && param.GetValueType() == typeof(bool))
            {
                param.SetValue(!(bool)param.GetValue());
                this.PropertyListView.Items.Refresh();
            }
            else
            {
                var dialog = new PropertyEditWindow(param);
                dialog.Owner = Window.GetWindow(this);
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                var result = dialog.ShowDialog();
                if (result == true)
                {
                    this.PropertyListView.Items.Refresh();
                }
            }
        }

        #endregion
    }
}
