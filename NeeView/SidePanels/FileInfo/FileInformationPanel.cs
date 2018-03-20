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
        public string TypeCode => nameof(FileInformationPanel);

        public ImageSource Icon { get; private set; }

        public Thickness IconMargin { get; private set; }

        public string IconTips => Properties.Resources.FileInfoName;

        public FrameworkElement View { get; private set; }

        public bool IsVisibleLock => false;

        public PanelPlace DefaultPlace => PanelPlace.Right;

        //
        public FileInformationPanel(FileInformation model)
        {
            View = new FileInformationView(model);

            Icon = App.Current.MainWindow.Resources["pic_info_24px"] as ImageSource;
            IconMargin = new Thickness(9);
        }
    }
}
