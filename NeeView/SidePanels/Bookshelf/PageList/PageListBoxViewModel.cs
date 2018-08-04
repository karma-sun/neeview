using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace NeeView
{
    public class PageListBoxViewModel : BindableBase
    {
        private PageListBoxModel _model;


        public PageListBoxViewModel(PageListBoxModel model)
        {
            _model = model;
        }


        public event EventHandler<ViewItemsChangedEventArgs> ViewItemsChanged;


        public PageListBoxModel Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }


        public void Loaded()
        {
            _model.Loaded();
            _model.ViewItemsChanged += (s, e) => ViewItemsChanged?.Invoke(s, e);
        }

        public void Unloaded()
        {
            _model.Unloaded();
            _model.ViewItemsChanged -= (s, e) => ViewItemsChanged?.Invoke(s, e);
        }

    }
}
