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
using System.ComponentModel;

namespace NeeView
{
    // TODO: SavePlaceを_VMだけで完結できるようにする

    /// <summary>
    /// FolderListControl.xaml の相互作用ロジック
    /// </summary>
    public partial class FolderListControl : UserControl
    {
        FolderListControlVM _VM;
        FolderList _FolderList;

        public BookHub BookHub
        {
            get { return (BookHub)GetValue(BookHubProperty); }
            set { SetValue(BookHubProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BookHub.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BookHubProperty =
            DependencyProperty.Register("BookHub", typeof(BookHub), typeof(FolderListControl), new PropertyMetadata(null, new PropertyChangedCallback(BookHubPropertyChanged)));

        //
        public static void BookHubPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // オブジェクトを取得して処理する
            FolderListControl ctrl = d as FolderListControl;
            if (ctrl != null)
            {
                ctrl._VM.BookHub = ctrl.BookHub;
            }
        }


        //
        public FolderListControl()
        {
            InitializeComponent();

            this.Style = this.Resources["BlackStyle"] as Style;

            _VM = new FolderListControlVM();
            _VM.FolderCollectionChanged += OnFolderCollectionChanged;
            _VM.SelectedItemChanged += OnSelectedItemChanged;
            _VM.FolderListSync += OnFolderListSync;
            _VM.FolderListReflesh += OnFolderListReflesh;
            this.DockPanel.DataContext = _VM;
        }

        private void OnFolderListReflesh(object sender, EventArgs e)
        {
            _VM.SavePlace(_FolderList?.SelectedItem);
            _VM.Reflesh();
            //throw new NotImplementedException();
        }

        private void OnFolderListSync(object sender, FolderListSyncArguments e)
        {
            if (e.isKeepPlace)
            {
                if (!_VM.FolderCollection.Contains(e.Path)) return;
            }

            _VM.SavePlace(_FolderList?.SelectedItem);
            _VM.SetPlace(System.IO.Path.GetDirectoryName(e.Path), e.Path);
        }

        private void OnSelectedItemChanged(object sender, EventArgs e)
        {
            _FolderList?.SetSelectedIndex(_VM.SelectedIndex);
        }

        //
        private void OnFolderCollectionChanged(object sender, EventArgs e)
        {
            var vm = new FolderListVM();
            vm.FolderCollection = _VM.FolderCollection;
            vm.SelectedIndex = _VM.FolderCollection.SelectedIndex < 0 ? 0 : _VM.FolderCollection.SelectedIndex;
            _FolderList = new FolderList(vm);
            _FolderList.Decided += FolderList_Decided;
            _FolderList.Moved += FolderList_Moved;
            _FolderList.MovedParent += FolderList_MovedParent;
            //_FolderList.Style = this.Resources["BlackStyle"] as Style;

            this.FolderListContent.Content = _FolderList;
        }

        private void FolderList_MovedParent(object sender, string e)
        {
            _VM.SavePlace(_FolderList?.SelectedItem);
            _VM.MoveToParent();
        }

        private void FolderList_Moved(object sender, string e)
        {
            _VM.SavePlace(_FolderList?.SelectedItem);
            _VM.SetPlace(e, null);
        }

        private void FolderList_Decided(object sender, string e)
        {
            _VM.BookHub.RequestLoad(e, BookLoadOption.None, false);
        }

        public void SetPlace(string place, string select)
        {
            _VM.SavePlace(_FolderList?.SelectedItem);
            _VM.SetPlace(place, select);
        }

        public void Sync()
        {
            _VM.SavePlace(_FolderList?.SelectedItem);
            _VM.Sync();
        }

        // フォルダ上階層に移動
        private void FolderUpButton_Click(object sender, RoutedEventArgs e)
        {
            _VM.MoveToParent();
        }

        // フォルダ同期
        private void FolderSyncButton_Click(object sender, RoutedEventArgs e)
        {
            Sync();
            _FolderList.FocusSelectedItem();
        }
    }

    public class FolderListControlVM : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion

        public event EventHandler FolderCollectionChanged;
        public event EventHandler SelectedItemChanged;
        public event EventHandler<FolderListSyncArguments> FolderListSync;
        public event EventHandler FolderListReflesh;

        private BookHub _BookHub;
        public BookHub BookHub
        {
            get { return _BookHub; }
            set
            {
                _BookHub = value;
                _BookHub.FolderListSync += (s, e) => FolderListSync?.Invoke(s, e);
                _BookHub.FolderListReflesh += (s, e) => FolderListReflesh?.Invoke(s, e); 
            }
        }

        #region Property: FolderCollection
        private FolderCollection _FolderCollection;
        public FolderCollection FolderCollection
        {
            get { return _FolderCollection; }
            private set
            {
                _FolderCollection = value;
                FolderCollectionChanged?.Invoke(this, null);
                OnPropertyChanged(nameof(Place));
            }
        }
        #endregion


        #region Property: SelectedIndex
        private int _SelectedIndex;
        public int SelectedIndex
        {
            get { return _SelectedIndex; }
            set { _SelectedIndex = value; OnPropertyChanged(); }
        }
        #endregion

        public string Place => _FolderCollection?.Place;

        private Dictionary<string, string> LastPlaceDictionary = new Dictionary<string, string>();

        private bool _IsDarty;

        //public FolderListControlVM()
        //{
        //    SetPlace(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), null);
        //}


        //
        public void SavePlace(FolderInfo folder)
        {
            if (folder == null || folder.ParentPath == null) return;
            LastPlaceDictionary[folder.ParentPath] = folder.Path;
        }

        //
        public void SetPlace(string place, string select)
        {
            if (select == null && place != null)
            {
                LastPlaceDictionary.TryGetValue(place, out select);
            }

            FolderCollection collection = FolderCollection;

            if (FolderCollection == null || _IsDarty || place != FolderCollection.Place)
            {
                _IsDarty = false;
                collection = new FolderCollection();
                collection.Place = place;
                if (select != null) collection.SelectedBook = select;
                collection.FolderOrder = BookHub.FolderOrder; // FolderOrder.FileName; // TODO:
                collection.RandomSeed = BookHub._FolderOrderSeed; // 0; // TODO:
                collection.Update(select, true, true);
            }

            var index = collection.IndexOfPath(select);
            SelectedIndex = index < 0 ? 0 : index;

            if (collection != FolderCollection)
            {
                FolderCollection = collection;
            }
            else
            {
                SelectedItemChanged?.Invoke(this, null);
            }
        }

        //
        public void MoveToParent()
        {
            if (FolderCollection == null) return;
            SetPlace(FolderCollection.ParentPlace, FolderCollection.Place);
        }

        //
        public void Sync()
        {
            string place = BookHub?.Current?.Place;
            if (place != null)
            {
                _IsDarty = true;
                SetPlace(System.IO.Path.GetDirectoryName(place), place);
            }
        }

        //
        public void Reflesh()
        {
            if (FolderCollection == null) return;

            _IsDarty = true;
            SetPlace(FolderCollection.Place, null);
        }
    }
}
