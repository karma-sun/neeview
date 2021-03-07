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

            Config.Current.Pagemark.AddPropertyChanged(nameof(PagemarkConfig.PanelListItemStyle), (s, e) => UpdateListBoxContent());

            MoreMenuDescription = new PagemarkListMoreMenuDescription(this);

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

        public PagemarkListMoreMenuDescription MoreMenuDescription { get; }

        public class PagemarkListMoreMenuDescription : ItemsListMoreMenuDescription
        {
            private PagemarkListViewModel _vm;

            public PagemarkListMoreMenuDescription(PagemarkListViewModel vm)
            {
                _vm = vm;
            }

            public override ContextMenu Create()
            {
                var menu = new ContextMenu();
                menu.Items.Add(CreateListItemStyleMenuItem(Properties.Resources.Word_StyleList, PanelListItemStyle.Normal));
                menu.Items.Add(CreateListItemStyleMenuItem(Properties.Resources.Word_StyleContent, PanelListItemStyle.Content));
                menu.Items.Add(CreateListItemStyleMenuItem(Properties.Resources.Word_StyleBanner, PanelListItemStyle.Banner));
                menu.Items.Add(new Separator());
                menu.Items.Add(CreateCheckMenuItem(Properties.Resources.Pagemark_MoreMenu_SortPath, new Binding(nameof(_model.IsSortPath)) { Source = _vm.Model }));
                menu.Items.Add(CreateCheckMenuItem(Properties.Resources.Pagemark_MoreMenu_CurrentBook, new Binding(nameof(_model.IsCurrentBook)) { Source = _vm.Model }));
                menu.Items.Add(new Separator());
                menu.Items.Add(CreateCommandMenuItem(@Properties.Resources.Pagemark_MoreMenu_OpenAsBook, _vm.OpenAsBookCommand));
                menu.Items.Add(new Separator());
                menu.Items.Add(CreateCommandMenuItem(Properties.Resources.Pagemark_MoreMenu_DeleteInvalid, _vm.RemoveUnlinkedCommand));
                return menu;
            }

            private MenuItem CreateListItemStyleMenuItem(string header, PanelListItemStyle style)
            {
                return CreateListItemStyleMenuItem(header, _vm.SetListItemStyle, style, Config.Current.Pagemark);
            }
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
            Config.Current.Pagemark.PanelListItemStyle = style;
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
            ToastService.Current.Show("PagemarkList", new Toast(string.Format(Properties.Resources.Notice_RemoveUnlinkedPagemark, count)));
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
