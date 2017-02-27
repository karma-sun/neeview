// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// Messanger: MoveFolderメッセージのパラメータ
    /// </summary>
    public class MoveFolderParams
    {
        public int Distance { get; set; }
        public BookLoadOption BookLoadOption { get; set; }
    }

    /// <summary>
    /// Messenger: FolderOrderメッセージのパラメータ
    /// </summary>
    public class FolderOrderParams
    {
        public FolderOrder FolderOrder { get; set; }
    }

    [Flags]
    public enum FolderSetPlaceOption
    {
        None,
        IsFocus = (1<<0),
        IsUpdateHistory = (1<<1),
        IsTopSelect = (1<<3),
    }


/// <summary>
/// FolderListControl ViewModel
/// </summary>
public class FolderListControlViewModel : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
        #endregion

        /// <summary>
        /// コンボボックス用リスト
        /// </summary>
        public Dictionary<FolderOrder, string> FolderOrderList => FolderOrderExtension.FolderOrderList;

        /// <summary>
        /// BookHub property.
        /// </summary>
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
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// FolderListView property.
        /// </summary>
        private FolderListView _FolderListView;
        public FolderListView FolderListView
        {
            get { return _FolderListView; }
            set
            {
                if (_FolderListView != value)
                {
                    _FolderListView = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(FolderListViewModel));
                }
            }
        }

        /// <summary>
        /// FolderListViewModel property.
        /// </summary>
        public FolderListViewModel FolderListViewModel => FolderListView?.VM;

        /// <summary>
        /// 現在のフォルダ
        /// </summary>
        private string _place => FolderListViewModel?.Place;

        /// <summary>
        /// そのフォルダで最後に選択されていた項目の記憶
        /// </summary>
        private Dictionary<string, string> _lastPlaceDictionary = new Dictionary<string, string>();

        /// <summary>
        /// フォルダ履歴
        /// </summary>
        private History<string> _history = new History<string>();

        /// <summary>
        /// 更新フラグ
        /// </summary>
        private bool _isDarty;

        
        /// <summary>
        /// Constructor
        /// </summary>
        public FolderListControlViewModel()
        {
            _history.Changed += (s, e) => UpdateCommandCanExecute();

            // regist messenger reciever
            Messenger.AddReciever("SetFolderOrder", CallSetFolderOrder);
            Messenger.AddReciever("GetFolderOrder", CallGetFolderOrder);
            Messenger.AddReciever("ToggleFolderOrder", CallToggleFolderOrder);
            Messenger.AddReciever("MoveFolder", CallMoveFolder);
        }


        /// <summary>
        /// フォルダ表示設定
        /// </summary>
        /// <param name="setting"></param>
        public void SetSetting(FolderListSetting setting)
        {
            if (setting == null) return;
            FolderItem.IsVisibleHistoryMark = setting.IsVisibleHistoryMark;
            FolderItem.IsVisibleBookmarkMark = setting.IsVisibleBookmarkMark;
        }

        /// <summary>
        /// フォルダ状態保存
        /// </summary>
        /// <param name="folder"></param>
        private void SavePlace(FolderItem folder)
        {
            if (folder == null || folder.ParentPath == null) return;
            _lastPlaceDictionary[folder.ParentPath] = folder.Path;
        }
        
        /// <summary>
        /// フォルダリスト更新
        /// </summary>
        /// <param name="place">フォルダパス</param>
        /// <param name="select">初期選択項目</param>
        /// <param name="isFocus">フォーカス取得</param>
        /// <param name="updateHistory">フォルダー履歴更新</param>
        //public void SetPlace(string place, string select, bool isFocus, bool updateHistory)

        public void SetPlace(string place, string select, FolderSetPlaceOption options)
        {
            // 現在フォルダの情報を記憶
            SavePlace(this.FolderListViewModel?.GetFolderItem(0));

            // 初期項目
            if (select == null && place != null)
            {
                _lastPlaceDictionary.TryGetValue(place, out select);
            }

            if (options.HasFlag(FolderSetPlaceOption.IsTopSelect))
            {
                select = null;
            }

            // 更新が必要であれば、新しいFolderListViewを作成する
            if (CheckFolderListUpdateneNcessary(place))
            {
                _isDarty = false;

                // FolderListView 更新
                this.FolderListView = CreateFolderListView(place, select, options.HasFlag(FolderSetPlaceOption.IsFocus));

                // 最終フォルダ更新
                ModelContext.BookHistory.LastFolder = _place;

                // 履歴追加
                if (options.HasFlag(FolderSetPlaceOption.IsUpdateHistory))
                {
                    if (place != _history.GetCurrent())
                    {
                        _history.Add(place);
                    }
                }
            }
            else
            {
                // 選択項目のみ変更
                this.FolderListViewModel.SelectedIndex = this.FolderListViewModel.FixedIndexOfPath(select);
            }

            // コマンド有効状態更新
            MoveToUp.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// リストの更新必要性チェック
        /// </summary>
        /// <param name="place"></param>
        /// <returns></returns>
        private bool CheckFolderListUpdateneNcessary(string place)
        {
            return (_isDarty || this.FolderListViewModel == null || place != this.FolderListViewModel.FolderCollection.Place);
        }

        /// <summary>
        /// FolderCollection 作成
        /// </summary>
        /// <param name="place"></param>
        /// <returns></returns>
        private FolderCollection CreateFolderCollection(string place)
        {
            try
            {
                var collection = new FolderCollection(place);
                collection.Initialize();
                return collection;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

                // 救済措置。取得に失敗した時はカレントディレクトリに移動
                var collection = new FolderCollection(Environment.CurrentDirectory);
                collection.Initialize();
                return collection;
            }
        }

        /// <summary>
        /// FolderListView 作成
        /// </summary>
        /// <param name="place"></param>
        /// <param name="select"></param>
        /// <param name="isFocus"></param>
        /// <returns></returns>
        private FolderListView CreateFolderListView(string place, string select, bool isFocus)
        {
            // FolderCollection
            var collection = CreateFolderCollection(place);
            collection.ParameterChanged += (s, e) => App.Current.Dispatcher.BeginInvoke((Action)(delegate () { Reflesh(true, false); }));

            // FolderListViewModel
            var vm = new FolderListViewModel(collection);
            vm.SelectedIndex = vm.FixedIndexOfPath(select);

            // FolderListView
            var view = new FolderListView(vm, isFocus);

            view.Decided += (s, e) => this.BookHub.RequestLoad(e, null, BookLoadOption.SkipSamePlace, false);
            view.Moved += (s, e) => this.SetPlace(e, null, FolderSetPlaceOption.IsFocus | FolderSetPlaceOption.IsUpdateHistory);
            view.MovedParent += (s, e) => this.MoveToParent_Execute();
            view.MovedHome += (s, e) => this.MoveToHome.Execute(null);
            view.MovedPrevious += (s, e) => this.MoveToPrevious.Execute(null);
            view.MovedNext += (s, e) => this.MoveToNext.Execute(null);

            return view;
        }

        /// <summary>
        /// 選択項目にフォーカス取得
        /// </summary>
        /// <param name="isFocus"></param>
        public void FocusSelectedItem(bool isFocus)
        {
            this.FolderListView?.FocusSelectedItem(true);
        }

        /// <summary>
        /// コマンド実行可能状態を更新
        /// </summary>
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
            SetPlace(place, null, FolderSetPlaceOption.IsFocus | FolderSetPlaceOption.IsUpdateHistory | FolderSetPlaceOption.IsTopSelect);
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
            SetPlace(place, null, FolderSetPlaceOption.IsFocus);
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
            SetPlace(place, null, FolderSetPlaceOption.IsFocus);
            _history.Move(+1);
        }


        /// <summary>
        /// 履歴取得
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        internal List<KeyValuePair<int, string>> GetHistory(int direction, int size)
        {
            return _history.GetHistory(direction, size);
        }

        /// <summary>
        /// MoveToHistory command.
        /// </summary>
        private RelayCommand<KeyValuePair<int, string>> _MoveToHistory;
        public RelayCommand<KeyValuePair<int, string>> MoveToHistory
        {
            get { return _MoveToHistory = _MoveToHistory ?? new RelayCommand<KeyValuePair<int, string>>(MoveToHistory_Executed); }
        }

        private void MoveToHistory_Executed(KeyValuePair<int, string> item)
        {
            var place = _history.GetHistory(item.Key);
            SetPlace(place, null, FolderSetPlaceOption.IsFocus);
            _history.SetCurrent(item.Key + 1);
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
            return (_place != null);
        }

        private void MoveToParent_Execute()
        {
            if (_place == null) return;
            var parent = System.IO.Path.GetDirectoryName(_place);
            SetPlace(parent, _place, FolderSetPlaceOption.IsFocus | FolderSetPlaceOption.IsUpdateHistory);
        }


        /// <summary>
        /// Sync command.
        /// 現在開いているフォルダで更新
        /// </summary>
        private RelayCommand _Sync;
        public RelayCommand Sync
        {
            get { return _Sync = _Sync ?? new RelayCommand(Sync_Executed); }
        }

        private void Sync_Executed()
        {
            string place = BookHub?.CurrentBook?.Place;
            if (place != null)
            {
                _isDarty = true; // 強制更新
                SetPlace(System.IO.Path.GetDirectoryName(place), place, FolderSetPlaceOption.IsFocus | FolderSetPlaceOption.IsUpdateHistory);

                FocusSelectedItem(true);
            }
        }



        /// <summary>
        /// 現在開いているフォルダで更新(弱)
        /// e.isKeepPlaceが有効の場合、フォルダは移動せず現在選択項目のみの移動を試みる
        /// </summary>
        /// <param name="e"></param>
        public void SyncWeak(FolderListSyncArguments e)
        {
            if (e != null && e.isKeepPlace)
            {
                if (this.FolderListViewModel == null || !this.FolderListViewModel.Contains(e.Path)) return;
            }

            var options = (e.IsFocus ? FolderSetPlaceOption.IsFocus : FolderSetPlaceOption.None) | FolderSetPlaceOption.IsUpdateHistory;
            SetPlace(System.IO.Path.GetDirectoryName(e.Path), e.Path, options);
        }

        /// <summary>
        /// フォルダリスト更新
        /// </summary>
        /// <param name="force">必要が無い場合も更新する</param>
        /// <param name="isFocus">フォーカスを取得する</param>
        public void Reflesh(bool force, bool isFocus)
        {
            if (this.FolderListViewModel == null) return;

            _isDarty = force || this.FolderListViewModel.IsDarty();

            var options = (isFocus ? FolderSetPlaceOption.IsFocus : FolderSetPlaceOption.None) | FolderSetPlaceOption.IsUpdateHistory;
            SetPlace(_place, null, options);
        }

        /// <summary>
        /// フォルダアイコンの表示更新
        /// </summary>
        /// <param name="path">更新するパス。nullならば全て更新</param>
        public void RefleshIcon(string path)
        {
            this.FolderListViewModel?.RefleshIcon(path);
        }


        /// <summary>
        /// Messenger reciever: フォルダの並びを設定
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CallSetFolderOrder(object sender, MessageEventArgs e)
        {
            var param = (FolderOrderParams)e.Parameter;
            this.FolderListViewModel?.SetFolderOrder(param.FolderOrder);
        }

        /// <summary>
        /// Messenger reciever: フォルダの並びを取得
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CallGetFolderOrder(object sender, MessageEventArgs e)
        {
            if (this.FolderListViewModel == null) return;

            var param = (FolderOrderParams)e.Parameter;
            param.FolderOrder = this.FolderListViewModel.GetFolderOrder();
        }

        /// <summary>
        /// Messenger reciever: フォルダの並びを順番に切り替える
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CallToggleFolderOrder(object sender, MessageEventArgs e)
        {
            this.FolderListViewModel?.ToggleFolderOrder();
        }


        /// <summary>
        /// Messenger reciever: フォルダ前後移動要求
        /// コマンドの「前のフォルダに移動」「次のフォルダへ移動」に対応
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CallMoveFolder(object sender, MessageEventArgs e)
        {
            var param = (MoveFolderParams)e.Parameter;

            var item = this.FolderListViewModel?.GetFolderItem(param.Distance);
            if (item != null)
            {
                SetPlace(_place, item.TargetPath, FolderSetPlaceOption.IsUpdateHistory);
                BookHub.RequestLoad(item.TargetPath, null, param.BookLoadOption, false);
                e.Result = true;
            }
        }
    }
}
