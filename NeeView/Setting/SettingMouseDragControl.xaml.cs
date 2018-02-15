using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace NeeView.Setting
{
    /// <summary>
    /// SettingMouseDragControl.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingMouseDragControl : UserControl
    {
        public SettingMouseDragControl()
        {
            InitializeComponent();

            Initialize();
            this.Root.DataContext = this;
        }

        #region DragAction

        // TODO: ひとまず動作のため。本来はMementoでなく、直接現在データを編集する
        private DragActionTable.Memento _memento;

        public void Initialize()
        {
            _memento = DragActionTable.Current.CreateMemento();

            // ドラッグアクション一覧作成
            DragActionCollection = new ObservableCollection<DragActionParam>();
            UpdateDragActionList();
        }

        // ドラッグ一覧専用パラメータ
        public class DragActionParam : BindableBase
        {
            public DragActionType Key { get; set; }
            public string Header { get; set; }
            public bool IsLocked { get; set; }


            private string _dragAction;
            public string DragAction
            {
                get { return _dragAction; }
                set { if (_dragAction != value) { _dragAction = value; RaisePropertyChanged(); } }
            }

            public string Tips { get; set; }
        }

        // コマンド一覧
        public ObservableCollection<DragActionParam> DragActionCollection { get; set; }

        //
        private Window GetOwner()
        {
            return Window.GetWindow(this);
        }

        //
        private void UpdateDragActionList()
        {
            DragActionCollection.Clear();
            foreach (var element in DragActionTable.Current)
            {
                ////var memento = Setting.DragActionMemento[element.Key];
                var memento = _memento[element.Key];

                var item = new DragActionParam()
                {
                    Key = element.Key,
                    Header = element.Key.ToLabel(),
                    IsLocked = element.Value.IsLocked,
                    DragAction = memento.Key,
                    Tips = element.Key.ToTips(),
                };

                DragActionCollection.Add(item);
            }

            this.DragActionListView.Items.Refresh();
        }

        //
        private void DragActionListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // nop.
        }

        //
        private void DragActionListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListViewItem targetItem = (ListViewItem)sender;

            var value = (DragActionParam)targetItem.DataContext;
            OpenDragActionSettingDialog(value);
        }

        //
        private void DragActionSettingButton_Click(object sender, RoutedEventArgs e)
        {
            var value = (DragActionParam)this.DragActionListView.SelectedValue;
            OpenDragActionSettingDialog(value);
        }

        //
        private void OpenDragActionSettingDialog(DragActionParam value)
        {
            if (value.IsLocked)
            {
                var dlg = new MessageDialog("", "この操作は変更できません");
                dlg.Owner = GetOwner();
                dlg.ShowDialog();
                return;
            }

            var context = new MouseDragSettingContext();
            context.Command = value.Key;
            context.Gestures = DragActionCollection.ToDictionary(i => i.Key, i => i.DragAction);

            var dialog = new MouseDragSettingWindow(context);
            dialog.Owner = GetOwner();
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var result = dialog.ShowDialog();
            if (result == true)
            {
                foreach (var item in DragActionCollection)
                {
                    item.DragAction = context.Gestures[item.Key];
                }

                this.DragActionListView.Items.Refresh();

                //// ##
                Restore();
            }
        }

        //
        private void ResetDragActionSettingButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog($"すべてのドラッグ操作を初期化します。よろしいですか？", "ドラッグ操作を初期化します");
            dialog.Commands.Add(UICommands.Yes);
            dialog.Commands.Add(UICommands.No);
            dialog.Owner = GetOwner();
            var answer = dialog.ShowDialog();

            if (answer == UICommands.Yes)
            {
                ////Setting.DragActionMemento = DragActionTable.CreateDefaultMemento();
                _memento = DragActionTable.CreateDefaultMemento();

                UpdateDragActionList();
                this.DragActionListView.Items.Refresh();

                //// ##
                Restore();
            }
        }

        // ##
        private void Restore ()
        {
            foreach (var dragAction in DragActionCollection)
            {
                _memento[dragAction.Key].Key = dragAction.DragAction;
            }
            DragActionTable.Current.Restore(_memento);
        }

        #endregion
    }
}
