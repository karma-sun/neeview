using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView
{
    public class PagemarkListViewModel : BindableBase, IDisposable
    {
        public PagemarkCollection Pagemarks => PagemarkCollection.Current;


        // Fields

        private PagemarkList _model;
        private CancellationTokenSource _removeUnlinkedCommandCancellationToken;
        private PagemarkListBox _listBoxContent;


        // Constructors

        public PagemarkListViewModel(PagemarkList model)
        {
            _model = model;
            _model.AddPropertyChanged(nameof(_model.PanelListItemStyle), (s, e) => UpdateListBoxContent());

            InitializeMoreMenu();

            UpdateListBoxContent();
        }


        // Properties

        public PagemarkList Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        public PagemarkListBox ListBoxContent
        {
            get { return _listBoxContent; }
            set
            {
                if (_listBoxContent != value)
                {
                    _listBoxContent?.Dispose();
                    _listBoxContent = value;
                    RaisePropertyChanged();
                }
            }
        }

        #region MoreMenu

        private PanelListItemStyleToBooleanConverter _PanelListItemStyleToBooleanConverter = new PanelListItemStyleToBooleanConverter();


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
            menu.Items.Add(CreateListItemStyleMenuItem(Properties.Resources.WordStyleList, PanelListItemStyle.Normal));
            menu.Items.Add(CreateListItemStyleMenuItem(Properties.Resources.WordStyleContent, PanelListItemStyle.Content));
            menu.Items.Add(CreateListItemStyleMenuItem(Properties.Resources.WordStyleBanner, PanelListItemStyle.Banner));
            menu.Items.Add(new Separator());
            menu.Items.Add(CreateCheckMenuItem(Properties.Resources.PagemarkMenuSortPath, new Binding(nameof(_model.IsSortPath)) { Source = _model }));
            menu.Items.Add(CreateCheckMenuItem(Properties.Resources.PagemarkMenuCurrentBook, new Binding(nameof(_model.IsCurrentBook)) { Source = _model }));
            menu.Items.Add(new Separator());
            menu.Items.Add(CreateCommandMenuItem(@Properties.Resources.PagemarkMenuOpenAsBook, OpenAsBookCommand));
            menu.Items.Add(new Separator());
            menu.Items.Add(CreateCommandMenuItem(Properties.Resources.PagemarkMenuDeleteInvalid, RemoveUnlinkedCommand));

            this.MoreMenu = menu;
        }

        private MenuItem CreateCheckMenuItem(string header, Binding binding)
        {
            var item = new MenuItem();
            item.Header = header;
            item.IsCheckable = true;
            item.SetBinding(MenuItem.IsCheckedProperty, binding);
            return item;
        }

        private MenuItem CreateCommandMenuItem(string header, ICommand command)
        {
            var item = new MenuItem();
            item.Header = header;
            item.Command = command;
            return item;
        }

        private MenuItem CreateCommandMenuItem(string header, CommandType command, object source)
        {
            var item = new MenuItem();
            item.Header = header;
            item.Command = RoutedCommandTable.Current.Commands[command];
            item.CommandParameter = MenuCommandTag.Tag; // コマンドがメニューからであることをパラメータで伝えてみる
            if (CommandTable.Current[command].CreateIsCheckedBinding != null)
            {
                var binding = CommandTable.Current[command].CreateIsCheckedBinding();
                binding.Source = source;
                item.SetBinding(MenuItem.IsCheckedProperty, binding);
            }

            return item;
        }

        private MenuItem CreateListItemStyleMenuItem(string header, PanelListItemStyle style)
        {
            var item = new MenuItem();
            item.Header = header;
            item.Command = SetListItemStyle;
            item.CommandParameter = style;
            var binding = new Binding(nameof(_model.PanelListItemStyle))
            {
                Converter = _PanelListItemStyleToBooleanConverter,
                ConverterParameter = style,
                Source = _model
            };
            item.SetBinding(MenuItem.IsCheckedProperty, binding);

            return item;
        }

        #endregion


        // Commands

        private RelayCommand<PanelListItemStyle> _SetListItemStyle;
        public RelayCommand<PanelListItemStyle> SetListItemStyle
        {
            get { return _SetListItemStyle = _SetListItemStyle ?? new RelayCommand<PanelListItemStyle>(SetListItemStyle_Executed); }
        }

        private void SetListItemStyle_Executed(PanelListItemStyle style)
        {
            _model.PanelListItemStyle = style;
        }


        private RelayCommand _removeUnlinkedCommand;
        public RelayCommand RemoveUnlinkedCommand
        {
            get { return _removeUnlinkedCommand = _removeUnlinkedCommand ?? new RelayCommand(RemoveUnlinkedCommand_Executed); }
        }

        private async void RemoveUnlinkedCommand_Executed()
        {
            // 直前の命令はキャンセル
            _removeUnlinkedCommandCancellationToken?.Cancel();
            _removeUnlinkedCommandCancellationToken = new CancellationTokenSource();
            var count = await PagemarkCollection.Current.RemoveUnlinkedAsync(_removeUnlinkedCommandCancellationToken.Token);
            ToastService.Current.Show("PagemarkList", new Toast(string.Format(Properties.Resources.NotifyRemoveUnlinkedPagemark, count)));
        }

        private RelayCommand _addPagemarkCommand;
        public RelayCommand AddPagemarkCommand
        {
            get { return _addPagemarkCommand = _addPagemarkCommand ?? new RelayCommand(AddPagemark_Executed); }
        }

        private void AddPagemark_Executed()
        {
            _model.AddPagemark();
        }

        private RelayCommand _openAsBookCommand;
        public RelayCommand OpenAsBookCommand
        {
            get { return _openAsBookCommand = _openAsBookCommand ?? new RelayCommand(OpenAdBookCommand_Executed); }
        }

        private void OpenAdBookCommand_Executed()
        {
            _model.OpenAsBook();
        }



        // Methods

        private void UpdateListBoxContent()
        {
            ListBoxContent = new PagemarkListBox(new PagemarkListBoxViewModel(Model.ListBox));
        }


        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _listBoxContent?.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
