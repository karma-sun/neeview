using System;

namespace NeeView
{
    [Flags]
    public enum FolderSetPlaceOption
    {
        None,

        /// <summary>
        /// フォーカスをあわせる
        /// </summary>
        Focus = (1 << 0),

        /// <summary>
        /// フォルダー履歴更新
        /// </summary>
        UpdateHistory = (1 << 1),

        /// <summary>
        /// 先頭を選択した態にする
        /// </summary>
        TopSelect = (1 << 3),

        /// <summary>
        /// 検索キーワードをクリア
        /// TODO: 未使用に付き削除
        /// </summary>
        ResetKeyword = (1 << 4),

        /// <summary>
        /// 同じ場所でも作り直す
        /// </summary>
        Refresh = (1 << 5),

        /// <summary>
        /// ブックマークではなく、ファイルシステムの場所を優先
        /// </summary>
        FileSystem = (1 << 6),
    }
}
