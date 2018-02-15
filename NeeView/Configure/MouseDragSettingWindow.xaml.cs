// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeLaboratory.ComponentModel;
using NeeLaboratory.Windows.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace NeeView.Configure
{
    /// <summary>
    /// MouseDragSettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MouseDragSettingWindow : Window
    {
        private MouseDragSettingViewModel _vm;

        public MouseDragSettingWindow(MouseDragSettingContext context)
        {
            InitializeComponent();

            _vm = new MouseDragSettingViewModel(context, this.GestureBox);
            DataContext = _vm;

            // ESCでウィンドウを閉じる
            this.InputBindings.Add(new KeyBinding(new RelayCommand(Close), new KeyGesture(Key.Escape)));
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
    /// 
    /// </summary>
    public class DragToken
    {
        // ジェスチャー文字列（１ジェスチャー）
        public string Gesture { get; set; }

        // 競合しているコマンド群
        public List<DragActionType> Conflicts { get; set; }

        // 競合メッセージ
        public string OverlapsText { get; set; }

        public bool IsConflict => Conflicts != null && Conflicts.Count > 0;
    }


    /// <summary>
    /// MouseDragSetting ViewModel
    /// </summary>
    public class MouseDragSettingViewModel : BindableBase
    {
        //
        private MouseDragSettingContext _context;

        //
        //private MouseInputManagerForGestureEditor _mouseGesture;

        /// <summary>
        /// Property: DragToken
        /// </summary>
        private DragToken _dragToken = new DragToken();
        public DragToken DragToken
        {
            get { return _dragToken; }
            set { if (_dragToken != value) { _dragToken = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// Property: Original Drag
        /// </summary>
        public string OriginalDrag { get; set; }

        /// <summary>
        /// NewGesture property.
        /// </summary>
        private string _NewDrag;
        public string NewDrag
        {
            get { return _NewDrag; }
            set { if (_NewDrag != value) { _NewDrag = value; RaisePropertyChanged(); } }
        }


        /// <summary>
        /// Window Title
        /// </summary>
        public string Header => _context.Header ?? _context.Command.ToLabel();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="gestureSender"></param>
        public MouseDragSettingViewModel(MouseDragSettingContext context, FrameworkElement gestureSender)
        {
            _context = context;

            //_mouseGesture = new MouseInputManagerForGestureEditor(gestureSender);
            //_mouseGesture.Gesture.MouseGestureProgressed += Gesture_MouseGestureProgressed;

            gestureSender.MouseDown += GestureSender_MouseDown;

            OriginalDrag = _context.Gesture;
        }

        private void GestureSender_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var dragKey = new DragKey(MouseButtonBitsExtensions.Create(e), Keyboard.Modifiers);

            UpdateGestureToken(dragKey.ToString());
        }


        /*
        /// <summary>
        /// Gesture Changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Gesture_MouseGestureProgressed(object sender, MouseGestureEventArgs e)
        {
            NewDrag = e.Sequence.ToString();
            UpdateGestureToken(NewDrag);
        }
        */


        /// <summary>
        /// Update Gesture Information
        /// </summary>
        /// <param name="gesture"></param>
        public void UpdateGestureToken(string gesture)
        {
            NewDrag = gesture;

            // Check Conflict
            var token = new DragToken();
            token.Gesture = gesture;

            if (!string.IsNullOrEmpty(token.Gesture))
            {
                token.Conflicts = _context.Gestures
                    .Where(i => i.Key != _context.Command && i.Value == token.Gesture)
                    .Select(i => i.Key)
                    .ToList();

                if (token.Conflicts.Count > 0)
                {
                    token.OverlapsText = string.Join("", token.Conflicts.Select(i => $"「{i.ToLabel()}」")) + "と競合しています";
                }
            }

            DragToken = token;
        }


        /// <summary>
        /// 決定
        /// </summary>
        public void Decide()
        {
            _context.Gesture = NewDrag;
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
            _context.Gesture = null;
            //_mouseGesture.Gesture.Reset();
        }
    }


    /// <summary>
    /// MouseDragSetting Model
    /// </summary>
    public class MouseDragSettingContext
    {
        /// <summary>
        /// 表示名。nullの場合はCommand名を使用する
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// 設定対象のコマンド
        /// </summary>
        public DragActionType Command { get; set; }

        /// <summary>
        /// 全てのコマンドの操作。競合判定に使用する
        /// </summary>
        public Dictionary<DragActionType, string> Gestures { get; set; }


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
