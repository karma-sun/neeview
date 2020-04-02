using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// なにかキーが押されているかを監視する
    /// </summary>
    public class KeyPressWatcher : IDisposable
    {
        private UIElement _target;
        private LinkedList<Key> _keys;

        public KeyPressWatcher(UIElement target)
        {
            _target = target;
            _keys = new LinkedList<Key>();

            _target.PreviewKeyDown += Target_PreviewKeyDown;
            _target.PreviewKeyUp += Target_PreviewKeyUp;
        }


        public event EventHandler<KeyEventArgs> PreviewKeyDown;
        public event EventHandler<KeyEventArgs> PreviewKeyUp;


        public bool IsPressed
        {
            get
            {
                if (_keys.Any() && _keys.All(e => Keyboard.IsKeyUp(e)))
                {
                    _keys.Clear();
                    return IsModifierKeysPressed;
                }
                else
                {
                    ////if (_keys.Any()) Debug.WriteLine("AnyKey: " + string.Join(",", _keys));
                    return _keys.Any() || IsModifierKeysPressed;
                }
            }
        }

        public bool IsModifierKeysPressed => Keyboard.Modifiers != ModifierKeys.None;


        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _target.PreviewKeyDown -= Target_PreviewKeyDown;
                    _target.PreviewKeyUp -= Target_PreviewKeyUp;
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion


        private void Target_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            AddKey(e.Key);
            PreviewKeyDown?.Invoke(sender, e);
        }

        private void Target_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            RemoveKey(e.Key);
            PreviewKeyUp?.Invoke(sender, e);
        }

        private void AddKey(Key key)
        {
            if (!RoutedCommandTable.Current.IsUsedKey(key)) return;

            if (_keys.Contains(key)) return;
            _keys.AddLast(key);
        }

        private void RemoveKey(Key key)
        {
            _keys.Remove(key);
        }
    }
}
