using System.Linq;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// 複数のTouchInputから適切なものを選んでそのエミュレーターを実行する
    /// </summary>
    public class TouchEmurlateController
    {
        public void Execute(object sender)
        {
            GetActiveTouchInput(sender)?.Emulator.Execute();
        }

        // NOTE: no use sender
        private TouchInput GetActiveTouchInput(object sender)
        {
            var mainViewManager = MainViewManager.Current;

            // find active window
            var window = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            if (window is null)
            {
                return null;
            }

            var main = mainViewManager.MainView.TouchInput;
            var sub = mainViewManager.MainViewBay.TouchInput;

            if (window == MainWindow.Current)
            {
                return mainViewManager.Window is null ? main : sub;
            }
            else if (window == mainViewManager.Window)
            {
                return main;
            }
            else
            {
                return null;
            }
        }

    }
}
