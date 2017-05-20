using NeeView.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    // TODO: MouseDragとの関係を整備。こちらをモデルとするように。
    public class ContentCanvasTransform : BindableBase
    {
        // system object.
        public static ContentCanvasTransform Current { get; private set; }


        // View変換情報表示スタイル
        public ShowMessageStyle ViewTransformShowMessageStyle { get; set; }

        // View変換情報表示のスケール表示をオリジナルサイズ基準にする
        public bool IsOriginalScaleShowMessage { get; set; }

        // 移動制限モード
        public bool IsLimitMove { get; set; } = true;

        // 回転単位
        public double AngleFrequency { get; set; }

        // 回転、拡縮をコンテンツの中心基準にする
        public bool IsControlCenterImage { get; set; }

        // 拡大率キープ
        public bool IsKeepScale { get; set; }

        // 回転キープ
        public bool IsKeepAngle { get; set; }

        // 反転キープ
        public bool IsKeepFlip { get; set; }

        // 表示開始時の基準
        public bool IsViewStartPositionCenter { get; set; }


        /// <summary>
        /// ContentAngle property.
        /// </summary>
        private double _contentAngle;
        public double ContentAngle
        {
            get { return _contentAngle; }
            set { if (_contentAngle != value) { _contentAngle = value; RaisePropertyChanged(); } }
        }


        // ##
        // TODO: 応急処置
        // MouseInputDrag との関係の整備は後で。
        private MouseInputDrag _drag;
        public void SetMouseInputDrag(MouseInputDrag drag)
        {
            _drag = drag;
            _drag.TransformChanged += (s, e) => TransformChanged?.Invoke(s, e);
            //_drag.TransformChanged += TransformChanged;
        }

        // 角度、スケール変更イベント
        public event EventHandler<TransformEventArgs> TransformChanged;

        //
        public ContentCanvasTransform()
        {
            Current = this;
        }



        // ドラッグでビュー操作設定の更新
        // TODO: MouseDragとのモデル整備
        public void SetMouseDragSetting(int direction, DragViewOrigin origin, PageReadOrder order)
        {
            if (_drag == null) return;

            _drag.IsLimitMove = this.IsLimitMove;
            _drag.DragControlCenter = this.IsControlCenterImage ? DragControlCenter.Target : DragControlCenter.View;
            _drag.AngleFrequency = this.AngleFrequency;

            if (origin == DragViewOrigin.None)
            {
                origin = this.IsViewStartPositionCenter
                    ? DragViewOrigin.Center
                    ////: _VM.BookSetting.BookReadOrder == PageReadOrder.LeftToRight
                    : order == PageReadOrder.LeftToRight
                        ? DragViewOrigin.LeftTop
                        : DragViewOrigin.RightTop;

                _drag.ViewOrigin = direction < 0 ? origin.Reverse() : origin;
                _drag.ViewHorizontalDirection = (origin == DragViewOrigin.LeftTop) ? 1.0 : -1.0;
            }
            else
            {
                _drag.ViewOrigin = direction < 0 ? origin.Reverse() : origin;
                _drag.ViewHorizontalDirection = (origin == DragViewOrigin.LeftTop || origin == DragViewOrigin.LeftBottom) ? 1.0 : -1.0;
            }
        }

        /// <summary>
        /// トランスフォーム初期化
        /// </summary>
        /// <param name="forceReset">すべての項目を初期化</param>
        /// <param name="angle">Nanでない場合はこの角度で初期化する</param>
        public void Reset(bool forceReset, double angle)
        {
            bool isResetScale = forceReset || !this.IsKeepScale;
            bool isResetAngle = forceReset || !this.IsKeepAngle || !double.IsNaN(angle);
            bool isResetFlip = forceReset || !this.IsKeepFlip;

            _drag?.Reset(isResetScale, isResetAngle, isResetFlip, double.IsNaN(angle) ? 0.0 : angle); // DefaultViewAngle(isResetAngle));
        }




        // TODO: MouseDragとのデータ整備

        // ビュー回転
        public double Angle => _drag.Angle;

        // ビュースケール
        public double ViewScale => _drag.Scale;

        //
        public double LoupeScale => MouseInputManager.Current.Loupe.LoupeScale;

        //
        public double FinalScale => _drag.Scale * LoupeScale;


        // ビュー反転
        public bool IsFlipHorizontal => _drag.IsFlipHorizontal;
        public bool IsFlipVertical => _drag.IsFlipVertical;


        // メッセージとして状態表示
        public void ShowMessage(TransformActionType ActionType, ViewContent mainContent)
        {
            if (ViewTransformShowMessageStyle == ShowMessageStyle.None) return;

            var infoMessage = InfoMessage.Current;

            switch (ActionType)
            {
                case TransformActionType.Scale:
                    string scaleText = IsOriginalScaleShowMessage && mainContent.IsValid
                        ? $"{(int)(ViewScale * mainContent.Scale * App.Config.Dpi.DpiScaleX * 100 + 0.1)}%"
                        : $"{(int)(ViewScale * 100.0 + 0.1)}%";
                    infoMessage.SetMessage(ViewTransformShowMessageStyle, scaleText);
                    break;
                case TransformActionType.Angle:
                    infoMessage.SetMessage(ViewTransformShowMessageStyle, $"{(int)(Angle)}°");
                    break;
                case TransformActionType.FlipHorizontal:
                    infoMessage.SetMessage(ViewTransformShowMessageStyle, "左右反転 " + (IsFlipHorizontal ? "ON" : "OFF"));
                    break;
                case TransformActionType.FlipVertical:
                    infoMessage.SetMessage(ViewTransformShowMessageStyle, "上下反転 " + (IsFlipVertical ? "ON" : "OFF"));
                    break;
                case TransformActionType.LoupeScale:
                    if (LoupeScale != 1.0)
                    {
                        infoMessage.SetMessage(ViewTransformShowMessageStyle, $"×{LoupeScale:0.0}");
                    }
                    break;
            }
        }



        #region Memento

        [DataContract]
        public class Memento
        {
            [DataMember]
            public ShowMessageStyle ViewTransformShowMessageStyle { get; set; }
            [DataMember]
            public bool IsOriginalScaleShowMessage { get; set; }
            [DataMember]
            public bool IsLimitMove { get; set; }
            [DataMember]
            public double AngleFrequency { get; set; }
            [DataMember]
            public bool IsControlCenterImage { get; set; }
            [DataMember]
            public bool IsKeepScale { get; set; }
            [DataMember]
            public bool IsKeepAngle { get; set; }
            [DataMember]
            public bool IsKeepFlip { get; set; }
            [DataMember]
            public bool IsViewStartPositionCenter { get; set; }
        }


        //
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.ViewTransformShowMessageStyle = this.ViewTransformShowMessageStyle;
            memento.IsOriginalScaleShowMessage = this.IsOriginalScaleShowMessage;
            memento.IsLimitMove = this.IsLimitMove;
            memento.AngleFrequency = this.AngleFrequency;
            memento.IsControlCenterImage = this.IsControlCenterImage;
            memento.IsKeepScale = this.IsKeepScale;
            memento.IsKeepAngle = this.IsKeepAngle;
            memento.IsKeepFlip = this.IsKeepFlip;
            memento.IsViewStartPositionCenter = this.IsViewStartPositionCenter;

            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.ViewTransformShowMessageStyle = memento.ViewTransformShowMessageStyle;
            this.IsOriginalScaleShowMessage = memento.IsOriginalScaleShowMessage;
            this.IsLimitMove = memento.IsLimitMove;
            this.AngleFrequency = memento.AngleFrequency;
            this.IsControlCenterImage = memento.IsControlCenterImage;
            this.IsKeepScale = memento.IsKeepScale;
            this.IsKeepAngle = memento.IsKeepAngle;
            this.IsKeepFlip = memento.IsKeepFlip;
            this.IsViewStartPositionCenter = memento.IsViewStartPositionCenter;
        }

        #endregion
    }
}