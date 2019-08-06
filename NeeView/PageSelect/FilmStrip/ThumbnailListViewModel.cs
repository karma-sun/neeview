using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;

namespace NeeView
{
    /// <summary>
    /// ThumbnailList : ViewModel
    /// </summary>
    public class ThumbnailListViewModel : BindableBase
    {
        #region Fields

        private ThumbnailList _model;

        #endregion

        #region Constructors

        public ThumbnailListViewModel(ThumbnailList model)
        {
            if (model == null) return;
            _model = model;
        }

        #endregion

        #region Properties

        public ThumbnailList Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        #endregion

        #region Methods

        public void MoveSelectedIndex(int delta)
        {
            _model.MoveSelectedIndex(delta);
        }

        public void RequestThumbnail(int start, int count, int margin, int direction)
        {
            _model.RequestThumbnail(start, count, margin, direction);
        }

        public void CancelThumbnailRequest()
        {
            _model.CancelThumbnailRequest();
        }

        internal void FlushSelectedIndex()
        {
            _model.FlushSelectedIndex();
        }

        internal void ResetDelayHide()
        {
            _model.ResetDelayHide();
        }

        #endregion
    }
}
