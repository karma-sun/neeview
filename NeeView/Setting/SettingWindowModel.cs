using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using NeeView.Setting;
using NeeView.Effects;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NeeView.Setting
{
    /// <summary>
    /// 設定画面 Model
    /// </summary>
    public class SettingWindowModel : BindableBase
    {
        private static Type _latestSelectedPageType;

        private List<SettingPage> _pages;

        public SettingWindowModel()
        {
            Initialize();
        }

        public List<SettingPage> Pages
        {
            get { return _pages; }
        }

        private void Initialize()
        {
            _pages = new List<SettingPage>();

            _pages.Add(new SettingPageEnvironment());
            _pages.Add(new SettingPageArchiver());
            _pages.Add(new SettingPageVisual());
            _pages.Add(new SettingPageManipurate());
            _pages.Add(new SettingPageBook());
            _pages.Add(new SettingPageHistory());
            _pages.Add(new SettingPageExternal());
            _pages.Add(new SettingPageCommand());
            _pages.Add(new SettingPageContextMenu());

            _latestSelectedPageType = _latestSelectedPageType ?? typeof(SettingPageEnvironment);
            var page = GetSettingPagesEnumerator(_pages).FirstOrDefault(e => e.GetType() == _latestSelectedPageType);
            if (page != null)
            {
                page.IsSelected = true;
            }
        }

        public void SetSelectedPage(SettingPage page)
        {
            if (page == null) return;
            _latestSelectedPageType = page.GetType();
        }

        private IEnumerable<SettingPage> GetSettingPagesEnumerator(IEnumerable<SettingPage> pages)
        {
            if (pages == null) yield break;

            foreach (var page in pages)
            {
                yield return page;
                if (page.Children != null)
                {
                    foreach (var child in GetSettingPagesEnumerator(page.Children))
                    {
                        yield return child;
                    }
                }
            }
        }

    }
}
