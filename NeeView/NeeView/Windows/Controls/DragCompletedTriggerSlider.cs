using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace NeeView.Windows.Controls
{
    /// <summary>
    /// ドラッグ中に Value の UpdateSource を発行しないスライダー。
    /// Binding には UpdateSourceTrigger=Explicit を設定する必要があります。
    /// </summary>
    public class DragCompletedTriggerSlider : Slider
    {
        private bool _isDragging;

        protected override void OnValueChanged(double oldValue, double newValue)
        {
            base.OnValueChanged(oldValue, newValue);
            if (!_isDragging)
            {
                UpdateSource();
            }
        }

        protected override void OnThumbDragStarted(DragStartedEventArgs e)
        {
            base.OnThumbDragStarted(e);
            _isDragging = true;
        }

        protected override void OnThumbDragCompleted(DragCompletedEventArgs e)
        {
            base.OnThumbDragCompleted(e);
            _isDragging = false;

            UpdateSource();
        }

        private void UpdateSource()
        {
            var expression = BindingOperations.GetBindingExpression(this, ValueProperty);
            expression?.UpdateSource();
        }
    }
}
