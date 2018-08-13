using NeeLaboratory.ComponentModel;

namespace NeeView
{
    public class FolderPanelViewModel : BindableBase
    {
        private FolderPanelModel _model;

        public FolderPanelViewModel(FolderPanelModel model)
        {
            _model = model;
        }

        public FolderPanelModel Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }
    }
}
