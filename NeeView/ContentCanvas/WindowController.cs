namespace NeeView
{
    public interface IHasWindowController
    {
        WindowController WindowController { get; }
    }

    public class WindowController
    {
        WindowStateManager _windowStateManager;
        ITopmostControllable _topmostController;

        public WindowController(WindowStateManager windowStateManager, ITopmostControllable topmostController)
        {
            _windowStateManager = windowStateManager;
            _topmostController = topmostController;
        }

        public WindowStateManager WindowStateManager => _windowStateManager;


        public void ToggleMinimize()
        {
            _windowStateManager.ToggleMinimize();
        }

        public void ToggleMaximize()
        {
            _windowStateManager.ToggleMaximize();
        }

        public void ToggleFullScreen()
        {
            _windowStateManager.ToggleFullScreen();
        }

        public void SetFullScreen(bool isFullScreen)
        {
            _windowStateManager.SetFullScreen(isFullScreen);
        }

        public void ToggleTopmost()
        {
            _topmostController.ToggleTopmost();
        }
    }
}
