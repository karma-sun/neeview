namespace NeeView
{
    public class NavigatorPanelAccessor : LayoutPanelAccessor
    {
        private NavigatePanel _panel;


        public NavigatorPanelAccessor() : base(nameof(NavigatePanel))
        {
            _panel = (NavigatePanel)MainLayoutPanelManager.Current.GetPanel(nameof(NavigatePanel));
        }


        internal WordNode CreateWordNode(string name)
        {
            return WordNodeHelper.CreateClassWordNode(name, this.GetType());
        }
    }

}