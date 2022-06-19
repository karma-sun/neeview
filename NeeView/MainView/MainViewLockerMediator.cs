using NeeView.Windows.Data;
using System;

namespace NeeView
{
    public class MainViewLockerMediator
    {
        private MainView _mainView;
        private MainViewLockerKey _currentKey;
        private DelayValue<bool> _lockValue;

        public MainViewLockerMediator(MainView mainView)
        {
            _mainView = mainView;
            _lockValue = new DelayValue<bool>(false);
            _lockValue.SetInterval(10.0);
            _lockValue.ValueChanged += LockValue_ValueChanged;
        }

        private void LockValue_ValueChanged(object sender, EventArgs e)
        {
            _mainView.SetResizeLock(_lockValue.Value);
        }


        public MainViewLockerKey CreateKey()
        {
            return new MainViewLockerKey(this);
        }


        public void Activate(MainViewLockerKey key)
        {
            if (key is null) throw new ArgumentNullException();
            if (_currentKey == key) return;

            _currentKey = key;

            _lockValue.SetValue(false, 0.0, DelayValueOverwriteOption.Force);
            _mainView.UpdateViewSize();
        }

        public void Deativate(MainViewLockerKey key)
        {
            if (_currentKey != key) return;

            _currentKey = null;
        }


        public void Lock(MainViewLockerKey key)
        {
            if (_currentKey != key) return;

            _lockValue.SetValue(true, 0.0, DelayValueOverwriteOption.Force);
        }

        public void Unlock(MainViewLockerKey key, double delayMilliseconds)
        {
            if (_currentKey != key) return;

            _lockValue.SetValue(false, delayMilliseconds, DelayValueOverwriteOption.Extend);
        }
    }


}
