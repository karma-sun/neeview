namespace NeeView.Runtime.LayoutPanel
{
    public interface ILayoutPanelWindowBuilder
    {
        LayoutPanelWindow CreateWindow(LayoutPanelWindowManager manager, LayoutPanel layoutPanel);
    }
}
