// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
using System.Runtime.Serialization;

namespace NeeView
{
    /// <summary>
    /// ファイル情報ペイン設定
    /// </summary>
    [DataContract]
    public class FolderListSetting
    {
        [DataMember]
        public Dock Dock { get; set; }

        [DataMember]
        public bool IsVisibleHistoryMark { get; set; }

        [DataMember]
        public bool IsVisibleBookmarkMark { get; set; }

        //
        private void Constructor()
        {
            Dock = Dock.Left;
            IsVisibleHistoryMark = true;
            IsVisibleBookmarkMark = true;
        }

        public FolderListSetting()
        {
            Constructor();
        }

        [OnDeserializing]
        private void Deserializing(StreamingContext c)
        {
            Constructor();
        }

        //
        public FolderListSetting Clone()
        {
            return (FolderListSetting)MemberwiseClone();
        }
    }





    /// <summary>
    /// FolderListControl.xaml の相互作用ロジック
    /// </summary>
    public partial class FolderListControl : UserControl
    {
        public FolderListSetting Setting
        {
            get { return (FolderListSetting)GetValue(SettingProperty); }
            set { SetValue(SettingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Setting.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SettingProperty =
            DependencyProperty.Register("Setting", typeof(FolderListSetting), typeof(FolderListControl), new PropertyMetadata(new FolderListSetting(), new PropertyChangedCallback(SettingPropertyChanged)));

        //
        public static void SettingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // オブジェクトを取得して処理する
            FolderListControl ctrl = d as FolderListControl;
            if (ctrl != null)
            {
                ctrl._VM.SetSetting(ctrl.Setting);
            }
        }



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

            _VM = new FolderListControlVM();
            _VM.FolderCollectionChanged += OnFolderCollectionChanged;
            _VM.SelectedItemChanged += OnSelectedItemChanged;
            this.DockPanel.DataContext = _VM;
        }

        //
        private void OnSelectedItemChanged(object sender, EventArgs e)
        {
            _FolderList?.SetSelectedIndex(_VM.SelectedIndex);
        }

        //
        private void OnFolderCollectionChanged(object sender, bool isFocus)
        {
            var vm = new FolderListVM();
            vm.FolderCollection = _VM.FolderCollection;
            vm.SelectedIndex = _VM.FolderCollection.SelectedIndex < 0 ? 0 : _VM.FolderCollection.SelectedIndex;
            _FolderList = new FolderList(vm, isFocus);
            _FolderList.Decided += (s, e) => _VM.BookHub.RequestLoad(e, BookLoadOption.SkipSamePlace, false);
            _FolderList.Moved += (s, e) => _VM.SetPlace(e, null, true);
            _FolderList.MovedParent += (s, e) => _VM.MoveToParent();
            _FolderList.SelectionChanged += (s, e) => _VM.SelectedIndex = e;

            this.FolderListContent.Content = _FolderList;
        }

        //
        public void SetPlace(string place, string select, bool isFocus)
        {
            _VM.SetPlace(place, select, isFocus);
        }

        // フォルダ上階層に移動
        private void FolderUpButton_Click(object sender, RoutedEventArgs e)
        {
            _VM.MoveToParent();
        }

        // フォルダ同期
        private void FolderSyncButton_Click(object sender, RoutedEventArgs e)
        {
            _VM.Sync();
            _FolderList.FocusSelectedItem();
        }
    }



