namespace NeeView
{
    public interface IHasDeviceInput
    {
        MouseInput MouseInput { get; }
        TouchInput TouchInput { get; }
    }
}
