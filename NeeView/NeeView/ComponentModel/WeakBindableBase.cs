using NeeLaboratory.ComponentModel;
using System;
using System.ComponentModel;
using System.Windows;

namespace NeeView.ComponentModel
{
    /// <summary>
    /// ModelのPropertyChangedを弱いイベントパターンで購読する
    /// </summary>
    /// <remarks>
    /// from: https://blog.okazuki.jp/entry/20110227/1298817065
    /// </remarks>
    /// <typeparam name="TModel">購読するModel</typeparam>
    public class WeakBindableBase<TModel> : BindableBase
        where TModel : INotifyPropertyChanged
    {
        // NOTE: GCの対象になるまで破棄されないように保持する
        private IWeakEventListener _propertyChangedListener;


        public WeakBindableBase(TModel model)
        {
            Model = model;

            _propertyChangedListener = new PropertyChangedWeakEventListener(base.RaisePropertyChanged);
            PropertyChangedEventManager.AddListener(Model, _propertyChangedListener, "");
        }


        public TModel Model { get; private set; }


        private class PropertyChangedWeakEventListener : IWeakEventListener
        {
            private Action<string> _raisePropertyChangedAction;

            public PropertyChangedWeakEventListener(Action<string> raisePropertyChangedAction)
            {
                _raisePropertyChangedAction = raisePropertyChangedAction;
            }

            public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
            {
                if (typeof(PropertyChangedEventManager) != managerType)
                {
                    return false;
                }

                var evt = e as PropertyChangedEventArgs;
                if (evt == null)
                {
                    return false;
                }

                _raisePropertyChangedAction(evt.PropertyName);
                return true;
            }
        }
    }

}
