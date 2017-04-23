using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using NeeView.Windows;

namespace NeeView.Windows.Controls
{
    /// <summary>
    /// SidePanelFrame ViewModel
    /// </summary>
    public class SidePanelFrameViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// PanelVisibility Changed
        /// </summary>
        public event EventHandler PanelVisibilityChanged;

        /// <summary>
        /// PropertyChanged event. 
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        //
        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        /// <summary>
        /// Width property.
        /// </summary>
        public double Width
        {
            get { return _width; }
            set
            {
                if (IsValid && _width != value)
                {
                    _width = Math.Max(value, Left.Width + Right.Width);
                    UpdateLeftMaxWidth();
                    UpdateRightMaxWidth();
                    RaisePropertyChanged();
                }
            }
        }

        //
        private double _width;

        //
        private void UpdateLeftMaxWidth()
        {
            Left.MaxWidth = _width - Right.Width;
        }

        //
        private void UpdateRightMaxWidth()
        {
            Right.MaxWidth = _width - Left.Width;
        }


        /// <summary>
        /// IsAutoHide property.
        /// </summary>
        public bool IsAutoHide
        {
            get { return this.Left.IsAutoHide; }
            set
            {
                if (this.Left.IsAutoHide != value)
                {
                    this.Left.IsAutoHide = value;
                    this.Right.IsAutoHide = value;
                    RaisePropertyChanged();
                    PanelVisibilityChanged?.Invoke(this, null);
                }
            }
        }



        /// <summary>
        /// Model property.
        /// </summary>
        public SidePanelFrameModel Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        //
        private SidePanelFrameModel _model;


        /// <summary>
        /// LeftPanel
        /// </summary>
        public LeftPanelViewModel Left { get; private set; }

        /// <summary>
        /// RightPanel
        /// </summary>
        public RightPanelViewModel Right { get; private set; }


        //
        public bool IsValid { get; private set; }

        //
        public DragStartDescription DragStartDescription { get; private set; }


        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="model"></param>
        public SidePanelFrameViewModel(SidePanelFrameModel model, ItemsControl leftItemsControl, ItemsControl rightItemsControl)
        {
            if (model == null) return;

            _model = model;

            Left = new LeftPanelViewModel(_model.Left, leftItemsControl);
            Left.PropertyChanged += Left_PropertyChanged;
            Left.PanelDroped += Left_PanelDroped;

            Right = new RightPanelViewModel(_model.Right, rightItemsControl);
            Right.PropertyChanged += Right_PropertyChanged;
            Right.PanelDroped += Right_PanelDroped;

            DragStartDescription = new DragStartDescription();
            DragStartDescription.DragStart += DragStartDescription_DragStart;
            DragStartDescription.DragEnd += DragStartDescription_DragEnd;

            IsValid = true;
        }

        private void DragStartDescription_DragStart(object sender, EventArgs e)
        {
            Left.IsDragged = true;
            Right.IsDragged = true;
        }

        private void DragStartDescription_DragEnd(object sender, EventArgs e)
        {
            Left.IsDragged = false;
            Right.IsDragged = false;
        }

        private void Left_PanelDroped(object sender, PanelDropedEventArgs e)
        {
            if (Right.Panel.Panels.Contains(e.Panel))
            {
                Right.Panel.Remove(e.Panel);
            }

            Left.Panel.Add(e.Panel, e.Index);
        }

        private void Right_PanelDroped(object sender, PanelDropedEventArgs e)
        {
            if (Left.Panel.Panels.Contains(e.Panel))
            {
                Left.Panel.Remove(e.Panel);
            }

            Right.Panel.Add(e.Panel, e.Index);
        }

        //
        private void Right_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Right.Width):
                    UpdateLeftMaxWidth();
                    //PanelVisibilityChanged?.Invoke(this, null);
                    break;
                case nameof(Right.PanelVisibility):
                    PanelVisibilityChanged?.Invoke(this, null);
                    break;
            }
        }

        //
        private void Left_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Left.Width):
                    UpdateRightMaxWidth();
                    //PanelVisibilityChanged?.Invoke(this, null);
                    break;
                case nameof(Left.PanelVisibility):
                    PanelVisibilityChanged?.Invoke(this, null);
                    break;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="size"></param>
        internal void UpdateVisibility(Point point, Size size)
        {
            Left.UpdateVisibility(point, size);
            Right.UpdateVisibility(point, size);
        }
    }
}