    //
    public class MoveFolderParams
    {
        public int Distance { get; set; }
        public BookLoadOption BookLoadOption { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
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

        public event EventHandler<bool> FolderCollectionChanged;
        public event EventHandler SelectedItemChanged;

        public Dictionary<FolderOrder, string> FolderOrderList => FolderOrderExtension.FolderOrderList;

        private BookHub _BookHub;
        public BookHub BookHub
        {
            get { return _BookHub; }
            set
            {
                _BookHub = value;
                _BookHub.FolderListSync += (s, e) => SyncWeak(e);
                _BookHub.HistoryChanged += (s, e) => RefleshIcon(e.Key);
                _BookHub.BookmarkChanged += (s, e) => RefleshIcon(e.Key);
                OnPropertyChanged();
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
                OnPropertyChanged();
                OnPropertyChanged(nameof(Place));
                OnPropertyChanged(nameof(PlaceDispString));
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

        public FolderInfo SelectedItem => (_FolderCollection != null && 0 <= SelectedIndex && SelectedIndex < _FolderCollection.Items.Count) ? _FolderCollection[SelectedIndex] : null;

        public string Place => _FolderCollection?.Place;

        public string PlaceDispString => string.IsNullOrEmpty(Place) ? "このPC" : Place;

        private Dictionary<string, string> LastPlaceDictionary = new Dictionary<string, string>();

        private bool _IsDarty;

        //
        public FolderListControlVM()
        {
            Messenger.AddReciever("SetFolderOrder", CallSetFolderOrder);
            Messenger.AddReciever("ToggleFolderOrder", CallToggleFolderOrder);
            Messenger.AddReciever("MoveFolder", CallMoveFolder);
        }


        //
        public void SetSetting(FolderListSetting setting)
        {
            if (setting == null) return;
            FolderInfo.IsVisibleHistoryMark = setting.IsVisibleHistoryMark;
            FolderInfo.IsVisibleBookmarkMark = setting.IsVisibleBookmarkMark;
        }

        //
        private void SavePlace(FolderInfo folder)
        {
            if (folder == null || folder.ParentPath == null) return;
            LastPlaceDictionary[folder.ParentPath] = folder.Path;
        }

        private FolderInfo GetFolderInfo(int index)
        {
            return (_FolderCollection != null && 0 <= index && index < _FolderCollection.Items.Count) ? _FolderCollection[index] : null;
        }

        //
        private FolderInfo GetExistSelectedItem()
        {
            if (_FolderCollection == null || FolderCollection.Items.Count <= 0) return null;

            for (int index = SelectedIndex;  index < _FolderCollection.Items.Count; ++index)
            {
                var folder = _FolderCollection[index];
                if (folder.IsExist()) return folder;
            }

            for (int index = SelectedIndex - 1; index >= 0; --index)
            {
                var folder = _FolderCollection[index];
                if (folder.IsExist()) return folder;
            }

            return null;
        }

        //
        public void SetPlace(string place, string select, bool isFocus)
        {
            SavePlace(GetExistSelectedItem());

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
                collection.Folder = new Folder(place);
                collection.Update(select);

                ModelContext.BookHistory.LastFolder = place;
            }

            var index = collection.IndexOfPath(select);
            SelectedIndex = index < 0 ? 0 : index;

            if (collection != FolderCollection)
            {
                FolderCollection?.Dispose();

                FolderCollection = collection;
                FolderCollectionChanged?.Invoke(this, isFocus);

                FolderCollection.Changed += (s, e) => App.Current.Dispatcher.BeginInvoke((Action)(delegate () { Reflesh(true); }));
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
            SetPlace(FolderCollection.ParentPlace, FolderCollection.Place, true);
        }

        //
        public void SyncWeak(FolderListSyncArguments e)
        {
            if (e != null && e.isKeepPlace)
            {
                if (!FolderCollection.Contains(e.Path)) return;
            }

            SetPlace(System.IO.Path.GetDirectoryName(e.Path), e.Path, e.IsFocus);
        }

        //
        public void Sync()
        {
            string place = BookHub?.CurrentBook?.Place;
            if (place != null)
            {
                _IsDarty = true;
                SetPlace(System.IO.Path.GetDirectoryName(place), place, true);
            }
        }

        //
        public void Reflesh(bool force)
        {
            if (FolderCollection == null) return;

            _IsDarty = force || FolderCollection.IsDarty();
            SetPlace(FolderCollection.Place, null, true);
        }

        //
        public void RefleshIcon(string path)
        {
            FolderCollection?.RefleshIcon(path);
        }

        //
        private void CallSetFolderOrder(object sender, MessageEventArgs e)
        {
            if (FolderCollection == null) return;

            var param = (FolderOrder)e.Parameter;
            FolderCollection.Folder.FolderOrder = param;
        }

        //
        private void CallToggleFolderOrder(object sender, MessageEventArgs e)
        {
            if (FolderCollection == null) return;

            FolderCollection.Folder.FolderOrder = FolderCollection.Folder.FolderOrder.GetToggle();
        }


        //
        private void CallMoveFolder(object sender, MessageEventArgs e)
        {
            var param = (MoveFolderParams)e.Parameter;

            e.Result = MoveFolder(param.Distance, param.BookLoadOption);
        }

        //
        private bool MoveFolder(int direction, BookLoadOption option)
        {
            if (FolderCollection == null) return false;

            int index = SelectedIndex;
            if (index < 0) return false;

            int next = (FolderCollection.Folder.FolderOrder == FolderOrder.Random)
                ? (index + FolderCollection.Items.Count + direction) % FolderCollection.Items.Count
                : index + direction;

            if (next < 0 || next >= FolderCollection.Items.Count) return false;

            SetPlace(FolderCollection.Place, FolderCollection[next].Path, false);
            BookHub.RequestLoad(FolderCollection[next].Path, option, false);

            return true;
        }

    }
}
