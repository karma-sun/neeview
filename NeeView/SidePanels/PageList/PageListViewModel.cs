using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows.Controls;
using System.Windows.Data;

namespace NeeView
{
    public class ViewItemsChangedEventArgs : EventArgs
    {
        public ViewItemsChangedEventArgs(List<Page> pages, int direction)
        {
            this.ViewItems = pages;
            this.Direction = direction;
        }

        public List<Page> ViewItems { get; set; }
        public int Direction { get; set; }
    }

    public class PageListViewModel : BindableBase
    {
        private PageList _model;


        public PageListViewModel(PageList model)
        {
            _model = model;
            _model.AddPropertyChanged(nameof(PageList.PageSortMode), (s, e) => RaisePropertyChanged(nameof(PageSortMode)));

                _model.PageHistoryChanged += 
                (s, e) => UpdateMoveToHistoryCommandCanExecute();

            _model.CollectionChanged +=
                (s, e) => UpdateMoveToUpCommandCanExecute();

            InitializeCommands();

            MoreMenuDescription = new PageListMoreMenuDescription(this);
        }

        public Dictionary<PageNameFormat, string> FormatList { get; } = AliasNameExtensions.GetAliasNameDictionary<PageNameFormat>();

        public Dictionary<PageSortMode, string> PageSortModeList { get; } = AliasNameExtensions.GetAliasNameDictionary<PageSortMode>();

        public PageSortMode PageSortMode
        {
            get => _model.PageSortMode;
            set => _model.PageSortMode = value;
        }

        public PageList Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }


        #region Commands

        public RelayCommand MoveToPreviousCommand { get; private set; }
        public RelayCommand MoveToNextCommand { get; private set; }
        public RelayCommand<KeyValuePair<int, PageHistoryUnit>> MoveToHistoryCommand { get; private set; }
        public RelayCommand MoveToUpCommand { get; private set; }

        private void InitializeCommands()
        {
            MoveToPreviousCommand = new RelayCommand(_model.MoveToPrevious, _model.CanMoveToPrevious);
            MoveToNextCommand = new RelayCommand(_model.MoveToNext, _model.CanMoveToNext);
            MoveToHistoryCommand = new RelayCommand<KeyValuePair<int, PageHistoryUnit>>(_model.MoveToHistory);
            MoveToUpCommand = new RelayCommand(_model.MoveToParent, _model.CanMoveToParent);
        }


        private RelayCommand<PanelListItemStyle> _setListItemStyle;
        public RelayCommand<PanelListItemStyle> SetListItemStyle
        {
            get { return _setListItemStyle = _setListItemStyle ?? new RelayCommand<PanelListItemStyle>(SetListItemStyle_Executed); }
        }

        private void SetListItemStyle_Executed(PanelListItemStyle style)
        {
            Config.Current.PageList.PanelListItemStyle = style;
        }

        #endregion Commands

        #region MoreMenu

        public PageListMoreMenuDescription MoreMenuDescription { get; }

        public class PageListMoreMenuDescription : ItemsListMoreMenuDescription
        {
            private PageListViewModel _vm;

            public PageListMoreMenuDescription(PageListViewModel vm)
            {
                _vm = vm;
            }

            public override ContextMenu Create()
            {
                var menu = new ContextMenu();
                menu.Items.Add(CreateListItemStyleMenuItem(Properties.Resources.Word_StyleList, PanelListItemStyle.Normal));
                menu.Items.Add(CreateListItemStyleMenuItem(Properties.Resources.Word_StyleContent, PanelListItemStyle.Content));
                menu.Items.Add(CreateListItemStyleMenuItem(Properties.Resources.Word_StyleBanner, PanelListItemStyle.Banner));
                menu.Items.Add(CreateListItemStyleMenuItem(Properties.Resources.Word_StyleThumbnail, PanelListItemStyle.Thumbnail));
                return menu;
            }

            private MenuItem CreateListItemStyleMenuItem(string header, PanelListItemStyle style)
            {
                return CreateListItemStyleMenuItem(header, _vm.SetListItemStyle, style, Config.Current.PageList);
            }
        }

        #endregion

        public List<KeyValuePair<int, PageHistoryUnit>> GetHistory(int direction, int size)
        {
            return _model.GetHistory(direction, size);
        }

        /// <summary>
        /// コマンド実行可能状態を更新
        /// </summary>
        private void UpdateMoveToHistoryCommandCanExecute()
        {
            this.MoveToPreviousCommand.RaiseCanExecuteChanged();
            this.MoveToNextCommand.RaiseCanExecuteChanged();
        }

        private void UpdateMoveToUpCommandCanExecute()
        { 
            this.MoveToUpCommand.RaiseCanExecuteChanged();
        }
    }
}
