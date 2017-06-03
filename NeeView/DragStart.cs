// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Windows;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// ドラッグ開始処理
    /// より直接的なドラッグ開始処理を提供する
    /// </summary>
    public class DragStart
    {
        private bool _dragReady;
        private Point _dragOrigin;
        private FrameworkElement _dragSender;
        private DataObject _dragObject;
        private DragDropEffects _dragEffects;

        //
        public event EventHandler Dragging;
        public event EventHandler Dragged;

        //
        public void Drag_MouseDown(object sender, MouseButtonEventArgs e, DataObject dragObject, DragDropEffects dragEffects)
        {
            _dragReady = true;
            _dragOrigin = e.GetPosition(_dragSender);
            _dragObject = dragObject;
            _dragSender = sender as FrameworkElement;
            _dragEffects = dragEffects;
        }

        //
        public void Drag_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _dragReady = false;
        }

        //
        public void Drag_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_dragReady) return;

            var pos = e.GetPosition(_dragSender);
            if (Math.Abs(pos.X - _dragOrigin.X) >= SystemParameters.MinimumHorizontalDragDistance || Math.Abs(pos.Y - _dragOrigin.Y) >= SystemParameters.MinimumVerticalDragDistance)
            {
                Dragging?.Invoke(this, null);
                DragDrop.DoDragDrop(_dragSender, _dragObject, _dragEffects);
                _dragReady = false;
                Dragged?.Invoke(this, null);
            }
        }
    }
}
