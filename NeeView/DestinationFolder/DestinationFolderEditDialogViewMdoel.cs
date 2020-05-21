using NeeLaboratory.ComponentModel;

namespace NeeView
{
    public class DestinationFolderEditDialogViewMdoel : BindableBase
    {
        private DestinationFolder _model;


        public DestinationFolderEditDialogViewMdoel(DestinationFolder model)
        {
            _model = model;
        }


        public string Name
        {
            get => _model.Name;
            set => _model.Name = value;
        }

        public string Path
        {
            get => _model.Path;
            set
            {
                _model.Path = value;
                RaisePropertyChanged(nameof(Name));
            }
        }
    }
}
