using System.Windows.Controls;
using System.Windows.Media;

namespace NeeView.Windows.Controls
{
    public interface IHasMaximizeButton
    {
        /// <summary>
        /// 最大化ボタン取得
        /// </summary>
        /// <returns>nullの場合は最大化ボタンは存在しない</returns>
        Button GetMaximizeButton();

        /// <summary>
        /// 最大化ボタン背景設定
        /// </summary>
        /// <remarks>
        /// スタイルが機能しない場合の代替手段として用意
        /// </remarks>
        /// <param name="brush"></param>
        void SetMaximizeButtonBackground(Brush brush);
    }
}
