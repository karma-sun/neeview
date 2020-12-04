using System;
using System.Collections.Generic;

namespace NeeView
{
    public class ViewController
    {
        private ViewComponent _viewComponent;
        private ScrollPageController _scrollPageControl;
        private PrintController _printControl;
        private TouchEmurlateController _touchEmurlateController;
        private WindowStateController _windowStateController;

        public ViewController(ViewComponent viewContent, ScrollPageController scrollPageControl, PrintController printControl)
        {
            _viewComponent = viewContent;
            _scrollPageControl = scrollPageControl;
            _printControl = printControl;
            _touchEmurlateController = new TouchEmurlateController();
            _windowStateController = new WindowStateController(MainWindow.Current);
        }

        public void FlipHorizontal(bool isFlip)
        {
            _viewComponent.DragTransformControl.FlipHorizontal(isFlip);
        }

        public void FlipVertical(bool isFlip)
        {
            _viewComponent.DragTransformControl.FlipVertical(isFlip);
        }

        public void ToggleFlipHorizontal()
        {
            _viewComponent.DragTransformControl.ToggleFlipHorizontal();
        }

        public void ToggleFlipVertical()
        {
            _viewComponent.DragTransformControl.ToggleFlipVertical();
        }

        public void ResetContentSizeAndTransform()
        {
            _viewComponent.ContentCanvas.ResetContentSizeAndTransform();
        }

        public void ViewRotateLeft(ViewRotateCommandParameter parameter)
        {
            _viewComponent.ContentCanvas.ViewRotateLeft(parameter);
        }

        public void ViewRotateRight(ViewRotateCommandParameter parameter)
        {
            _viewComponent.ContentCanvas.ViewRotateRight(parameter);
        }

        public void ScaleDown(ViewScaleCommandParameter parameter)
        {
            _viewComponent.DragTransformControl.ScaleDown(parameter.Scale, parameter.IsSnapDefaultScale, _viewComponent.ContentCanvas.MainContentScale);
        }

        public void ScaleUp(ViewScaleCommandParameter parameter)
        {
            _viewComponent.DragTransformControl.ScaleUp(parameter.Scale, parameter.IsSnapDefaultScale, _viewComponent.ContentCanvas.MainContentScale);
        }

        public void ScrollUp(ViewScrollCommandParameter parameter)
        {
            _viewComponent.DragTransformControl.ScrollUp(parameter);
        }

        public void ScrollDown(ViewScrollCommandParameter parameter)
        {
            _viewComponent.DragTransformControl.ScrollDown(parameter);
        }

        public void ScrollLeft(ViewScrollCommandParameter parameter)
        {
            _viewComponent.DragTransformControl.ScrollLeft(parameter);
        }

        public void ScrollRight(ViewScrollCommandParameter parameter)
        {
            _viewComponent.DragTransformControl.ScrollRight(parameter);
        }

        public void ScrollNTypeUp(ViewScrollNTypeCommandParameter parameter)
        {
            _scrollPageControl.ScrollNTypeUp(parameter);
        }

        public void ScrollNTypeDown(ViewScrollNTypeCommandParameter parameter)
        {
            _scrollPageControl.ScrollNTypeDown(parameter);
        }

        public void PrevScrollPage(object sender, ScrollPageCommandParameter parameter)
        {
            _scrollPageControl.PrevScrollPage(sender, parameter);
        }

        public void NextScrollPage(object sender, ScrollPageCommandParameter parameter)
        {
            _scrollPageControl.NextScrollPage(sender, parameter);
        }

        public void SetLoupeMode(bool isLoupeMode)
        {
            _viewComponent.MouseInput.IsLoupeMode = isLoupeMode;
        }

        public bool GetLoupeMode()
        {
            return _viewComponent.MouseInput.IsLoupeMode;
        }

        public void ToggleLoupeMode()
        {
            _viewComponent.MouseInput.IsLoupeMode = !_viewComponent.MouseInput.IsLoupeMode;
        }

        public void LoupeZoomIn()
        {
            _viewComponent.MouseInput.Loupe.LoupeZoomIn();
        }

        public void LoupeZoomOut()
        {
            _viewComponent.MouseInput.Loupe.LoupeZoomOut();
        }


        public bool CanCopyImageToClipboard()
        {
            return _viewComponent.ContentCanvas.CanCopyImageToClipboard();
        }

        public void CopyImageToClipboard()
        {
            _viewComponent.ContentCanvas.CopyImageToClipboard();
        }

        public bool TestStretchMode(PageStretchMode mode, bool isToggle)
        {
            return _viewComponent.ContentCanvas.TestStretchMode(mode, isToggle);
        }

        public void SetStretchMode(PageStretchMode mode, bool isToggle)
        {
            _viewComponent.ContentCanvas.SetStretchMode(mode, isToggle);
        }

        public PageStretchMode GetToggleStretchMode(ToggleStretchModeCommandParameter parameter)
        {
            return _viewComponent.ContentCanvas.GetToggleStretchMode(parameter);
        }

        public PageStretchMode GetToggleStretchModeReverse(ToggleStretchModeCommandParameter parameter)
        {
            return _viewComponent.ContentCanvas.GetToggleStretchModeReverse(parameter);
        }


        public bool GetAutoRotateLeft()
        {
            return _viewComponent.ContentCanvas.IsAutoRotateLeft;
        }

        public void SetAutoRotateLeft(bool flag)
        {
            _viewComponent.ContentCanvas.IsAutoRotateLeft = flag;
        }

        public void ToggleAutoRotateLeft()
        {
            _viewComponent.ContentCanvas.IsAutoRotateLeft = !_viewComponent.ContentCanvas.IsAutoRotateLeft;
        }

        public bool GetAutoRotateRight()
        {
            return _viewComponent.ContentCanvas.IsAutoRotateRight;
        }

        public void SetAutoRotateRight(bool flag)
        {
            _viewComponent.ContentCanvas.IsAutoRotateRight = flag;
        }

        public void ToggleAutoRotateRight()
        {
            _viewComponent.ContentCanvas.IsAutoRotateRight = !_viewComponent.ContentCanvas.IsAutoRotateRight;
        }

        public bool CanPrint()
        {
            return _printControl.CanPrint();
        }

        public void Print()
        {
            _printControl.Print();
        }

        public void TouchInputEmutrate(object sender)
        {
            _touchEmurlateController.Execute(sender);
        }

        public void OpenContextMenu()
        {
            _viewComponent.RaiseOpenContextMenuRequest();
        }

        public void ToggleTopmost(object sender)
        {
            _windowStateController.ToggleTopmost(sender);
        }

        public void ToggleWindowMinimize(object sender)
        {
            _windowStateController.ToggleMinimize(sender);
        }

        public void ToggleWindowMaximize(object sender)
        {
            _windowStateController.ToggleMaximize(sender);
        }

        public void ToggleWindowFullScreen(object sender)
        {
            _windowStateController.ToggleFullScreen(sender);
        }

        public void SetFullScreen(object sender, bool isFullScreen)
        {
            _windowStateController.SetFullScreen(sender, isFullScreen);
        }

        public void StretchWindow()
        {
            _viewComponent.MainView.StretchWindow();
        }
    }

}
