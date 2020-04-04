using System;
using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    public enum PanelPlace
    {
        Left,
        Right,
    }

    /// <summary>
    /// パネル定義
    /// </summary>
    public interface IPanel
    {
        /// <summary>
        /// IsSelected変更イベント
        /// </summary>
        event EventHandler IsSelectedChanged;

        /// <summary>
        /// VisibleLock変更イベント
        /// </summary>
        event EventHandler IsVisibleLockChanged;

        /// <summary>
        /// 識別名
        /// </summary>
        string TypeCode { get; }

        /// <summary>
        /// アイコン
        /// </summary>
        ImageSource Icon { get; }

        /// <summary>
        /// アイコンレイアウト
        /// </summary>
        Thickness IconMargin { get; }

        /// <summary>
        /// アイコン説明
        /// </summary>
        string IconTips { get; }

        /// <summary>
        /// パネル実体
        /// </summary>
        FrameworkElement View { get; }

        /// <summary>
        /// パネルが選択されているか
        /// </summary>
        bool IsSelected { get; set; }

        /// <summary>
        /// 表示状態フラグ
        /// </summary>
        bool IsVisible { get; set; }

        /// <summary>
        /// 表示固定フラグ
        /// </summary>
        bool IsVisibleLock { get; }

        /// <summary>
        /// 標準パネル位置
        /// </summary>
        PanelPlace DefaultPlace { get; }


        /// <summary>
        /// 表示更新
        /// </summary>
        void Refresh();

        /// <summary>
        /// パネルにフォーカスを移す
        /// </summary>
        void Focus();
    }
}
