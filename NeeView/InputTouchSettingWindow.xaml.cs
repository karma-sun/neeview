// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.Collections.ObjectModel;
using System.Diagnostics;
using NeeView.Windows.Input;
using NeeView.ComponentModel;

namespace NeeView
{
    /// <summary>
    ///  InputToucheSettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class InputTouchSettingWindow : Window
    {
        private InputTouchSettingViewModel _vm;

        //
        public InputTouchSettingWindow(InputTouchSettingContext context)
        {
            InitializeComponent();

            this.GestureBox.PreviewMouseLeftButtonUp += GestureBox_PreviewMouseLeftButtonUp;

            _vm = new InputTouchSettingViewModel(context, this.GestureBox);
            DataContext = _vm;

            // ESCでウィンドウを閉じる
            this.InputBindings.Add(new KeyBinding(new RelayCommand(Close), new KeyGesture(Key.Escape)));
        }

        //
        private void GestureBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var width = this.GestureBox.ActualWidth;
            var pos = e.GetPosition(this.GestureBox);

            _vm.SetTouchGesture(pos, this.GestureBox.ActualWidth, this.GestureBox.ActualHeight);
        }

        //
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.Decide();

            this.DialogResult = true;
            this.Close();
        }

        //
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    /// <summary>
    /// MouseGestureSetting ViewModel
    /// </summary>
    public class InputTouchSettingViewModel : BindableBase
    {
        //
        private InputTouchSettingContext _context;

        /// <summary>
        /// Property: GestureToken
        /// </summary>
        private GestureToken _gestureToken = new GestureToken();
        public GestureToken GestureToken
        {
            get { return _gestureToken; }
            set { if (_gestureToken != value) { _gestureToken = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// Property: Original Gesture
        /// </summary>
        public string OriginalGesture { get; set; }

        /// <summary>
        /// NewGesture property.
        /// </summary>
        private string _NewGesture = "";
        public string NewGesture
        {
            get { return _NewGesture; }
            set { if (_NewGesture != value) { _NewGesture = value; RaisePropertyChanged(); } }
        }


        /// <summary>
        /// Window Title
        /// </summary>
        public string Header => _context.Header ?? _context.Command.ToDispString();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="gestureSender"></param>
        public InputTouchSettingViewModel(InputTouchSettingContext context, FrameworkElement gestureSender)
        {
            _context = context;
            OriginalGesture = _context.Gesture;
        }

        //
        internal void SetTouchGesture(Point pos, double width, double heigth)
        {
            var gesture = pos.X < width * 0.5 ? TouchGesture.TouchLeft : TouchGesture.TouchRight;

            this.NewGesture = gesture.ToString();
            UpdateGestureToken(NewGesture);
        }


        /// <summary>
        /// Update Gesture Information
        /// </summary>
        /// <param name="gesture"></param>
        public void UpdateGestureToken(string gesture)
        {
            // Check Conflict
            var token = new GestureToken();
            token.Gesture = gesture;

            if (!string.IsNullOrEmpty(token.Gesture))
            {
                token.Conflicts = _context.Gestures
                    .Where(i => i.Key != _context.Command && i.Value == token.Gesture)
                    .Select(i => i.Key)
                    .ToList();

                if (token.Conflicts.Count > 0)
                {
                    token.OverlapsText = string.Join("", token.Conflicts.Select(i => $"「{i.ToDispString()}」")) + "と競合しています";
                }
            }

            GestureToken = token;
        }


        /// <summary>
        /// 決定
        /// </summary>
        public void Decide()
        {
            _context.Gesture = NewGesture;
        }


        /// <summary>
        /// Command: ClearCommand
        /// </summary>
        private RelayCommand _clearCommand;
        public RelayCommand ClearCommand
        {
            get { return _clearCommand = _clearCommand ?? new RelayCommand(ClearCommand_Executed); }
        }

        private void ClearCommand_Executed()
        {
            this.NewGesture = "";
            UpdateGestureToken(NewGesture);
        }
    }


    /// <summary>
    /// MouseGestureSetting Model
    /// </summary>
    public class InputTouchSettingContext
    {
        /// <summary>
        /// 表示名。nullの場合はCommand名を使用する
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// 設定対象のコマンド
        /// </summary>
        public CommandType Command { get; set; }

        /// <summary>
        /// 全てのコマンドのジェスチャー。競合判定に使用する
        /// </summary>
        public Dictionary<CommandType, string> Gestures { get; set; }


        /// <summary>
        /// Property: Gesture
        /// </summary>
        public string Gesture
        {
            get { return Gestures[Command]; }
            set { if (Gestures[Command] != value) { Gestures[Command] = value; } }
        }
    }


}
