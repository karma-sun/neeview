using NeeLaboratory.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace NeeView.Configure
{
    /// <summary>
    /// 設定画面 ViewModel
    /// </summary>
    public class SettingWindowViewModel : BindableBase
    {
        private SettingWindowModel _model;

        public SettingWindowViewModel(SettingWindowModel model)
        {
            _model = model;
        }
        
        public SettingWindowModel Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }
    }


}
