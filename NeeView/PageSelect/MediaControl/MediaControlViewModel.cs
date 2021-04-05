using NeeLaboratory.ComponentModel;
using System;
using System.Windows.Input;

namespace NeeView
{
    public class MediaControlViewModel : BindableBase
    {
        private MediaControl _model;
        private MediaPlayerOperator _operator;
        private MouseWheelDelta _mouseWheelDelta = new MouseWheelDelta();

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
            BookOperation.Current.Book?.Viewer.RaisePageTerminatedEvent(this, 1);
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

        public void MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int turn = _mouseWheelDelta.NotchCount(e);
            if (turn == 0) return;

            for (int i = 0; i < Math.Abs(turn); ++i)
            {
                if (turn < 0)
                {
                    BookOperation.Current.NextPage(this);
                }
                else
                {
                    BookOperation.Current.PrevPage(this);
                }
            }
        }

        internal void MouseWheelVolume(object sender, MouseWheelEventArgs e)
        {
            var delta = (double)e.Delta / 6000.0;
            Operator.AddVolume(delta);
        }

        internal bool KeyVolume(Key key)
        {
            switch (key)
            {
                case Key.Up:
                case Key.Right:
                    Operator.AddVolume(+0.01);
                    return true;

                case Key.Down:
                case Key.Left:
                    Operator.AddVolume(-0.01);
                    return true;

                default:
                    return false;
            }
        }

        #endregion
    }
}
