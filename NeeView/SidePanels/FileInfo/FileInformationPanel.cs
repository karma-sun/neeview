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

        public FileInformationPanel(FileInformation model)
        {
            _view = new FileInformationView(model);

            Icon = App.Current.MainWindow.Resources["pic_info_24px"] as ImageSource;
            IconMargin = new Thickness(9);

            Config.Current.Information.AddPropertyChanged(nameof(InformationConfig.IsSelected), (s, e) => IsSelectedChanged?.Invoke(this, null));
        }

#pragma warning disable CS0067
        public event EventHandler IsVisibleLockChanged;
#pragma warning restore CS0067

        public event EventHandler IsSelectedChanged;


        public string TypeCode => nameof(FileInformationPanel);

        public ImageSource Icon { get; private set; }

        public Thickness IconMargin { get; private set; }

        public string IconTips => Properties.Resources.FileInfoName;

        public FrameworkElement View => _view;

        public bool IsSelected
        {
            get { return Config.Current.Information.IsSelected; }
            set { if (Config.Current.Information.IsSelected != value) Config.Current.Information.IsSelected = value; }
        }

        public bool IsVisible
        {
            get => Config.Current.Information.IsVisible;
            set => Config.Current.Information.IsVisible = value;
        }

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
