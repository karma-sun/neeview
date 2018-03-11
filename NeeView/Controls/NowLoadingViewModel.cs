using NeeLaboratory.ComponentModel;

namespace NeeView
{
    /// <summary>
    /// NowLoading : ViewModel
    /// </summary>
    public class NowLoadingViewModel : BindableBase
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="model"></param>
        public NowLoadingViewModel(NowLoading model)
        {
            _model = model;
        }

        /// <summary>
        /// Model property.
        /// </summary>
        public NowLoading Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }
        private NowLoading _model;



    }

}
