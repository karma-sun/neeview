using NeeLaboratory.ComponentModel;

namespace NeeView
{
    public class MediaControlViewModel : BindableBase
    {
        private MediaControl _model;
        private MediaPlayerOperator _operator;

        public MediaControlViewModel(MediaControl model)
        {
            _model = model;
            _model.Changed += Model_Changed;
        }

        public MediaPlayerOperator Operator
        {
            get { return _operator; }
            set { if (_operator != value) { _operator = value; RaisePropertyChanged(); } }
        }

        #region Methods

        private void Model_Changed(object sender, MediaPlayerChanged e)
        {
            Operator?.Dispose();

            if (e.IsValid)
            {
                Operator = new MediaPlayerOperator(e.MediaPlayer);
                Operator.MediaEnded += Operator_MediaEnded;
                Operator.Open(e.Uri, e.IsLastStart);
            }
            else
            {
                Operator = null;
            }

            MediaPlayerOperator.Current = Operator;
        }

        private void Operator_MediaEnded(object sender, System.EventArgs e)
        {
            BookOperation.Current.Book?.RaisePageTerminatedEvent(1);
        }

        public void SetScrubbing(bool isScrubbing)
        {
            if (_operator == null)
            {
                return;
            }

            _operator.IsScrubbing = isScrubbing;
        }

        public void ToggleTimeFormat()
        {
            if (_operator == null)
            {
                return;
            }

            _operator.IsTimeLeftDisp = !_operator.IsTimeLeftDisp;
        }

        #endregion
    }
}
