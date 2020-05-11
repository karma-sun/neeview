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
    /// <summary>
    /// 履歴パネル
    /// Type: ControlModel? ViewModelParts?
    /// </summary>
    public class PageListPanel : BindableBase, IPanel
    {
        private PageListView _view;
        private PageListPresenter _presenter;

        public PageListPanel(PageList model)
        {
            _view = new PageListView(model);
            _presenter = new PageListPresenter(_view, model);

            Icon = App.Current.MainWindow.Resources["pic_photo_library_24px"] as ImageSource;
            IconMargin = new Thickness(9);

            Config.Current.PageList.AddPropertyChanged(nameof(PageListConfig.IsSelected), (s, e) => IsSelectedChanged?.Invoke(this, null));
        }

#pragma warning disable CS0067
        public event EventHandler IsVisibleLockChanged;
#pragma warning restore CS0067

        public event EventHandler IsSelectedChanged;


        public string TypeCode => nameof(PageListPanel);

        public ImageSource Icon { get; private set; }

        public Thickness IconMargin { get; private set; }

        public string IconTips => Properties.Resources.PageListName;

        public FrameworkElement View => _view;

        public bool IsSelected
        {
            get { return Config.Current.PageList.IsSelected; }
            set { if (Config.Current.PageList.IsSelected != value) Config.Current.PageList.IsSelected = value; }
        }

        public bool IsVisible
        {
            get => Config.Current.PageList.IsVisible;
            set => Config.Current.PageList.IsVisible = value;
        }

        public bool IsVisibleLock => false;

        public PanelPlace DefaultPlace { get; set; } = PanelPlace.Right;


        public void Refresh()
        {
            // nop.
        }

        public void Focus()
        {
            _presenter.FocusAtOnce();
        }
    }

}
