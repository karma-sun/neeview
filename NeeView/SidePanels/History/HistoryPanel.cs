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
        public string TypeCode => nameof(HistoryPanel);

        public ImageSource Icon { get; private set; }

        public Thickness IconMargin { get; private set; }

        public string IconTips => Properties.Resources.HistoryName;

        private HistoryListView _view;
        public FrameworkElement View => _view;

        public bool IsVisibleLock => false;

        public PanelPlace DefaultPlace => PanelPlace.Left;


        //
        public HistoryPanel(HistoryList model)
        {
            _view = new HistoryListView(model);

            Icon = App.Current.MainWindow.Resources["pic_history_24px"] as ImageSource;
            IconMargin = new Thickness(7, 8, 9, 8);
            //IconMargin = new Thickness(8);
        }
    }

}
