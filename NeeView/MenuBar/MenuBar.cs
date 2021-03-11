using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// MenuBar : Model
    /// </summary>
    public class MenuBar : BindableBase
    {
        private MainMenuSelector _mainMenuSelector;
        private WindowStateManager _windowStateManager;


        public MenuBar(MainMenuSelector mainMenuSelector, WindowStateManager windowStateManager)
        {
            _mainMenuSelector = mainMenuSelector;
            _windowStateManager = windowStateManager;

            _mainMenuSelector.AddPropertyChanged(nameof(MainMenuSelector.MenuBarMenu),
                (s, e) => RaisePropertyChanged(nameof(MainMenu)));
        }


        public Menu MainMenu => _mainMenuSelector.MenuBarMenu;

        public WindowStateManager WindowStateManager => _windowStateManager;



        #region Memento
        [DataContract]
        public class Memento : IMemento
        {
            [DataMember, DefaultValue(false)]
            public bool CaptionEmulateInFullScreen { get; set; }

            [DataMember]
            public bool IsHamburgerMenu { get; set; }

            public void RestoreConfig(Config config)
            {
                config.Window.IsCaptionEmulateInFullScreen = CaptionEmulateInFullScreen;
                config.MenuBar.IsHamburgerMenu = IsHamburgerMenu;
            }
        }
        #endregion
    }
}
