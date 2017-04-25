using NeeView.Windows.Controls;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    public class FolderListPanel : IPanel, INotifyPropertyChanged
    {
        /// <summary>
        /// PropertyChanged event. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        //
        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string TypeCode => nameof(FolderListPanel);

        public ImageSource Icon { get; private set; }

        public Thickness IconMargin { get; private set; }

        public string IconTips => "フォルダーリスト";

        private FolderListPanelView _view;
        public FrameworkElement View => _view;

        public bool IsVisibleLock => this.FolderListControl.IsRenaming;

        //
        public FolderListPanel()
        {
            _view = new FolderListPanelView();

            Icon = App.Current.MainWindow.Resources["pic_folder_24px"] as DrawingImage;
            IconMargin = new Thickness(8);
        }

        // TODO: 構築順、どうなの？
        public void Initialize(MainWindowVM vm)
        {
            _view.Initialize(vm);
        }

        // TODO: ここでの呼び出しはおかしい
        public void SetPlace(string place, string select, bool isFocus)
        {
            _view.FolderList.SetPlace(place, select, isFocus);
        }

        // TODO: おかしい
        public FolderListControlView FolderListControl => _view.FolderList;

        // TODO: おかしい
        public PageListControl PageListControl => _view.PageList;
    }

}
