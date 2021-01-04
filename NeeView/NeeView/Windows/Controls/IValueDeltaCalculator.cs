namespace NeeView.Windows.Controls
{
    public interface IValueDeltaCalculator
    {
        object Calc(object value, int delta);
    }
}
