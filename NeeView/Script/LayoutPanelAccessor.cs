using NeeView.Runtime.LayoutPanel;

namespace NeeView
{
    public class LayoutPanelAccessor
    {
        private string _key;
        private CustomLayoutPanelManager _manager;
        private LayoutPanel _layoutPanel;

        public LayoutPanelAccessor(string key)
        {
            _key = key;
            _manager = CustomLayoutPanelManager.Current;
            _layoutPanel = _manager.Panels[key];
        }


        [WordNodeMember]
        public bool IsSelected
        {
            get { return AppDispatcher.Invoke(() => _manager.IsPanelSelected(_layoutPanel)); }
            set
            {
                if (value)
                {
                    Open();
                }
                else
                {
                    Close();
                }
            }
        }

        [WordNodeMember]
        public bool IsVisible
        {
            get { return AppDispatcher.Invoke(() => _manager.IsPanelVisible(_layoutPanel)); }
        }

        [WordNodeMember]
        public bool IsFloating
        {
            get { return AppDispatcher.Invoke(() => _manager.IsPanelFloating(_layoutPanel)); }
        }

        [WordNodeMember]
        public void Open()
        {
            AppDispatcher.Invoke(() => _manager.Open(_key, true));
        }

        [WordNodeMember]
        public void OpenDock()
        {
            AppDispatcher.Invoke(() => _manager.OpenDock(_key, true));
        }

        [WordNodeMember]
        public void OpenFloat()
        {
            AppDispatcher.Invoke(() => _manager.OpenWindow(_key, true));
        }

        [WordNodeMember]
        public void Close()
        {
            AppDispatcher.Invoke(() => _manager.Close(_key));
        }
    }
}
