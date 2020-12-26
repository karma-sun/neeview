using NeeLaboratory.ComponentModel;
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
    public class FileInformationPanel : BindableBase, IPanel
    {
        private FileInformationView _view;
        private FileInformation _model;

        public FileInformationPanel(FileInformation model)
        {
            _view = new FileInformationView(model);
            _model = model;

            Icon = App.Current.MainWindow.Resources["pic_info_24px"] as ImageSource;
        }

#pragma warning disable CS0067
        public event EventHandler IsVisibleLockChanged;
#pragma warning restore CS0067


        public string TypeCode => nameof(FileInformationPanel);

        public ImageSource Icon { get; private set; }

        public string IconTips => Properties.Resources.Information_Title;

        public FrameworkElement View => _view;

        public FileInformation FileInformation => _model;

        public bool IsVisibleLock => false;

        public PanelPlace DefaultPlace => PanelPlace.Right;


        public void Refresh()
        {
            // nop.
        }

        public void Focus()
        {
            _view.FocusAtOnce();
        }
    }
}
