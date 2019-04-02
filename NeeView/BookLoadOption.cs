using System;

namespace NeeView
{
    /// <summary>
    /// ロードオプションフラグ
    /// </summary>
    [Flags]
    public enum BookLoadOption
    {
        None = 0,

        /// <summary>
        /// 再帰 s
        /// </summary>
        Recursive = (1 << 0),

        // (1 << 1),

        /// <summary>
        /// 初期ページを先頭ページにする b^
        /// </summary>
        FirstPage = (1 << 2),

        /// <summary>
        /// 初期ページを最終ページにする b^+s
        /// </summary>
        LastPage = (1 << 3),

        /// <summary>
        /// 再読み込みフラグ(BookHubで使用) h
        /// </summary>
        ReLoad = (1 << 4),

        /// <summary>
        /// 履歴の順番を変更しない h^
        /// </summary>
        KeepHistoryOrder = (1 << 5), 

        /// <summary>
        /// 可能ならばフォルダーリストで選択する h, noused?
        /// </summary>
        SelectFoderListMaybe = (1 << 6),

        /// <summary>
        /// 可能ならば履歴リストで選択する h^
        /// </summary>
        SelectHistoryMaybe = (1 << 7),

        /// <summary>
        /// 同じフォルダーならば読み込まない h^
        /// </summary>
        SkipSamePlace = (1 << 8),

        /// <summary>
        /// 自動再帰 s^
        /// </summary>
        AutoRecursive = (1 << 9),

        /// <summary>
        /// 履歴情報から全て復元 h^
        /// </summary>
        Resume = (1 << 10),

        /// <summary>
        /// 再帰、ただし履歴が優先 s?^
        /// </summary>
        DefaultRecursive = (1 << 11),

        // (1 <<12)

        /// <summary>
        /// 再帰しない s
        /// </summary>
        NotRecursive = (1 << 13),

        /// <summary>
        /// このアドレスはブックです h^
        /// </summary>
        IsBook = (1 << 14),

        /// <summary>
        /// このアドレスはページです h^
        /// </summary>
        IsPage = (1 << 15),
    };


    public static class BookLoadOptionHelper
    {
        /// <summary>
        /// 設定を加味した再帰フラグを取得
        /// </summary>
        /// <param name="isRecursived">デフォルト値</param>
        /// <param name="setting">設定</param>
        public static bool IsRecursiveFolder(bool isRecursived, BookLoadSetting setting)
        {
            if (setting.Options.HasFlag(BookLoadOption.NotRecursive))
            {
                return false;
            }
            else if (setting.Options.HasFlag(BookLoadOption.Recursive))
            {
                return true;
            }
            else
            {
                return isRecursived;
            }
        }
    }

    /// <summary>
    /// BookLoad 設定
    /// </summary>
    public class BookLoadSetting
    {
        public BookLoadOption Options { get; set; }
        public BookPageCollectMode BookPageCollectMode { get; set; } = BookPageCollectMode.ImageAndBook;

        public BookLoadSetting Clone()
        {
            return (BookLoadSetting)MemberwiseClone();
        }
    }
}
