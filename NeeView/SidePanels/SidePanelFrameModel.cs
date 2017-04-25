using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

namespace NeeView
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
        /// IsSideBarVisible property.
        /// </summary>
        public bool IsSideBarVisible
        {
            get { return _IsSideBarVisible; }
            set { if (_IsSideBarVisible != value) { _IsSideBarVisible = value; RaisePropertyChanged(); } }
        }

        //
        private bool _IsSideBarVisible;



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


        //
        public event EventHandler SelectedPanelChanged;


        /// <summary>
        /// 
        /// </summary>
        public SidePanelFrameModel()
        {
            _left = new SidePanel();
            _left.PropertyChanged += Left_PropertyChanged;

            _right = new SidePanel();
            _right.PropertyChanged += Right_PropertyChanged;
        }

        private void Right_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Right.SelectedPanel):
                    SelectedPanelChanged?.Invoke(Right, null);
                    break;
            }
        }

        private void Left_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Left.SelectedPanel):
                    SelectedPanelChanged?.Invoke(Left, null);
                    break;
            }
        }

        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public bool IsSideBarVisible { get; set; }

            [DataMember]
            public SidePanel.Memento Left { get; set; }

            [DataMember]
            public SidePanel.Memento Right { get; set; }

            public void Constructor()
            {
                IsSideBarVisible = true;
                Left = new SidePanel.Memento();
                Right = new SidePanel.Memento();
            }

            public Memento()
            {
                Constructor();
            }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                Constructor();
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.IsSideBarVisible = this.IsSideBarVisible;
            memento.Left = Left.CreateMemento();
            memento.Right = Right.CreateMemento();

            return memento;
        }

        public void Restore(Memento memento, List<IPanel> panels)
        {
            if (memento != null)
            {
                IsSideBarVisible = memento.IsSideBarVisible;
                _left.Restore(memento.Left, panels);
                _right.Restore(memento.Right, panels);
            }

            // 未登録パネルをすべて登録
            foreach (var panel in panels.Where(e => !_left.Panels.Contains(e) && !_right.Panels.Contains(e)))
            {
                _left.Panels.Add(panel);
            }

            // 情報更新
            SelectedPanelChanged?.Invoke(this, null);
        }

        #endregion
    }
}
