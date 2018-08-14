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
        public string TypeCode => nameof(PageListPanel);

        public ImageSource Icon { get; private set; }

        public Thickness IconMargin { get; private set; }

        public string IconTips => Properties.Resources.PageListName;

        private PageListView _view;
        public FrameworkElement View => _view;

        public bool IsVisibleLock => false;

        public PanelPlace DefaultPlace => PanelPlace.Right;


        //
        public PageListPanel(PageList model)
        {
            _view = new PageListView(model);

            Icon = App.Current.MainWindow.Resources["pic_photo_library_24px"] as ImageSource;
            IconMargin = new Thickness(9);
        }
    }

}
