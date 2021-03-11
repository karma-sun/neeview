using NeeLaboratory.ComponentModel;
using System.Windows;

namespace NeeView
{
    public class WindowEnvironment : BindableBase
    {
        static WindowEnvironment() => Current = new WindowEnvironment();
        public static WindowEnvironment Current { get; }


        private bool _isHighContrast = SystemParameters.HighContrast;


        private WindowEnvironment()
        {
            SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
        }

        public bool IsHighContrast
        {
            get { return _isHighContrast; }
            set { SetProperty(ref _isHighContrast, value); }
        }


        private void SystemParameters_StaticPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SystemParameters.HighContrast):
                    IsHighContrast = SystemParameters.HighContrast;
                    break;
            }
        }
    }
}
