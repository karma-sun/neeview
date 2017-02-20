// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// FolderList ViewModel
    /// </summary>
    public class FolderListViewModel : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
        #endregion

        public FolderCollection FolderCollection { get; set; }

        public FolderListItemStyle FolderListItemStyle => PanelContext.FolderListItemStyle;

        public double PicturePanelHeight => ThumbnailHeight + 24.0;

        public double ThumbnailWidth => Math.Floor(PanelContext.ThumbnailManager.ThumbnailSizeX / App.Config.DpiScaleFactor.X);
        public double ThumbnailHeight => Math.Floor(PanelContext.ThumbnailManager.ThumbnailSizeY / App.Config.DpiScaleFactor.Y);


        /// <summary>
        /// SelectIndex property.
        /// </summary>
        private int _selectedIndex;
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;
                RaisePropertyChanged();
            }
        }


        //
        public FolderListViewModel()
        {
            RaisePropertyChanged(nameof(FolderListItemStyle));
            PanelContext.FolderListStyleChanged += (s, e) => RaisePropertyChanged(nameof(FolderListItemStyle));
        }


        //
        /// <summary>
        /// RemoveCommand command.
        /// </summary>
        private RelayCommand<object> _removeCommand;
        public RelayCommand<object> RemoveCommand
        {
            get { return _removeCommand = _removeCommand ?? new RelayCommand<object>(RemoveCommand_Executed); }
        }

        private void RemoveCommand_Executed(object parameter)
        {
            var item = parameter as FolderInfo;
            if (item != null)
            {
                Remove(item);
            }
        }


        public void Remove(FolderInfo info)
        {
            if (info.IsEmpty) return;

            var stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;
            var thumbnail = new Image();
            thumbnail.SnapsToDevicePixels = true;
            thumbnail.Source = info.Icon;
            thumbnail.Width = 32;
            thumbnail.Height = 32;
            thumbnail.Margin = new System.Windows.Thickness(0, 0, 4, 0);
            stackPanel.Children.Add(thumbnail);
            var textblock = new TextBlock();
            textblock.Text = info.Path;
            textblock.VerticalAlignment = VerticalAlignment.Center;
            stackPanel.Children.Add(textblock);
            stackPanel.Margin = new Thickness(0, 0, 0, 20);

            Messenger.Send(this, new MessageEventArgs("RemoveFile") { Parameter = new RemoveFileParams() { Path = info.Path, Visual = stackPanel } });
        }


        /// <summary>
        /// OpenPlaceCommand command.
        /// </summary>
        private RelayCommand<object> _openPlaceCommand;
        public RelayCommand<object> OpenPlaceCommand
        {
            get { return _openPlaceCommand = _openPlaceCommand ?? new RelayCommand<object>(OpenPlaceCommand_Executed); }
        }

        private void OpenPlaceCommand_Executed(object parameter)
        {
            var item = parameter as FolderInfo;
            if (item != null)
            {
                System.Diagnostics.Process.Start("explorer.exe", "/select,\"" + item.Path + "\"");
            }
        }






        // サムネイル要求
        public void RequestThumbnail(int start, int count, int margin, int direction)
        {
            //Debug.WriteLine($"{start}({count})");
            PanelContext.ThumbnailManager.RequestThumbnail(FolderCollection.Items, start, count, margin, direction);
        }
    }
}
