// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace NeeView
{
    public class Locker
    {
        public class Key : IDisposable
        {
            public Locker Locker { get; set; }

            public Key(Locker locker)
            {
                this.Locker = locker;
            }

            public void Dispose()
            {
                this.Locker?.Unlock(this);
            }
        }

        private List<Key> _keys = new List<Key>();

        public bool IsLocked => _keys.Any();

        //
        public Key Lock()
        {
            var key = new Key(this);
            _keys.Add(key);
            return key;
        }

        //
        public void Unlock(Key key)
        {
            if (key.Locker == this)
            {
                _keys.Remove(key);
                key.Locker = null;
            }
        }
    }


    /// <summary>
    /// リサイズによるコンテンツの再作成管理
    /// </summary>
    public class ContentRebuild : BindableBase, IEngine
    {
        // system object
        public static ContentRebuild Current { get; private set; }

        #region Win32API

        // ウィンドウメッセージ
        const int WM_SIZE = 0x0005;
        const int WM_ENTERSIZEMOVE = 0x0231;
        const int WM_EXITSIZEMOVE = 0x0232;

        // Win32API の PostMessage 関数のインポート
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern bool PostMessage(IntPtr hWnd, Int32 Msg, IntPtr wParam, IntPtr lParam);


        #endregion

        #region Fields

        // ウィンドウリサイズ中かどうか
        bool _isResizingWindow;

        // コンテンツサイズ更新要求
        bool _isUpdateContentSize;

        #endregion

        #region Constructors

        //
        public ContentRebuild()
        {
            Current = this;

            // コンテンツ変更監視
            ContentCanvas.Current.ContentChanged += (s, e) => Request();

            // スケール変化に追従
            DragTransform.Current.AddPropertyChanged(nameof(DragTransform.Scale), (s, e) => Request());

            // ルーペ状態に追従
            LoupeTransform.Current.AddPropertyChanged(nameof(LoupeTransform.FixedScale), (s, e) => Request());

            // リサイズフィルター状態監視
            PictureProfile.Current.AddPropertyChanged(nameof(PictureProfile.IsResizeFilterEnabled), (s, e) => Request());
            ImageFilter.Current.PropertyChanged += (s, e) => Request();

            // ドット表示監視
            ContentCanvas.Current.AddPropertyChanged(nameof(ContentCanvas.IsEnabledNearestNeighbor), (s, e) => Request());

            // サイズ指定状態監視
            PictureProfile.Current.CustomSize.PropertyChanged += (s, e) => RequestWithResize();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Locker property.
        /// 更新を停止させるために使用する
        /// </summary>
        public Locker Locker { get; } = new Locker();

        /// <summary>
        /// IsRequested property.
        /// </summary>
        private bool _isRequested;
        public bool IsRequested
        {
            get { return _isRequested; }
            set { if (_isRequested != value) { _isRequested = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// IsBusy property.
        /// </summary>
        private bool _IsBusy;
        public bool IsBusy
        {
            get { return _IsBusy; }
            set { if (_IsBusy != value) { _IsBusy = value; RaisePropertyChanged(); } }
        }

        #endregion

        #region Medhots

        // ウィンドウプロシージャ初期化
        // ウィンドウリサイズ中を監視
        public void InitinalizeWinProc(Window window)
        {
            // ウィンドウプロシージャ監視
            var hsrc = HwndSource.FromVisual(window) as HwndSource;
            hsrc.AddHook(WndProc);
        }

        // ウィンドウプロシージャ
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_ENTERSIZEMOVE:
                    _isResizingWindow = true;
                    break;
                case WM_EXITSIZEMOVE:
                    _isResizingWindow = false;
                    break;
            }
            return IntPtr.Zero;
        }


        /// <summary>
        /// フレーム処理
        /// 必要ならば現在の表示サイズでコンテンツを再作成する
        /// </summary>
        private void OnRendering(object sender, EventArgs e)
        {
            RebuildFrame();
        }

        //
        private void RebuildFrame()
        {
            if (!_isRequested || _isResizingWindow || Locker.IsLocked) return;

            // サイズ指定による更新
            if (_isUpdateContentSize)
            {
                _isUpdateContentSize = false;
                ContentCanvas.Current.UpdateContentSize();
                DragTransformControl.Current.SnapView();
            }

            var mouseButtonBits = MouseButtonBitsExtensions.Create();
            if (MouseInput.Current.IsLoupeMode && MouseInput.Current.Normal.LongLeftButtonDownMode == LongButtonDownMode.Loupe)
            {
                mouseButtonBits = mouseButtonBits & ~MouseButtonBits.LeftButton;
            }
            if (mouseButtonBits != MouseButtonBits.None) return;

            if (MainWindowModel.Current.AnyKey.IsPressed) return;

            bool isSuccessed = true;
            var dpiScaleX = Config.Current.RawDpi.DpiScaleX;
            var scale = DragTransform.Current.Scale * LoupeTransform.Current.FixedScale * dpiScaleX;
            foreach (var viewConent in ContentCanvas.Current.Contents.Where(e => e.IsValid))
            {
                isSuccessed = viewConent.Rebuild(scale) && isSuccessed;
            }

            this.IsRequested = !isSuccessed;

            UpdateStatus();
        }


        // 更新要求
        public void Request()
        {
            this.IsRequested = true;
        }

        // リサイズ更新要求
        public void RequestWithResize()
        {
            _isUpdateContentSize = true;
            this.IsRequested = true;
        }

        //
        public void UpdateStatus()
        {
            this.IsBusy = ContentCanvas.Current.Contents.Where(e => e.IsValid).Any(e => e.IsResizing);
        }

        public void StartEngine()
        {
            CompositionTarget.Rendering += OnRendering;
        }

        public void StopEngine()
        {
            CompositionTarget.Rendering -= OnRendering;
        }

        #endregion
    }
}
