using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    public class ViewComponentProvider : IDisposable
    {
        static ViewComponentProvider() => Current = new ViewComponentProvider();
        public static ViewComponentProvider Current { get; }


        private Dictionary<Window, ViewComponent> _map = new Dictionary<Window, ViewComponent>();
        private ViewComponent _defaultViewComponent;
        private bool _disposedValue;

        public event EventHandler<MouseEventArgs> MouseMoved;


        private ViewComponentProvider()
        {
            BookHub.Current.BookChanged += BookHub_BookChanged;

            ApplicationDisposer.Current.Add(this);
        }

        private void BookHub_BookChanged(object sender, BookChangedEventArgs e)
        {
            var mouseInput = GetViewComponent().MouseInput;

            if (e.Address != null)
            {
                mouseInput.Cancel();
            }
            else
            {
                mouseInput.IsLoupeMode = false;
            }
        }

        public void Add(Window owner, ViewComponent component, bool isDefault = false)
        {
            if (owner is null) throw new ArgumentNullException(nameof(owner));
            if (component is null) throw new ArgumentNullException(nameof(component));

            component.MouseInput.MouseMoved += MouseMoved;
            _map[owner] = component;

            if (isDefault || _defaultViewComponent is null)
            {
                _defaultViewComponent = component;
            }
        }

        public void Remove(Window owner)
        {
            if (owner is null) throw new ArgumentNullException(nameof(owner));

            if (_map.TryGetValue(owner, out var component))
            {
                component.MouseInput.MouseMoved -= MouseMoved;
                _map.Remove(owner);
            }
        }

        public ViewComponent GetViewComponent(object sender = null)
        {
            if (sender is null)
            {
                return _defaultViewComponent ?? throw new InvalidOperationException();
            }
            if (sender is FrameworkElement element)
            {
                var owner = Window.GetWindow(element);
                if (_map.TryGetValue(owner, out var controller))
                {
                    return controller;
                }
            }

            return _defaultViewComponent ?? throw new InvalidOperationException();
        }

        public ViewController GetViewController(object sender)
        {
            return GetViewComponent().ViewController;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    foreach (var component in _map.Values)
                    {
                        component.Dispose();
                    }
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }


    public class ViewComponent : IDisposable
    {
        private bool _disposedValue;


        // TODO: MainView依存はおかしい
        // TODO: MouseGestureCommandCollection.Current
        public ViewComponent(MainWindow mainWindow)
        {
            var mouseGestureCommandCollection = MouseGestureCommandCollection.Current;
            var bookHub = BookHub.Current;

            DragTransform = new DragTransform();
            DragTransformControl = new DragTransformControl(DragTransform, mainWindow.MainView, mainWindow.MainContentShadow);
            LoupeTransform = new LoupeTransform();

            MouseInput = new MouseInput(new MouseInputContext(mainWindow.MainView, mouseGestureCommandCollection, DragTransformControl, DragTransform, LoupeTransform));
            TouchInput = new TouchInput(new TouchInputContext(mainWindow.MainView, mainWindow.MainContentShadow, mouseGestureCommandCollection, DragTransform, DragTransformControl));

            var scrollPageController = new ScrollPageController(this, BookSettingPresenter.Current, BookOperation.Current);
            var printController = new PrintController(this, mainWindow);
            ViewController = new ViewController(this, scrollPageController, printController);

            ContentCanvas = new ContentCanvas(this, bookHub);
            ContentCanvasBrush = new ContentCanvasBrush(ContentCanvas);

            ContentRebuild = new ContentRebuild(this);
        }

        public DragTransform DragTransform { get; private set; }
        public DragTransformControl DragTransformControl { get; private set; }
        public LoupeTransform LoupeTransform { get; private set; }

        public MouseInput MouseInput { get; private set; }
        public TouchInput TouchInput { get; private set; }

        public ViewController ViewController { get; private set; }

        public ContentCanvas ContentCanvas { get; private set; }
        public ContentCanvasBrush ContentCanvasBrush { get; private set; }

        public ContentRebuild ContentRebuild { get; private set; }



        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    ContentCanvas.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
