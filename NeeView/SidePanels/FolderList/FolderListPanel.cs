// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.Windows.Controls;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    public class FolderListPanel : IPanel, INotifyPropertyChanged
    {
        /// <summary>
        /// PropertyChanged event. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        //
        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string TypeCode => nameof(FolderListPanel);

        public ImageSource Icon { get; private set; }

        public Thickness IconMargin { get; private set; }

        public string IconTips => "フォルダーリスト";

        private FolderListPanelView _view;
        public FrameworkElement View => _view;

        public bool IsVisibleLock => this.FolderListControl.IsRenaming;

        //
        public FolderListPanel()
        {
            _view = new FolderListPanelView();

            Icon = App.Current.MainWindow.Resources["pic_folder_24px"] as DrawingImage;
            IconMargin = new Thickness(8);
        }

        // TODO: 構築順、どうなの？
        public void Initialize(MainWindowVM vm)
        {
            _view.Initialize(vm);
        }

        // TODO: ここでの呼び出しはおかしい
        public void SetPlace(string place, string select, bool isFocus)
        {
            _view.FolderList.SetPlace(place, select, isFocus);
        }

        // TODO: おかしい
        public FolderListControlView FolderListControl => _view.FolderList;

        // TODO: おかしい
        public PageListControl PageListControl => _view.PageList;

        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public FolderListControlViewModel.Memento FolderListMemento { get; set; }

            [DataMember]
            public PageListControlViewModel.Memento PageListMemento { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.FolderListMemento = FolderListControl.VM.CreateMemento();
            memento.PageListMemento = PageListControl.VM.CreateMemento();
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            FolderListControl.VM.Restore(memento.FolderListMemento);
            PageListControl.VM.Restore(memento.PageListMemento);
        }

        #endregion
    }

}
