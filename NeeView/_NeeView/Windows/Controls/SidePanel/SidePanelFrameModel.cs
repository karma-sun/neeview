using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace NeeView.Windows.Controls
{
    /// <summary>
    /// SidePanelFrame Model
    /// </summary>
    public class SidePanelFrameModel : INotifyPropertyChanged
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
        /// Left property.
        /// </summary>
        public SidePanel Left
        {
            get { return _left; }
            set { if (_left != value) { _left = value; RaisePropertyChanged(); } }
        }

        //
        private SidePanel _left;


        /// <summary>
        /// Right property.
        /// </summary>
        public SidePanel Right
        {
            get { return _right; }
            set { if (_right != value) { _right = value; RaisePropertyChanged(); } }
        }

        //
        private SidePanel _right;



        /// <summary>
        /// 
        /// </summary>
        public SidePanelFrameModel()
        {
            _left = new SidePanel();
            _right = new SidePanel();
        }

        #region Memento

        public class Memento
        {
            public SidePanel.Memento Left { get; set; }
            public SidePanel.Memento Right { get; set; }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.Left = Left.CreateMemento();
            memento.Right = Right.CreateMemento();

            return memento;
        }

        public void Restore(Memento memento, List<IPanel> panels)
        {
            if (memento != null)
            {
                _left.Restore(memento.Left, panels);
                _right.Restore(memento.Right, panels);
            }

            // 未登録パネルをすべて登録
            foreach (var panel in panels.Where(e => !_left.Panels.Contains(e) && !_right.Panels.Contains(e)))
            {
                _left.Panels.Add(panel);
            }
        }

        #endregion
    }
}
