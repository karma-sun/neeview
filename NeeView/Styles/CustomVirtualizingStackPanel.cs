using System.Windows.Controls;

namespace NeeView
{
    public class CustomVirtualizingStackPanel : VirtualizingStackPanel
    {
        public void BringIntoView(int index)
        {
            this.BringIndexIntoView(index);
        }
    }
}
