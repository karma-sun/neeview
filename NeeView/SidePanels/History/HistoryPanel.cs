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
    public class HistoryPanel : BindableBase, IPanel
    {
        private HistoryListView _view;
        private HistoryListPresenter _presenter;

        public HistoryPanel(HistoryList model)
        {
            _view = new HistoryListView(model);
            _presenter = new HistoryListPresenter(_view, model);

            Icon = App.Current.MainWindow.Resources["pic_history_24px"] as ImageSource;
            IconMargin = new Thickness(7, 8, 9, 8);
            //IconMargin = new Thickness(8);

            Config.Current.History.AddPropertyChanged(nameof(HistoryConfig.IsSelected), (s, e) => IsSelectedChanged?.Invoke(this, null));
        }

#pragma warning disable CS0067
        public event EventHandler IsVisibleLockChanged;
#pragma warning restore CS0067

        public event EventHandler IsSelectedChanged;


        public string TypeCode => nameof(HistoryPanel);

        public ImageSource Icon { get; private set; }

        public Thickness IconMargin { get; private set; }

        public string IconTips => Properties.Resources.HistoryName;

        public FrameworkElement View => _view;

        public bool IsSelected
        {
            get { return Config.Current.History.IsSelected; }
            set { if (Config.Current.History.IsSelected != value) Config.Current.History.IsSelected = value; }
        }

        public bool IsVisible
        {
            get => Config.Current.History.IsVisible;
            set => Config.Current.History.IsVisible = value;
        }

        public bool IsVisibleLock => false;

        public PanelPlace DefaultPlace => PanelPlace.Left;


        public void Refresh()
        {
            _presenter.Refresh();
        }

        public void Focus()
        {
            _presenter.FocusAtOnce();
        }
    }

}
