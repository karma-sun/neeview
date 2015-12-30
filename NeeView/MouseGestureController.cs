using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    public class MouseGestureEx : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
        #endregion

        public MouseGestureCommandBinding CommandBinding { get; private set; }
        public MouseGestureController Controller { get; private set; }

        #region Property: GestureText
        private string _GestureText;
        public string GestureText
        {
            get { return _GestureText; }
            set { _GestureText = value; OnPropertyChanged(); }
        }
        #endregion



        public event EventHandler<MouseButtonEventArgs> MouseClickEventHandler;

        public MouseGestureEx(FrameworkElement sender)
        {
            CommandBinding = new MouseGestureCommandBinding();

            Controller = new MouseGestureController(sender);
            Controller.MouseGestureUpdateEventHandler += OnMouseGestureUpdate;
            Controller.MouseGestureExecuteEventHandler += OnMouseGesture;
            Controller.MouseClickEventHandler += OnMouseClick;
        }

        public void ClearClickEventHandler()
        {
            MouseClickEventHandler = null;
        }

        private void OnMouseClick(object sender, MouseButtonEventArgs e)
        {
            MouseClickEventHandler?.Invoke(sender, e);
        }

        private void OnMouseGesture(object sender, MouseGestureCollection e)
        {
            CommandBinding.Execute(e);
        }

        private void OnMouseGestureUpdate(object sender, MouseGestureCollection e)
        {
            GestureText = e.ToString();
        }

        public string GetGestureCommandName()
        {
            var command = CommandBinding.GetCommand(Controller.Gesture);
            if (command != null)
            {
                BookCommandType commandType;
                if (Enum.TryParse<BookCommandType>(command.Name, out commandType))
                {
                    return BookCommandExtension.Headers[commandType].Text;
                }
            }

            return null;
        }

        public string GetGestureString()
        {
            return Controller.Gesture.ToDispString();
        }

        public string GetGestureText()
        {
            string commandName = GetGestureCommandName();
            return ((commandName != null) ? commandName + "\n" : "")  + Controller.Gesture.ToDispString();
        }
    }

    public class MouseGestureCommandBinding
    {
        Dictionary<string, RoutedCommand> _Commands;

        public MouseGestureCommandBinding()
        {
            _Commands = new Dictionary<string, RoutedCommand>();
        }

        public void Clear()
        {
            _Commands.Clear();
        }

        public void Add(string gestureText, RoutedCommand command)
        {
            _Commands[gestureText] = command;
        }


        public RoutedCommand GetCommand(MouseGestureCollection gesture)
        {
            string key = gesture.ToString();

            if (_Commands.ContainsKey(key))
            {
                return _Commands[key];
            }
            else
            {
                return null;
            }
        }

        public void Execute(MouseGestureCollection gesture)
        {
            Execute(gesture.ToString());
        }

        public void Execute(string gestureText)
        {
            if (_Commands.ContainsKey(gestureText))
            {
                if (_Commands[gestureText].CanExecute(null, null))
                {
                    _Commands[gestureText].Execute(null, null);
                }
            }
        }
    }


    public enum MouseGestureDirection
    {
        None,
        Up,
        Right,
        Down,
        Left
    }

    public class MouseGestureCollection : ObservableCollection<MouseGestureDirection>
    {
        public override string ToString()
        {
            string gestureText = "";
            foreach (var e in this)
            {
                gestureText += e.ToString()[0];
            }

            return gestureText;
        }

        private static Dictionary<MouseGestureDirection, string> _DispStrings = new Dictionary<MouseGestureDirection, string>
        {
            [MouseGestureDirection.None] = "",
            [MouseGestureDirection.Up] = "↑",
            [MouseGestureDirection.Right] = "→",
            [MouseGestureDirection.Down] = "↓",
            [MouseGestureDirection.Left] = "←",
        };


        public string ToDispString()
        {
            string gestureText = "";
            foreach (var e in this)
            {
                gestureText += _DispStrings[e];
            }

            return gestureText;
        }
    }


    public class MouseGestureController : INotifyPropertyChanged
    {
#region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(name));
            }
        }
