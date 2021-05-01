using NeeLaboratory.ComponentModel;
using System.Collections.Generic;

namespace NeeView
{
    public class PageSortModePaletteViewModel : BindableBase
    {
        private PageSortModePaletteModel _model;


        public PageSortModePaletteViewModel()
        {
            _model = new PageSortModePaletteModel();
            _model.AddPropertyChanged(nameof(_model.PageSortModeList),
                (s, e) => RaisePropertyChanged(nameof(PageSortModeList)));
        }


        public List<PageSortMode> PageSortModeList => _model.PageSortModeList;


        public void Decide(PageSortMode mode)
        {
            BookSettingPresenter.Current.SetSortMode(mode);
        }
    }

}
