﻿// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

// from http://stackoverflow.com/questions/11703833/dragmove-and-maximize

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// ウィンドウキャプションのマウス操作エミュレート
    /// </summary>
    public class WindowCaptionEmulator
    {
        /// <summary>
        /// 対象ウィンドウ
        /// </summary>
        private Window _window;

        /// <summary>
        /// 入力エレメント
        /// </summary>
        private FrameworkElement _target;

        /// <summary>
        /// ドラッグ状態
        /// </summary>
        private bool _isDrag;

        /// <summary>
        /// ドラッグ開始座標
        /// </summary>
        private Point _dragStartPoint;

        /// <summary>
        /// 有効フラグ
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// リサイズ不可時の有効フラグ.
        /// (フルスクリーン)
        /// </summary>
        //public bool IsEnabeldWhenNoResized { get; set; } = true;
        public bool IsEnabeldWhenNoResized => Preference.Current.window_captionemunate_fullscreen;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="window"></param>
        /// <param name="target"></param>
        public WindowCaptionEmulator(Window window, FrameworkElement target)
        {
            _window = window;
            _target = target;

            _target.MouseLeftButtonDown += Target_MouseLeftButtonDown;
            _target.MouseLeftButtonUp += Target_MouseLeftButtonUp;
            _target.MouseMove += Target_MouseMove;
        }

        /// <summary>
        /// 
        /// </summary>
        private void SwitchWindowState()
        {
            switch (_window.WindowState)
            {
                case WindowState.Normal:
                    _window.WindowState = WindowState.Maximized;
                    break;
                case WindowState.Maximized:
                    _window.WindowState = WindowState.Normal;
                    break;
            }
        }


        /// <summary>
        /// 左ボタン押した処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Target_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsEnabled) return;
            if (!IsEnabeldWhenNoResized && _window.ResizeMode == ResizeMode.NoResize) return;

            if (e.ClickCount == 2)
            {
                switch (_window.WindowState)
                {
                    case WindowState.Normal:
                        _window.WindowState = WindowState.Maximized;
                        break;
                    case WindowState.Maximized:
                        _window.WindowState = WindowState.Normal;
                        break;
                }
                return;
            }

            else if (_window.WindowState == WindowState.Maximized)
            {
                _isDrag = true;
                _dragStartPoint = e.GetPosition(_window);
                return;
            }

            _window.DragMove();
        }

        /// <summary>
        /// 左ボタン離した処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Target_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDrag = false;
        }

        /// <summary>
        /// マウス移動処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Target_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDrag) return;

            if (e.LeftButton != MouseButtonState.Pressed)
            {
                _isDrag = false;
                return;
            }

            var pos = e.GetPosition(_window);
            var dx = Math.Abs(pos.X - _dragStartPoint.X);
            var dy = Math.Abs(pos.Y - _dragStartPoint.Y);
            if (dx > SystemParameters.MinimumHorizontalDragDistance || dy > SystemParameters.MinimumVerticalDragDistance)
            {
                _isDrag = false;

                double percentHorizontal = e.GetPosition(_window).X / _window.ActualWidth;
                double targetHorizontal = _window.RestoreBounds.Width * percentHorizontal;
                
                var cursor = Windows.CursorInfo.GetNowScreenPosition();
                _window.Left = cursor.X - targetHorizontal;
                _window.Top = 0;

#if false
                // マルチモニタ検証ができていないので。
                var x = cursor.X - _window.RestoreBounds.Width * 0.5;
                if (x + _window.RestoreBounds.Width > SystemParameters.WorkArea.Width)
                {
                    x = SystemParameters.WorkArea.Width - _window.RestoreBounds.Width;
                }
                if (x < 0)
                {
                    x = 0;
                }
                _window.Left = x;
#endif

                _window.WindowStyle = WindowStyle.None; // ※瞬時に切り替わるようにするため一時的に変更。WindowShapeSelectorで修正される
                _window.WindowState = WindowState.Normal;

                if (Mouse.LeftButton == MouseButtonState.Pressed) _window.DragMove();
            }
        }
    }
}