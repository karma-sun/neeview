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
    public class ImageEffectPanel : IPanel, INotifyPropertyChanged
    {
        /// <summary>
        /// PropertyChanged event. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        //
        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        public string TypeCode => nameof(ImageEffectPanel);

        public ImageSource Icon { get; private set; }

        public Thickness IconMargin { get; private set; }

        public string IconTips => "エフェクト";

        public FrameworkElement View { get; private set; }

        public bool IsVisibleLock => false;


        //
        public ImageEffectPanel(ImageEffect model)
        {
            View = new ImageEffectView(model);

            Icon = App.Current.MainWindow.Resources["pic_toy_24px"] as ImageSource;
            IconMargin = new Thickness(8);
        }
    }
}
