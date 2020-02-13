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
        /// IsVisibleLock変更イベント
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
