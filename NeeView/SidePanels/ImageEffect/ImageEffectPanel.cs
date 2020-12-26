using NeeLaboratory.ComponentModel;
using NeeView.Effects;
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
    /// <summary>
    /// ImageEffect : Panel
    /// </summary>
    public class ImageEffectPanel : BindableBase, IPanel
    {
        private ImageEffectView _view;
        
        public ImageEffectPanel(ImageEffect model, ImageFilter imageFilter)
        {
            _view = new ImageEffectView(model, imageFilter);

            Icon = App.Current.MainWindow.Resources["pic_toy_24px"] as ImageSource;
        }

#pragma warning disable CS0067
        public event EventHandler IsVisibleLockChanged;
#pragma warning restore CS0067


        public string TypeCode => nameof(ImageEffectPanel);

        public ImageSource Icon { get; private set; }

        public string IconTips => Properties.Resources.Effect_Title;

        public FrameworkElement View => _view;

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
