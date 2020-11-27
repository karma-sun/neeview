using NeeLaboratory.ComponentModel;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public class ViewComponent : IDisposable
    {
        static ViewComponent() => Current = new ViewComponent();
        public static ViewComponent Current { get; }


        private MainView _mainView;
        private bool _disposedValue;


        // TODO: MainView依存はおかしい
        // TODO: 各種シングルトン依存の排除
        public void Initialize()
        {
            var mouseGestureCommandCollection = MouseGestureCommandCollection.Current;
            var bookHub = BookHub.Current;

            _mainView = new MainView();

            DragTransform = new DragTransform();
            DragTransformControl = new DragTransformControl(DragTransform, _mainView.View, _mainView.MainContentShadow);
            LoupeTransform = new LoupeTransform();

            MouseInput = new MouseInput(new MouseInputContext(_mainView.View, mouseGestureCommandCollection, DragTransformControl, DragTransform, LoupeTransform));
            TouchInput = new TouchInput(new TouchInputContext(_mainView.View, _mainView.MainContentShadow, mouseGestureCommandCollection, DragTransform, DragTransformControl));

            var scrollPageController = new ScrollPageController(this, BookSettingPresenter.Current, BookOperation.Current);
            var printController = new PrintController(this, _mainView);
            ViewController = new ViewController(this, scrollPageController, printController);

            ContentCanvas = new ContentCanvas(this, bookHub);
            ContentCanvasBrush = new ContentCanvasBrush(ContentCanvas);

            ContentRebuild = new ContentRebuild(this);

            _mainView.DataContext = new MainViewViewModel(this);
        }


        public event EventHandler OpenContextMenuRequest;
        public event EventHandler FocusMainViewRequest;


        public MainView MainView => _mainView;

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

        public void RaiseOpenContextMenuRequest()
        {
            OpenContextMenuRequest?.Invoke(this, null);
        }

        public void RaiseFocusMainViewRequest()
        {
            FocusMainViewRequest?.Invoke(this, null);
        }
    }
}
