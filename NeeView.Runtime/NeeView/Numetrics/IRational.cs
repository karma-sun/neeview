namespace NeeView.Numetrics
{
    public interface IRational
    {
        double ToValue();
        string ToRationalString();

        IRational Reduction();
    }
}
