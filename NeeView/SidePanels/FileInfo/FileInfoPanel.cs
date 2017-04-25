using NeeView.Windows.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    public class FileInfoPanel : IPanel, INotifyPropertyChanged
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


        public string TypeCode => nameof(FileInfoPanel);

        public ImageSource Icon { get; private set; }

        public Thickness IconMargin { get; private set; }

        public string IconTips => "ファイル情報";

        private FileInfoPanelView _view;
        public FrameworkElement View => _view;

        public bool IsVisibleLock => false;


        //
        public FileInfoPanel()
        {
            _view = new FileInfoPanelView();

            Icon = App.Current.MainWindow.Resources["pic_info_24px"] as ImageSource;
            IconMargin = new Thickness(9);
        }

        //
        public void Initialize(MainWindowVM vm)
        {
            _view.Initialize(vm);
        }


        /// <summary>
        /// ViewContent property.
        /// </summary>
        public ViewContent ViewContent
        {
            get { return _view.FileInfoControl.ViewContent; }
            set { _view.FileInfoControl.ViewContent = value; }
        }

        /// <summary>
        /// Setting property.
        /// </summary>
        public FileInfoSetting Setting
        {
            get { return _view.FileInfoControl.Setting; }
            set { _view.FileInfoControl.Setting = value; }
        }
    }
}
