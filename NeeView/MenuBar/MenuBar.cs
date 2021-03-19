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
        private WindowStateManager _windowStateManager;


        public MenuBar(WindowStateManager windowStateManager)
        {
            _windowStateManager = windowStateManager;

            NeeView.MainMenu.Current.AddPropertyChanged(nameof(NeeView.MainMenu.Menu),
                (s, e) => RaisePropertyChanged(nameof(MainMenu)));
        }


        public Menu MainMenu => NeeView.MainMenu.Current.Menu;

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
