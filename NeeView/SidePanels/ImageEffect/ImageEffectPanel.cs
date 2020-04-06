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
            IconMargin = new Thickness(8);

            Config.Current.Effect.AddPropertyChanged(nameof(EffectConfig.IsSelected), (s, e) => IsSelectedChanged?.Invoke(this, null));
        }

#pragma warning disable CS0067
        public event EventHandler IsVisibleLockChanged;
#pragma warning restore CS0067

        public event EventHandler IsSelectedChanged;


        public string TypeCode => nameof(ImageEffectPanel);

        public ImageSource Icon { get; private set; }

        public Thickness IconMargin { get; private set; }

        public string IconTips => Properties.Resources.EffectName;

        public FrameworkElement View => _view;

        public bool IsSelected
        {
            get { return Config.Current.Effect.IsSelected; }
            set { if (Config.Current.Effect.IsSelected != value) Config.Current.Effect.IsSelected = value; }
        }

        public bool IsVisible
        {
            get => Config.Current.Effect.IsVisible;
            set => Config.Current.Effect.IsVisible = value;
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
