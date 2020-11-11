using NeeLaboratory.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// ウィンドウ切り替え
    /// </summary>
    public class WindowActivator
    {
        public static WindowActivator Current { get; }
        static WindowActivator() => Current = new WindowActivator();

        private WindowActivator()
        {
            RemoteCommandService.Current.AddReciever("ActivateMainWindow", ActivateMainWindow);
        }

        private void ActivateMainWindow(RemoteCommand command)
        {
            Application.Current.MainWindow.Activate();
        }


        public void NextActivate(int direction)
        {
            var changed = NextSubWindow(direction, false);
            if (changed) return;

            var process = ProcessActivator.NextActivate(direction);
            if (process != null)
            {
                RemoteCommandService.Current.Send(new RemoteCommand("ActivateMainWindow"), new RemoteCommandDelivery(process.Id));
                return;
            }

            if (GetSubWindows().Any())
            {
                Application.Current.MainWindow.Activate();
            }
        }

        public IEnumerable<Window> GetSubWindows()
        {
            return MainLayoutPanelManager.Current.Windows.Windows.Cast<Window>();
        }

        public bool NextSubWindow(int direction, bool loopable)
        {
            var windows = new List<Window>() { Application.Current.MainWindow };
            var subWindows = GetSubWindows();
            windows.AddRange(direction > 0 ? subWindows : subWindows.Reverse());

            if (!loopable && windows.Last().IsActive) return false;

            var activeWindow = windows.FirstOrDefault(e => e.IsActive);
            if (activeWindow is null)
            {
                var isActived = windows.First().Activate();
                //Debug.WriteLine($"Activate: {isActived}: {windows.First().Title}");
            }
            else
            {
                var index = (windows.IndexOf(activeWindow) + 1) % windows.Count;
                var isActived = windows[index].Activate();
                //Debug.WriteLine($"Activate: {isActived}: {windows[index].Title}");
            }

            return true;
        }



    }

}
