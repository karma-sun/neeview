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

        public PageListPanel(PageList model)
        {
            _view = new PageListView(model);

            Icon = App.Current.MainWindow.Resources["pic_photo_library_24px"] as ImageSource;
            IconMargin = new Thickness(9);
        }

#pragma warning disable CS0067
        public event EventHandler IsVisibleLockChanged;
#pragma warning restore CS0067

        public string TypeCode => nameof(PageListPanel);

        public ImageSource Icon { get; private set; }

        public Thickness IconMargin { get; private set; }

        public string IconTips => Properties.Resources.PageListName;

        public FrameworkElement View => _view;

        public bool IsVisibleLock => false;

        public PanelPlace DefaultPlace { get; set; } = PanelPlace.Right;


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
