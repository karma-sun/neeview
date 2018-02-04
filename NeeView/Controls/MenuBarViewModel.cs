// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeLaboratory.ComponentModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// MenuBar : ViewModel
    /// </summary>
    public class MenuBarViewModel : BindableBase
    {
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="model"></param>
        public MenuBarViewModel(FrameworkElement control, MenuBar model)
        {
            _model = model;
            _model.CommandGestureChanged += (s, e) => MainMenu?.UpdateInputGestureText();

            MainMenuInitialize();

            InitializeWindowCaptionEmulator(control);
        }


        /// <summary>
        /// Model property.
        /// </summary>
        public MenuBar Model
        {
            get { return _model; }
            set { if (_model != value) { _model = value; RaisePropertyChanged(); } }
        }

        private MenuBar _model;

        //
        public Window Window { get; private set; }


        //
        public Menu MainMenu
        {
            get { return _mainMenu; }
            set { _mainMenu = value; RaisePropertyChanged(); }
        }

        private Menu _mainMenu;

        //
        public void MainMenuInitialize()
        {
            this.MainMenu = CreateMainMenu(new SolidColorBrush(Color.FromRgb(0x22, 0x22, 0x22)));

            BindingOperations.SetBinding(MainMenu, Menu.BackgroundProperty, new Binding("Background") { ElementName = "MainMenuJoint" });
            BindingOperations.SetBinding(MainMenu, Menu.ForegroundProperty, new Binding("Foreground") { ElementName = "MainMenuJoint" });
        }

        //
        private Menu CreateMainMenu(Brush foreground)
        {
            var menu = _model.MainMenuSource.CreateMenu();

            foreach(MenuItem item in menu.Items)
            {
                item.Foreground = foreground;
            }

            return menu;
        }


        //
        public Dictionary<CommandType, RoutedUICommand> BookCommands => RoutedCommandTable.Current.Commands;

        //
        public Development Development => Development.Current;



        /// <summary>
        /// WindowCaptionEmulator property.
        /// </summary>
        public WindowCaptionEmulator WindowCaptionEmulator
        {
            get { return _windowCaptionEmulator; }
            set { if (_windowCaptionEmulator != value) { _windowCaptionEmulator = value; RaisePropertyChanged(); } }
        }

        private WindowCaptionEmulator _windowCaptionEmulator;

        private void InitializeWindowCaptionEmulator(FrameworkElement control)
        {
            this.Window = System.Windows.Window.GetWindow(control);

            // window caption emulatr
            this.WindowCaptionEmulator = new WindowCaptionEmulator(Window, control);
            this.WindowCaptionEmulator.IsEnabled = !WindowShape.Current.IsCaptionVisible || WindowShape.Current.IsFullScreen;

            // IsCaptionVisible か IsFullScreen の変更を監視すべきだが、処理が軽いためプロパティ名の判定をしない
            WindowShape.Current.PropertyChanged +=
                (s, e) => this.WindowCaptionEmulator.IsEnabled = !WindowShape.Current.IsCaptionVisible || WindowShape.Current.IsFullScreen;
        }
    }


}
