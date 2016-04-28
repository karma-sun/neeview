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
        private void OnFolderCollectionChanged(object sender, EventArgs _e)
        {
            var vm = new FolderListVM();
            vm.FolderCollection = _VM.FolderCollection;
            vm.SelectedIndex = _VM.FolderCollection.SelectedIndex < 0 ? 0 : _VM.FolderCollection.SelectedIndex;
            _FolderList = new FolderList(vm);
            _FolderList.Decided += (s, e) => _VM.BookHub.RequestLoad(e, BookLoadOption.SkipSamePlace, false);
            _FolderList.Moved += (s, e) => _VM.SetPlace(e, null);
            _FolderList.MovedParent += (s, e) => _VM.MoveToParent();
            _FolderList.SelectionChanged += (s, e) => _VM.SelectedIndex = e;

            this.FolderListContent.Content = _FolderList;
        }

        //
        public void SetPlace(string place, string select)
        {
            _VM.SetPlace(place, select);
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

        public Dictionary<FolderOrder, string> FolderOrderList => FolderOrderExtension.FolderOrderList;

        private BookHub _BookHub;
        public BookHub BookHub
        {
            get { return _BookHub; }
            set
            {
                _BookHub = value;
                _BookHub.FolderListSync += (s, e) => SyncWeak(e);
                _BookHub.FolderListReflesh += (s, e) => Reflesh();
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
                FolderCollectionChanged?.Invoke(this, null);
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

        public FolderInfo SelectedItem => (_FolderCollection != null && 0 <= SelectedIndex && SelectedIndex < _FolderCollection.Items.Count) ? _FolderCollection[SelectedIndex] : null; // { get; set; }

        public string Place => _FolderCollection?.Place;

        public string PlaceDispString => string.IsNullOrEmpty(Place) ? "このPC" : Place;

        private Dictionary<string, string> LastPlaceDictionary = new Dictionary<string, string>();

        private bool _IsDarty;

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

        //
        public void SetPlace(string place, string select)
        {
            SavePlace(SelectedItem);

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
                collection.FolderOrder = BookHub.FolderOrder;
                collection.RandomSeed = BookHub._FolderOrderSeed;
                collection.Update(select);
            }

            var index = collection.IndexOfPath(select);
            SelectedIndex = index < 0 ? 0 : index;

            if (collection != FolderCollection)
            {
                FolderCollection?.Dispose();

                FolderCollection = collection;
                FolderCollection.Changed += (s, e) => App.Current.Dispatcher.BeginInvoke((Action)(delegate () { Reflesh(); }));
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
        public void SyncWeak(FolderListSyncArguments e)
        {
            if (e != null && e.isKeepPlace)
            {
                if (!FolderCollection.Contains(e.Path)) return;
            }

            SetPlace(System.IO.Path.GetDirectoryName(e.Path), e.Path);
        }

        //
        public void Sync()
        {
            string place = BookHub?.CurrentBook?.Place;
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

        //
        public void RefleshIcon(string path)
        {
            FolderCollection?.RefleshIcon(path);
        }
    }
}
