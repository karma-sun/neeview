using NeeLaboratory.ComponentModel;

namespace NeeView
{
    /// <summary>
    /// NowLoading : Model
    /// </summary>
    public class NowLoading : BindableBase
    {
        public static NowLoading Current { get; private set; }

        /// <summary>
        /// constructor
        /// </summary>
        public NowLoading()
        {
            Current = this;

            BookHub.Current.Loading +=
                (s, e) => IsDispNowLoading = e != null;
        }

        /// <summary>
        /// IsDispNowLoading property.
        /// </summary>
        public bool IsDispNowLoading
        {
            get { return _IsDispNowLoading; }
            set { if (_IsDispNowLoading != value) { _IsDispNowLoading = value; RaisePropertyChanged(); } }
        }

        private bool _IsDispNowLoading;

        //
        public void SetLoading(string message)
        {
            IsDispNowLoading = true;
            WindowTitle.Current.LoadingPath = message;
        }

        //
        public void ResetLoading()
        {
            IsDispNowLoading = false;
            WindowTitle.Current.LoadingPath = null;
        }
    }

}
