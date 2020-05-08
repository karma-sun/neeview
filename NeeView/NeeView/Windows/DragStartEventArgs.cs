using System;
using System.Windows;
using System.Windows.Input;


namespace NeeView.Windows
{
    public class DragStartEventArgs : EventArgs
    {
        private MouseEventArgs _mouseEventArgs;

        public DragStartEventArgs(object dragItem, DataObject data, DragDropEffects allowedEffects, MouseEventArgs mouseEventArgs)
        {
            _mouseEventArgs = mouseEventArgs;
            this.DragItem = dragItem;
            this.Data = data;
            this.AllowedEffects = allowedEffects;
        }

        public bool Cancel { get; set; }

        public object DragItem { get; set; }

        public DataObject Data { get; set; }

        public DragDropEffects AllowedEffects { get; set; }

        public Point GetPosition(IInputElement relativeTo)
        {
            return _mouseEventArgs.GetPosition(relativeTo);
        }
    }
}
