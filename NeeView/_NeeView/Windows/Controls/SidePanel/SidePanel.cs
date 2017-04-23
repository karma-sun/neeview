using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;

namespace NeeView.Windows.Controls
{
    /// <summary>
    /// SidePanel 
    /// </summary>
    public class SidePanel : INotifyPropertyChanged
    {
        /// <summary>
        /// PropertyChanged event. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// Panels property.
        /// </summary>
        private ObservableCollection<IPanel> _panels;
        public ObservableCollection<IPanel> Panels
        {
            get { return _panels; }
            set { if (_panels != value) { _panels = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// SelectedPanel property.
        /// </summary>
        private IPanel _selectedPanel;
        public IPanel SelectedPanel
        {
            get { return _selectedPanel; }
            set { if (_selectedPanel != value) { _selectedPanel = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// Width property.
        /// </summary>
        private double _width = 250.0;
        public double Width
        {
            get { return _width; }
            set { if (_width != value) { _width = value; RaisePropertyChanged(); } }
        }



        /// <summary>
        /// constructor
        /// </summary>
        public SidePanel()
        {
            _panels = new ObservableCollection<IPanel>();
        }


        /// <summary>
        /// Select
        /// </summary>
        /// <param name="content"></param>
        public void Select(IPanel content)
        {
            if (_panels.Contains(content))
            {
                SelectedPanel = SelectedPanel != content ? content : null;
            }
        }

        public void Remove(IPanel panel)
        {
            Panels.Remove(panel);
            if (SelectedPanel == panel) SelectedPanel = null;
        }

        public void Add(IPanel panel, int index)
        {
            if (Panels.Contains(panel))
            {
                var current = Panels.IndexOf(panel);
                Panels.Move(current, index);
            }
            else
            {
                Panels.Insert(index, panel);
            }
        }

        //
        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public List<string> PanelTypeCodes { get; set; }

            [DataMember]
            public string SelectedPanelTypeCode { get; set; }

            [DataMember]
            public double Width { get; set; }

            /// <summary>
            /// constructor
            /// </summary>
            private void Constructor()
            {
                PanelTypeCodes = new List<string>();
                Width = 250.0;
            }

            /// <summary>
            /// constructor
            /// </summary>
            public Memento()
            {
                Constructor();
            }

            /// <summary>
            /// デシリアイズ前処理
            /// </summary>
            /// <param name="c"></param>
            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                Constructor();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.PanelTypeCodes = Panels.Select(e => e.TypeCode).ToList();
            memento.SelectedPanelTypeCode = SelectedPanel?.TypeCode;
            memento.Width = Width;

            return memento;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memento"></param>
        /// <param name="panels"></param>
        public void Restore(Memento memento, List<IPanel> panels)
        {
            if (memento == null) return;

            Panels = new ObservableCollection<IPanel>(memento.PanelTypeCodes.Select(e => panels.FirstOrDefault(panel => panel.TypeCode == e)).Where(e => e != null));
            SelectedPanel = Panels.FirstOrDefault(e => e.TypeCode == memento.SelectedPanelTypeCode);
            Width = memento.Width;
        }

        #endregion

    }
}
