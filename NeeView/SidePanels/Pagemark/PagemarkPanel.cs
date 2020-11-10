using NeeLaboratory.ComponentModel;
using NeeView.Windows.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    public class PagemarkPanel : BindableBase, IPanel, IDisposable
    {
        private PagemarkListView _view;

        public PagemarkPanel(PagemarkList model)
        {
            _view = new PagemarkListView(model);

            Icon = App.Current.MainWindow.Resources["pic_bookmark_24px"] as ImageSource;

            Config.Current.Pagemark.AddPropertyChanged(nameof(PagemarkConfig.IsSelected), (s, e) => IsSelectedChanged?.Invoke(this, null));
        }

#pragma warning disable CS0067
        public event EventHandler IsVisibleLockChanged;
#pragma warning restore CS0067

        public event EventHandler IsSelectedChanged;


        public string TypeCode => nameof(PagemarkPanel);

        public ImageSource Icon { get; private set; }

        public string IconTips => Properties.Resources.PagemarkName;

        public FrameworkElement View => _view;

        public bool IsSelected
        {
            get { return Config.Current.Pagemark.IsSelected; }
            set { if (Config.Current.Pagemark.IsSelected != value) Config.Current.Pagemark.IsSelected = value; }
        }

        public bool IsVisible
        {
            get => Config.Current.Pagemark.IsVisible;
            set => Config.Current.Pagemark.IsVisible = value;
        }

        public bool IsVisibleLock => false;

        public PanelPlace DefaultPlace => PanelPlace.Right;


        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _view.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        public void Refresh()
        {
            _view.Refresh();
        }

        public void Focus()
        {
            _view.FocusAtOnce();
        }
    }
}