#endregion


        private FrameworkElement _Sender;

        private bool _IsButtonDown = false;
        private bool _IsDragging = false;

        private bool _IsEnableClickEvent;

        private Point _StartPoint;
        private Point _EndPoint;

        MouseGestureDirection _Direction;

        static Dictionary<MouseGestureDirection, Vector> GestureDirectionVector = new Dictionary<MouseGestureDirection, Vector>
        {
            [MouseGestureDirection.None] = new Vector(0, 0),
            [MouseGestureDirection.Up] = new Vector(0, -1),
            [MouseGestureDirection.Right] = new Vector(1, 0),
            [MouseGestureDirection.Down] = new Vector(0, 1),
            [MouseGestureDirection.Left] = new Vector(-1, 0)
        };


#region Property: Gesture
        private MouseGestureCollection _Gesture;
        public MouseGestureCollection Gesture
        {
            get { return _Gesture; }
            set { _Gesture = value; OnPropertyChanged(); }
        }
#endregion


        public MouseGestureController(FrameworkElement sender)
        {
            _Sender = sender;

            _Gesture = new MouseGestureCollection();
            _Gesture.CollectionChanged += (s, e) => MouseGestureUpdateEventHandler.Invoke(this, _Gesture);

            _Sender.PreviewMouseRightButtonDown += OnMouseButtonDown;
            _Sender.PreviewMouseRightButtonUp += OnMouseButtonUp;
            _Sender.PreviewMouseWheel += OnMouseWheel;
            _Sender.PreviewMouseMove += OnMouseMove;
        }




        public void Reset()
        {
            _Direction = MouseGestureDirection.None;
            _Gesture.Clear();
        }
        
        private void OnMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            _StartPoint = e.GetPosition(_Sender);

            _IsButtonDown = true;
            _IsDragging = false;
            _IsEnableClickEvent = true;

            Reset();

            _Sender.CaptureMouse();
        }

        public event EventHandler<MouseGestureCollection> MouseGestureUpdateEventHandler;
        public event EventHandler<MouseGestureCollection> MouseGestureExecuteEventHandler;
        public event EventHandler<MouseButtonEventArgs> MouseClickEventHandler;

        private void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_IsButtonDown) return;
            _IsButtonDown = false;

            _Sender.ReleaseMouseCapture();

            if (_IsEnableClickEvent && _IsDragging && _Gesture.Count > 0)
            {
                MouseGestureExecuteEventHandler?.Invoke(this, _Gesture);
            }
            else if (_IsEnableClickEvent)
            {
                MouseClickEventHandler(sender, e);
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_IsButtonDown) return;

            _EndPoint = e.GetPosition(_Sender);

            if (!_IsDragging)
            {
                if (Math.Abs(_EndPoint.X - _StartPoint.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(_EndPoint.Y - _StartPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    _IsDragging = true;
                    _StartPoint = e.GetPosition(_Sender);
                }
                else
                {
                    return;
                }
            }

            DragMove(_StartPoint, _EndPoint);
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // クリック系のイベントを無効にする
            _IsEnableClickEvent = false;
        }

        // 移動
        private void DragMove(Point start, Point end)
        {
            var v1 = _EndPoint - _StartPoint;

            double lengthX = SystemParameters.MinimumHorizontalDragDistance;
            double lengthY = SystemParameters.MinimumVerticalDragDistance;
            //double length = 8;

            // 一定距離未満は判定しない
            if (Math.Abs(v1.X) < lengthX && Math.Abs(v1.Y) < lengthY) return;

            // 方向を決める
            // 斜め方向は以前の方向とする

            if (_Direction != MouseGestureDirection.None && Math.Abs(Vector.AngleBetween(GestureDirectionVector[_Direction], v1)) < 45)
            {
                // そのまま
            }
            else
            {
                foreach (MouseGestureDirection direction in Enum.GetValues(typeof(MouseGestureDirection)))
                {
                    var v0 = GestureDirectionVector[direction];
                    var angle = Vector.AngleBetween(GestureDirectionVector[direction], v1);
                    if (direction != MouseGestureDirection.None && Math.Abs(Vector.AngleBetween(GestureDirectionVector[direction], v1)) < 30)
                    {
                        _Direction = direction;
                        _Gesture.Add(_Direction);
                        break;
                    }
                }
            }

            // 開始点の更新
            _StartPoint = _EndPoint;
        }

    }
}
