using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;
using System.Collections.ObjectModel;
using NeeLaboratory.Windows.Input;
using System.Runtime.Serialization;
using System.Windows.Input;
using System.Linq;
using System.Diagnostics;
using NeeLaboratory.ComponentModel;
using System.Threading;
using NeeView.Properties;
using System.Threading.Tasks;

namespace NeeView
{
    public class PlaylistViewModel : BindableBase
    {
        private PlaylistModel _model;


        public PlaylistViewModel(PlaylistModel model)
        {
            _model = model;

            MoreMenuDescription = new PlaylistMoreMenuDescription(this);
        }


        public PlaylistModel Model => _model;


        #region MoreMenu

        public PlaylistMoreMenuDescription MoreMenuDescription { get; }

        public class PlaylistMoreMenuDescription : ItemsListMoreMenuDescription
        {
            private PlaylistViewModel _vm;

            public PlaylistMoreMenuDescription(PlaylistViewModel vm)
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
                menu.Items.Add(CreateCommandMenuItem("@New", _vm.NewCommand));
                menu.Items.Add(CreateCommandMenuItem("@Open", _vm.OpenCommand));
                menu.Items.Add(CreateCommandMenuItem("@Delete", _vm.DeleteCommand));
                menu.Items.Add(CreateCommandMenuItem("@Rename", _vm.RenameCommand));
                menu.Items.Add(new Separator());
                menu.Items.Add(new MenuItem() { Header = "@Group" });
                menu.Items.Add(new MenuItem() { Header = "@CurrentOnly" });
                menu.Items.Add(new MenuItem() { Header = "@VisibleMarker" });
                menu.Items.Add(new Separator());
                menu.Items.Add(new MenuItem() { Header = "@RemoveIgnoreItems" });
                menu.Items.Add(new MenuItem() { Header = "@Sort" });
                menu.Items.Add(new Separator());
                menu.Items.Add(new MenuItem() { Header = "@OpenAsBook" });

                return menu;
            }

            private MenuItem CreateListItemStyleMenuItem(string header, PanelListItemStyle style)
            {
                return CreateListItemStyleMenuItem(header, _vm.SetListItemStyle, style, Config.Current.Playlist);
            }
        }

        #endregion

        #region Commands

        private RelayCommand<PanelListItemStyle> _setListItemStyle;

        public RelayCommand<PanelListItemStyle> SetListItemStyle
        {
            get { return _setListItemStyle = _setListItemStyle ?? new RelayCommand<PanelListItemStyle>(SetListItemStyle_Executed); }
        }

        private void SetListItemStyle_Executed(PanelListItemStyle style)
        {
            Config.Current.Playlist.PanelListItemStyle = style;
        }


        private RelayCommand _sddPagelistItemCommand;
        public RelayCommand AddPagelistItemCommand
        {
            get { return _sddPagelistItemCommand = _sddPagelistItemCommand ?? new RelayCommand(AddPagelistItemCommand_Execute); }
        }

        private void AddPagelistItemCommand_Execute()
        {
            // TODO: 本来はコマンドを呼ぶべき？
            _model.AddPlaylist();
        }


        private RelayCommand _NewCommand;
        public RelayCommand NewCommand
        {
            get { return _NewCommand = _NewCommand ?? new RelayCommand(NewCommand_Execute); }
        }

        private void NewCommand_Execute()
        {
            throw new NotImplementedException();
        }


        private RelayCommand _OpenCommand;
        public RelayCommand OpenCommand
        {
            get { return _OpenCommand = _OpenCommand ?? new RelayCommand(OpenCommand_Execute); }
        }

        private void OpenCommand_Execute()
        {
            throw new NotImplementedException();
        }

        private RelayCommand _DeleteCommand;
        public RelayCommand DeleteCommand
        {
            get { return _DeleteCommand = _DeleteCommand ?? new RelayCommand(DeleteCommand_Execute); }
        }

        private void DeleteCommand_Execute()
        {
            throw new NotImplementedException();
        }


        private RelayCommand _RenameCommand;
        public RelayCommand RenameCommand
        {
            get { return _RenameCommand = _RenameCommand ?? new RelayCommand(RenameCommand_Execute); }
        }

        private void RenameCommand_Execute()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
