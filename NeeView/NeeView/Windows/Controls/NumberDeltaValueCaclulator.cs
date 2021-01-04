namespace NeeView.Windows.Controls
{
    public class NumberDeltaValueCaclulator : IValueDeltaCalculator
    {
        public double Scale { get; set; } = 1.0;

        public object Calc(object value, int delta)
        {
            switch (value)
            {
                case int n:
                    return (int)(n + delta * Scale);
                case double n:
                    return n + delta * Scale;
                default:
                    return value;
            }
        }
    }
}
