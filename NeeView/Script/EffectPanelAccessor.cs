namespace NeeView
{
    public class EffectPanelAccessor : LayoutPanelAccessor
    {
        private ImageEffectPanel _panel;


        public EffectPanelAccessor() : base(nameof(ImageEffectPanel))
        {
            _panel = (ImageEffectPanel)MainLayoutPanelManager.Current.GetPanel(nameof(ImageEffectPanel));
        }


        internal WordNode CreateWordNode(string name)
        {
            return WordNodeHelper.CreateClassWordNode(name, this.GetType());
        }
    }

}