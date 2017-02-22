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
using System.Diagnostics;

namespace NeeView
{
    /// <summary>
    /// フォルダリストの表示方法
    /// </summary>
    public enum FolderListItemStyle
    {
        Normal, // テキストのみ
        Picture, // バナー付き
    };

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



        private FolderListControlVM _VM;
        private FolderList _folderList;

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
            _folderList?.SetSelectedIndex(_VM.SelectedIndex);
        }

        //
        private void OnFolderCollectionChanged(object sender, bool isFocus)
        {
            var vm = new FolderListViewModel(_VM.FolderCollection);
            vm.SelectedIndex = _VM.FolderCollection.SelectedIndex < 0 ? 0 : _VM.FolderCollection.SelectedIndex;
            _folderList = new FolderList(vm, isFocus);
            _folderList.Decided += (s, e) => _VM.BookHub.RequestLoad(e, null, BookLoadOption.SkipSamePlace, false);
            _folderList.Moved += (s, e) => _VM.SetPlace(e, null, true, true);
            _folderList.MovedParent += (s, e) => _VM.MoveToParent_Execute();
            _folderList.SelectionChanged += (s, e) => _VM.SelectedIndex = e;
            _folderList.MovedHome += (s, e) => _VM.MoveToHome.Execute(null);
            _folderList.MovedPrevious += (s, e) => _VM.MoveToPrevious.Execute(null);
            _folderList.MovedNext += (s, e) => _VM.MoveToNext.Execute(null);

            this.FolderListContent.Content = _folderList;
        }


        //
        public void SetPlace(string place, string select, bool isFocus)
        {
            _VM.SetPlace(place, select, isFocus, true);
        }


        // フォルダ同期
        private void FolderSyncButton_Click(object sender, RoutedEventArgs e)
        {
            _VM.Sync();
            _folderList.FocusSelectedItem(true);
        }


        //
        private async void FolderList_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                await Task.Yield();
                _folderList.FocusSelectedItem(true);
            }
        }
    }



    //
    public class MoveFolderParams
    {
        public int Distance { get; set; }
        public BookLoadOption BookLoadOption { get; set; }
    }

    //
    public class FolderOrderParams
    {
        public FolderOrder FolderOrder { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class FolderListControlVM : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
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

        private BookHub _bookHub;
        public BookHub BookHub
        {
            get { return _bookHub; }
            set
            {
                _bookHub = value;
                _bookHub.FolderListSync += (s, e) => SyncWeak(e);
                _bookHub.HistoryChanged += (s, e) => RefleshIcon(e.Key);
                _bookHub.BookmarkChanged += (s, e) => RefleshIcon(e.Key);
                //_BookHub.PagemarkChanged += (s, e) => RefleshIcon(e.Key);
                RaisePropertyChanged();
            }
        }

        #region Property: FolderCollection
        private FolderCollection _folderCollection;
        public FolderCollection FolderCollection
        {
            get { return _folderCollection; }
            private set
            {
                _folderCollection = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Place));
                RaisePropertyChanged(nameof(PlaceDispString));
            }
        }
        #endregion


        #region Property: SelectedIndex
        private int _selectedIndex;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set { _selectedIndex = value; RaisePropertyChanged(); }
        }
        #endregion

        public FolderInfo SelectedItem => (_folderCollection != null && 0 <= SelectedIndex && SelectedIndex < _folderCollection.Items.Count) ? _folderCollection[SelectedIndex] : null;

        public string Place => _folderCollection?.Place;

        public string PlaceDispString => string.IsNullOrEmpty(Place) ? "このPC" : Place;

        /// <summary>
        /// そのフォルダで最後に選択されていた項目の記憶
        /// </summary>
        private Dictionary<string, string> _lastPlaceDictionary = new Dictionary<string, string>();

        /// <summary>
        /// フォルダ履歴
        /// </summary>
        private History<string> _history = new History<string>();


        private bool _isDarty;

        //
        public FolderListControlVM()
        {
            _history.Changed += (s, e) => UpdateCommandCanExecute();

            Messenger.AddReciever("SetFolderOrder", CallSetFolderOrder);
            Messenger.AddReciever("GetFolderOrder", CallGetFolderOrder);
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
            _lastPlaceDictionary[folder.ParentPath] = folder.Path;
        }

        private FolderInfo GetFolderInfo(int index)
        {
            return (_folderCollection != null && 0 <= index && index < _folderCollection.Items.Count) ? _folderCollection[index] : null;
        }

        /// <summary>
        /// 有効な選択項目取得
        /// 削除された等で位置が変更になった場合にずらす？
        /// 必要性に疑問
        /// </summary>
        /// <returns></returns>
        private FolderInfo GetExistSelectedItem()
        {
            if (_folderCollection == null || FolderCollection.Items.Count <= 0) return null;
            if (SelectedIndex < 0) return null;

            for (int index = SelectedIndex; index < _folderCollection.Items.Count; ++index)
            {
                var folder = _folderCollection[index];
                if (folder.IsExist()) return folder;
            }

            for (int index = SelectedIndex - 1; index >= 0; --index)
            {
                var folder = _folderCollection[index];
                if (folder.IsExist()) return folder;
            }

            return null;
        }

        //
        public void SetPlace(string place, string select, bool isFocus, bool updateHistory)
        {
            SavePlace(GetExistSelectedItem());

            if (select == null && place != null)
            {
                _lastPlaceDictionary.TryGetValue(place, out select);
            }

            FolderCollection collection = FolderCollection;

            if (FolderCollection == null || _isDarty || place != FolderCollection.Place)
            {
                _isDarty = false;
                try
                {
                    var newCollection = new FolderCollection();
                    newCollection.Place = place;
                    newCollection.Folder = new Folder(place);
                    newCollection.Update(select);
                    collection = newCollection;
                    ModelContext.BookHistory.LastFolder = place;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }

                // 救済措置。取得に失敗した時はカレントディレクトリに移動
                if (collection == null)
                {
                    place = Environment.CurrentDirectory;
                    var newCollection = new FolderCollection();
                    newCollection.Place = place;
                    newCollection.Folder = new Folder(place);
                    newCollection.Update(select);
                    collection = newCollection;
                    ModelContext.BookHistory.LastFolder = place;
                }
            }


            var index = collection.IndexOfPath(select);
            SelectedIndex = index < 0 ? 0 : index;

            if (collection != FolderCollection)
            {
                FolderCollection?.Dispose();

                FolderCollection = collection;
                FolderCollectionChanged?.Invoke(this, isFocus);

                FolderCollection.Changed += (s, e) => App.Current.Dispatcher.BeginInvoke((Action)(delegate () { Reflesh(true, false); }));
            }
            else
            {
                SelectedItemChanged?.Invoke(this, null);
            }

            // 履歴追加
            if (updateHistory)
            {
                if (place != _history.GetCurrent())
                {
                    //Debug.WriteLine("ADD:" + place);
                    _history.Add(place);
                }
            }

            //
            MoveToUp.RaiseCanExecuteChanged();
        }

        //
        private void UpdateCommandCanExecute()
        {
            this.MoveToPrevious.RaiseCanExecuteChanged();
            this.MoveToNext.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// MoveToHome command.
        /// </summary>
        private RelayCommand _MoveToHome;
        public RelayCommand MoveToHome
        {
            get { return _MoveToHome = _MoveToHome ?? new RelayCommand(MoveToHome_Executed); }
        }

        private void MoveToHome_Executed()
        {
            if (_bookHub == null) return;

            var place = _bookHub.GetFixedHome();
            SetPlace(place, FolderCollection.Place, true, true);
        }



        /// <summary>
        /// MoveToPrevious command.
        /// </summary>
        private RelayCommand _MoveToPrevious;
        public RelayCommand MoveToPrevious
        {
            get { return _MoveToPrevious = _MoveToPrevious ?? new RelayCommand(MoveToPrevious_Executed, MoveToPrevious_CanExecutre); }
        }

        private bool MoveToPrevious_CanExecutre()
        {
            return _history.CanPrevious();
        }

        private void MoveToPrevious_Executed()
        {
            if (!_history.CanPrevious()) return;

            var place = _history.GetPrevious();
            SetPlace(place, FolderCollection.Place, true, false);
            _history.Move(-1);
        }

        /// <summary>
        /// MoveToNext command.
        /// </summary>
        private RelayCommand _MoveToNext;
        public RelayCommand MoveToNext
        {
            get { return _MoveToNext = _MoveToNext ?? new RelayCommand(MoveToNext_Executed, MoveToNext_CanExecute); }
        }

        private bool MoveToNext_CanExecute()
        {
            return _history.CanNext();
        }

        private void MoveToNext_Executed()
        {
            if (!_history.CanNext()) return;

            var place = _history.GetNext();
            SetPlace(place, FolderCollection.Place, true, false);
            _history.Move(+1);
        }


        /// <summary>
        /// MoveToUp command.
        /// </summary>
        private RelayCommand _MoveToUp;
        public RelayCommand MoveToUp
        {
            get { return _MoveToUp = _MoveToUp ?? new RelayCommand(MoveToParent_Execute, MoveToParent_CanExecute); }
        }

        private bool MoveToParent_CanExecute()
        {
            return (FolderCollection?.Place != null);
        }

        //
        public void MoveToParent_Execute()
        {
            if (FolderCollection?.Place == null) return;
            SetPlace(FolderCollection.ParentPlace, FolderCollection.Place, true, true);
        }

        //
        public void SyncWeak(FolderListSyncArguments e)
        {
            if (e != null && e.isKeepPlace)
            {
                if (!FolderCollection.Contains(e.Path)) return;
            }

            SetPlace(System.IO.Path.GetDirectoryName(e.Path), e.Path, e.IsFocus, true);
        }

        //
        public void Sync()
        {
            string place = BookHub?.CurrentBook?.Place;
            if (place != null)
            {
                _isDarty = true;
                SetPlace(System.IO.Path.GetDirectoryName(place), place, true, true);
            }
        }

        //
        public void Reflesh(bool force, bool isFocus)
        {
            if (FolderCollection == null) return;

            _isDarty = force || FolderCollection.IsDarty();
            SetPlace(FolderCollection.Place, null, isFocus, true);
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

            var param = (FolderOrderParams)e.Parameter;
            FolderCollection.Folder.FolderOrder = param.FolderOrder;
        }

        //
        private void CallGetFolderOrder(object sender, MessageEventArgs e)
        {
            if (FolderCollection == null) return;

            var param = (FolderOrderParams)e.Parameter;
            param.FolderOrder = FolderCollection.Folder.FolderOrder;
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

            SetPlace(FolderCollection.Place, FolderCollection[next].TargetPath, false, true);
            BookHub.RequestLoad(FolderCollection[next].TargetPath, null, option, false);

            return true;
        }
    }

    /// <summary>
    /// 履歴
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class History<T>
    {
        public event EventHandler Changed;

        /// <summary>
        /// フォルダー履歴
        /// </summary>
        private List<T> _history = new List<T>();

        /// <summary>
        /// 現在履歴位置。0で先頭
        /// </summary>
        private int _current;

        public void Add(T path)
        {
            if (_current != _history.Count)
            {
                _history = _history.Take(_current).ToList();
            }
            _history.Add(path);
            _current = _history.Count;
            Changed?.Invoke(this, null);
        }

        public void Move(int delta)
        {
            _current = NVUtility.Clamp(_current + delta, 0, _history.Count);
            Changed?.Invoke(this, null);
        }

        public T GetCurrent()
        {
            var index = _current - 1;
            return (index >= 0) ? _history[index] : default(T);
        }

        public bool CanPrevious()
        {
            return _current - 2 >= 0;
        }

        public T GetPrevious()
        {
            var index = _current - 2;
            return (index >= 0) ? _history[index] : default(T);
        }

        public bool CanNext()
        {
            return _current < _history.Count;
        }

        public T GetNext()
        {
            return (_current < _history.Count) ? _history[_current] : default(T);
        }
    }
}
