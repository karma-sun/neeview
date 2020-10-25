using System;
using System.Windows;

namespace NeeView.Windows
{
    public class WindowStateChangeEventArgs : EventArgs
    {
        public WindowStateChangeEventArgs()
        {
        }

        public WindowStateChangeEventArgs(Window window, WindowState oldState, WindowState newState)
        {
            Window = window;
            OldState = oldState;
            NewState = newState;
        }

        public Window Window { get; set; }
        public WindowState OldState { get; set; }
        public WindowState NewState { get; set; }
    }

}
