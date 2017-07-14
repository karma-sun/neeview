// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// リサイズによるコンテンツの再作成管理
    /// </summary>
    public class ContentRebuild
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

        // リサイズ要求
        private bool _isRequested;

        #endregion
        
        #region Constructors

        //
        public ContentRebuild()
        {
            Current = this;
            CompositionTarget.Rendering += new EventHandler(OnRendering);

            // スケール変化に追従
            DragTransform.Current.AddPropertyChanged(nameof(DragTransform.Scale), (s, e) => Request());
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
            if (!_isRequested || _isResizingWindow) return;
            if (MouseButtonBitsExtensions.Create() != MouseButtonBits.None) return;

            bool isSuccessed = true;
            var scale = DragTransform.Current.Scale;
            foreach (var viewConent in ContentCanvas.Current.Contents.Where(e => e.IsValid))
            {
                isSuccessed = viewConent.Rebuild(scale) && isSuccessed;
            }

            _isRequested = !isSuccessed;
        }


        // リサイズ要求
        public void Request()
        {
            _isRequested = true;
        }

        #endregion
    }
}
