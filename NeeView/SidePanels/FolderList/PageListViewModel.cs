// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.Windows.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace NeeView
{
    /// <summary>
    /// 
    /// </summary>
    public class PageListViewModel : INotifyPropertyChanged
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

        public event EventHandler PagesChanged;

#if false
        #region Property: VM
        private MainWindowVM _VM;
        public MainWindowVM VM
        {
            get { return _VM; }
            set
            {
                _VM = value;
                _VM.PageListChanged += (s, e) => Reflesh();
                RaisePropertyChanged();
            }
        }
        #endregion
#endif


        private BookHub _bookHub;
        public BookHub BookHub
        {
            get { return _bookHub; }
            set
            {
                _bookHub = value;
                _bookHub.ViewContentsChanged += BookHub_ViewContentsChanged;
                RaisePropertyChanged();
            }
        }

        private void BookHub_ViewContentsChanged(object sender, ViewSource e)
        {
            var contents = e?.Sources;
            if (contents == null) return;

            var mainContent = contents.Count > 0 ? (contents.First().Position < contents.Last().Position ? contents.First() : contents.Last()) : null;
            if (mainContent != null)
            {
                SelectedItem = mainContent.Page;
            }
        }

        public Dictionary<PageNameFormat, string> FormatList { get; } = new Dictionary<PageNameFormat, string>
        {
            [PageNameFormat.None] = "そのまま",
            [PageNameFormat.Smart] = "標準表示",
            [PageNameFormat.NameOnly] = "名前のみ",
        };

        #region Property: Format
        private PageNameFormat _format = PageNameFormat.Smart;
        public PageNameFormat Format
        {
            get { return _format; }
            set { _format = value; RaisePropertyChanged(); }
        }
        #endregion


        public Dictionary<PageSortMode, string> PageSortModeList => PageSortModeExtension.PageSortModeList;

        #region Property: Title
        private string _title;
        public string Title
        {
            get { return _title; }
            set { _title = value; RaisePropertyChanged(); }
        }
        #endregion

        #region Property: PageSortMode
        private PageSortMode _pageSortMode;
        public PageSortMode PageSortMode
        {
            get { return _pageSortMode; }
            set { _pageSortMode = value; _bookHub.SetSortMode(value); }
        }
        #endregion

        #region Property: SelectedItem
        private Page _selectedItem;
        public Page SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; RaisePropertyChanged(); }
        }
        #endregion



        #region MoreMenu

        /// <summary>
        /// MoreMenu property.
        /// </summary>
        public ContextMenu MoreMenu
        {
            get { return _MoreMenu; }
            set { if (_MoreMenu != value) { _MoreMenu = value; RaisePropertyChanged(); } }
        }

        //
        private ContextMenu _MoreMenu;


        //
        private void InitializeMoreMenu()
        {
            var menu = new ContextMenu();
            menu.Items.Add(CreateListItemStyleMenuItem("一覧表示", PanelListItemStyle.Normal));
            menu.Items.Add(CreateListItemStyleMenuItem("コンテンツ表示", PanelListItemStyle.Content));

            this.MoreMenu = menu;
        }

        //
        private MenuItem CreateListItemStyleMenuItem(string header, PanelListItemStyle style)
        {
            var item = new MenuItem();
            item.Header = header;
            item.Command = SetListItemStyle;
            item.CommandParameter = style;
            var binding = new Binding(nameof(PanelListItemStyle))
            {
                Converter = _PanelListItemStyleToBooleanConverter,
                ConverterParameter = style
            };
            item.SetBinding(MenuItem.IsCheckedProperty, binding);

            return item;
        }


        private PanelListItemStyleToBooleanConverter _PanelListItemStyleToBooleanConverter = new PanelListItemStyleToBooleanConverter();


        /// <summary>
        /// SetListItemStyle command.
        /// </summary>
        public RelayCommand<PanelListItemStyle> SetListItemStyle
        {
            get { return _SetListItemStyle = _SetListItemStyle ?? new RelayCommand<PanelListItemStyle>(SetListItemStyle_Executed); }
        }

        //
        private RelayCommand<PanelListItemStyle> _SetListItemStyle;

        //
        private void SetListItemStyle_Executed(PanelListItemStyle style)
        {
            _model.PanelListItemStyle = style;
        }

        /*
        /// <summary>
        /// PanelListItemStyle property.
        /// TODO: 保存されるものなのでモデル的なクラスでの実装が望ましい
        /// </summary>
        public PanelListItemStyle PanelListItemStyle
        {
            get { return _PanelListItemStyle; }
            set
            {
                if (_PanelListItemStyle != value)
                {
                    _PanelListItemStyle = value;
                    ////this.FolderListView?.SetPanelListItemStyle(_PanelListItemStyle);
                    RaisePropertyChanged();
                }
            }
        }

        //
        private PanelListItemStyle _PanelListItemStyle;
        */


        #endregion

        /// <summary>
        /// Model property.
        /// </summary>
        public PageList Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        private PageList _model;


        public PageListViewModel(PageList model)
        {
            _model = model;
            this.BookHub = _model.BookHub;

            InitializeMoreMenu();

            Reflesh();
        }

        /*
        //
        public void Initialize(MainWindowVM vm)
        {
            VM = vm;
            BookHub = vm.BookHub;

            InitializeMoreMenu();

            Reflesh();
        }
        */


        //
        private void Reflesh()
        {
            Title = System.IO.Path.GetFileName(_bookHub.CurrentBook?.Place);

            _pageSortMode = _bookHub.BookMemento.SortMode;
            RaisePropertyChanged(nameof(PageSortMode));

            App.Current?.Dispatcher.Invoke(() => PagesChanged?.Invoke(this, null));
        }


        //
        public void Jump(Page page)
        {
            _bookHub.JumpPage(page);
        }


        //
        public bool CanRemove(Page page)
        {
            return _bookHub.CanRemoveFile(page);
        }

        //
        public async Task Remove(Page page)
        {
            await _bookHub.RemoveFile(page);
        }

        // サムネイル要求
        public void RequestThumbnail(int start, int count, int margin, int direction)
        {
            //if (VM == null) return;
            if (_model.PanelListItemStyle.HasThumbnail())
            {
                ThumbnailManager.Current.RequestThumbnail(_model.PageCollection, QueueElementPriority.PageListThumbnail, start, count, margin, direction);
            }
        }

#if false
        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember]
            public PanelListItemStyle PanelListItemStyle { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.PanelListItemStyle = this.PanelListItemStyle;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.PanelListItemStyle = memento.PanelListItemStyle;
        }

        #endregion
#endif
    }
}
