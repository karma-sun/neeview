using System;
using System.Windows;

namespace NeeView.Windows
{
    public class WindowStateChangeEventArgs : EventArgs
    {
        public WindowStateChangeEventArgs()
        {
        }

        public WindowStateChangeEventArgs(WindowState oldState, WindowState newState)
        {
            OldState = oldState;
            NewState = newState;
        }

        public WindowState OldState { get; set; }
        public WindowState NewState { get; set; }
    }

}
