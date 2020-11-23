using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace NeeView
{
    public class ViewControlMediator
    {
        static ViewControlMediator() => Current = new ViewControlMediator();
        public static ViewControlMediator Current { get; }


        private Dictionary<Window, IViewControllable> _map = new Dictionary<Window, IViewControllable>();
        private IViewControllable _defaultController = new EmptyViewController();

        public void Initialize()
        {
            var controller = new MainViewController();
            AddViewController(MainWindow.Current, controller);
            SetDefaultViewController(controller);
        }

        public void AddViewController(Window owner, IViewControllable controller)
        {
            if (owner is null) throw new ArgumentNullException(nameof(owner));
            if (controller is null) throw new ArgumentNullException(nameof(controller));

            _map[owner] = controller;
        }

        public void RemoveViewController(Window owner)
        {
            if (owner is null) throw new ArgumentNullException(nameof(owner));

            _map.Remove(owner);
        }

        public void SetDefaultViewController(IViewControllable controller)
        {
            if (controller is null) throw new ArgumentNullException();

            _defaultController = controller;
        }

        public IViewControllable GetViewController(object sender)
        {
            if (sender is FrameworkElement element)
            {
                var owner = Window.GetWindow(element);
                if (_map.TryGetValue(owner, out var controller))
                {
                    return controller;
                }
            }

            return _defaultController;
        }


        public void FlipHorizontal(object sender, bool isFlip)
        {
            GetViewController(sender).FlipHorizontal(isFlip);
        }

        public void FlipVertical(object sender, bool isFlip)
        {
            GetViewController(sender).FlipVertical(isFlip);
        }

        public void ToggleFlipHorizontal(object sender)
        {
            GetViewController(sender).ToggleFlipHorizontal();
        }

        public void ToggleFlipVertical(object sender)
        {
            GetViewController(sender).ToggleFlipVertical();
        }

        public void ResetContentSizeAndTransform(object sender)
        {
            GetViewController(sender).ResetContentSizeAndTransform();
        }

        public void ViewRotateLeft(object sender, ViewRotateCommandParameter parameter)
        {
            GetViewController(sender).ViewRotateLeft(parameter);
        }

        public void ViewRotateRight(object sender, ViewRotateCommandParameter parameter)
        {
            GetViewController(sender).ViewRotateRight(parameter);
        }

        public void ScaleDown(object sender, ViewScaleCommandParameter parameter)
        {
            GetViewController(sender).ScaleDown(parameter);
        }

        public void ScaleUp(object sender, ViewScaleCommandParameter parameter)
        {
            GetViewController(sender).ScaleUp(parameter);
        }

        public void ScrollUp(object sender, ViewScrollCommandParameter parameter)
        {
            GetViewController(sender).ScrollUp(parameter);
        }

        public void ScrollDown(object sender, ViewScrollCommandParameter parameter)
        {
            GetViewController(sender).ScrollDown(parameter);
        }

        public void ScrollLeft(object sender, ViewScrollCommandParameter parameter)
        {
            GetViewController(sender).ScrollLeft(parameter);
        }

        public void ScrollRight(object sender, ViewScrollCommandParameter parameter)
        {
            GetViewController(sender).ScrollRight(parameter);
        }

        public void ScrollNTypeUp(object sender, ViewScrollNTypeCommandParameter parameter)
        {
            GetViewController(sender).ScrollNTypeUp(parameter);
        }

        public void ScrollNTypeDown(object sender, ViewScrollNTypeCommandParameter parameter)
        {
            GetViewController(sender).ScrollNTypeDown(parameter);
        }
    }


    public interface IViewControllable
    {
        void FlipHorizontal(bool isFlip);
        void FlipVertical(bool isFlip);
        void ToggleFlipHorizontal();
        void ToggleFlipVertical();
        void ResetContentSizeAndTransform();
        void ViewRotateLeft(ViewRotateCommandParameter parameter);
        void ViewRotateRight(ViewRotateCommandParameter parameter);
        void ScaleDown(ViewScaleCommandParameter parameter);
        void ScaleUp(ViewScaleCommandParameter parameter);
        void ScrollUp(ViewScrollCommandParameter parameter);
        void ScrollDown(ViewScrollCommandParameter parameter);
        void ScrollLeft(ViewScrollCommandParameter parameter);
        void ScrollRight(ViewScrollCommandParameter parameter);
        void ScrollNTypeUp(ViewScrollNTypeCommandParameter parameter);
        void ScrollNTypeDown(ViewScrollNTypeCommandParameter parameter);
    }


    public class EmptyViewController : IViewControllable
    {
        public void FlipHorizontal(bool isFlip)
        {
        }

        public void FlipVertical(bool isFlip)
        {
        }

        public void ToggleFlipHorizontal()
        {
        }

        public void ToggleFlipVertical()
        {
        }

        public void ResetContentSizeAndTransform()
        {
        }

        public void ViewRotateLeft(ViewRotateCommandParameter parameter)
        {
        }

        public void ViewRotateRight(ViewRotateCommandParameter parameter)
        {
        }

        public void ScaleDown(ViewScaleCommandParameter parameter)
        {
        }

        public void ScaleUp(ViewScaleCommandParameter parameter)
        {
        }

        public void ScrollUp(ViewScrollCommandParameter parameter)
        {
        }

        public void ScrollDown(ViewScrollCommandParameter parameter)
        {
        }

        public void ScrollLeft(ViewScrollCommandParameter parameter)
        {
        }

        public void ScrollRight(ViewScrollCommandParameter parameter)
        {
        }

        public void ScrollNTypeUp(ViewScrollNTypeCommandParameter parameter)
        {
        }

        public void ScrollNTypeDown(ViewScrollNTypeCommandParameter parameter)
        {
        }
    }

    public class MainViewController : IViewControllable
    {
        public void FlipHorizontal(bool isFlip)
        {
            DragTransformControl.Current.FlipHorizontal(isFlip);
        }

        public void FlipVertical(bool isFlip)
        {
            DragTransformControl.Current.FlipVertical(isFlip);
        }

        public void ToggleFlipHorizontal()
        {
            DragTransformControl.Current.ToggleFlipHorizontal();
        }

        public void ToggleFlipVertical()
        {
            DragTransformControl.Current.ToggleFlipVertical();
        }

        public void ResetContentSizeAndTransform()
        {
            ContentCanvas.Current.ResetContentSizeAndTransform();
        }

        public void ViewRotateLeft(ViewRotateCommandParameter parameter)
        {
            ContentCanvas.Current.ViewRotateLeft(parameter);
        }

        public void ViewRotateRight(ViewRotateCommandParameter parameter)
        {
            ContentCanvas.Current.ViewRotateRight(parameter);
        }

        public void ScaleDown(ViewScaleCommandParameter parameter)
        {
            DragTransformControl.Current.ScaleDown(parameter.Scale, parameter.IsSnapDefaultScale, ContentCanvas.Current.MainContentScale);
        }

        public void ScaleUp(ViewScaleCommandParameter parameter)
        {
            DragTransformControl.Current.ScaleUp(parameter.Scale, parameter.IsSnapDefaultScale, ContentCanvas.Current.MainContentScale);
        }

        public void ScrollUp(ViewScrollCommandParameter parameter)
        {
            DragTransformControl.Current.ScrollUp(parameter);
        }

        public void ScrollDown(ViewScrollCommandParameter parameter)
        {
            DragTransformControl.Current.ScrollDown(parameter);
        }

        public void ScrollLeft(ViewScrollCommandParameter parameter)
        {
            DragTransformControl.Current.ScrollLeft(parameter);
        }

        public void ScrollRight(ViewScrollCommandParameter parameter)
        {
            DragTransformControl.Current.ScrollRight(parameter);
        }

        public void ScrollNTypeUp(ViewScrollNTypeCommandParameter parameter)
        {
            MainWindowModel.Current.ScrollNTypeUp(parameter);
        }

        public void ScrollNTypeDown(ViewScrollNTypeCommandParameter parameter)
        {
            MainWindowModel.Current.ScrollNTypeDown(parameter);
        }
    }


}
