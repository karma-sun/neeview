using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// 複数のMouseInputのイベントをまとめて受信 (未使用)
    /// </summary>
    public class MouseInputProxy
    {
        static MouseInputProxy() => Current = new MouseInputProxy();
        public static MouseInputProxy Current { get; }


        private List<MouseInput> _mouseInputs = new List<MouseInput>();


        public event EventHandler<MouseButtonEventArgs> MouseButtonChanged;

        public event EventHandler<MouseWheelEventArgs> MouseWheelChanged;

        public event EventHandler<MouseEventArgs> MouseMoved;


        public void Add(MouseInput mouseInput)
        {
            if (mouseInput is null) throw new ArgumentNullException();

            if (_mouseInputs.Contains(mouseInput)) return;

            mouseInput.MouseButtonChanged += (s, e) => MouseButtonChanged?.Invoke(s, e);
            mouseInput.MouseWheelChanged += (s, e) => MouseWheelChanged?.Invoke(s, e);
            mouseInput.MouseMoved += (s, e) => MouseMoved?.Invoke(s, e);

            _mouseInputs.Add(mouseInput);
        }
    }
}
