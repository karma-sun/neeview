// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

namespace NeeView
{
    /// <summary>
    /// コマンドの種類
    /// </summary>
    public enum CommandType
    {
        None, // 

        OpenSettingWindow,
        OpenVersionWindow,
        CloseApplication,

        LoadAs,
        ReLoad,
        OpenApplication,
        OpenFilePlace,
        Export,
        DeleteFile,
        CopyFile,
        CopyImage,
        Paste,

        ClearHistory,

        PrevPage,
        NextPage,
        PrevOnePage,
        NextOnePage,
        PrevScrollPage,
        NextScrollPage,
        MovePageWithCursor,

        FirstPage,
        LastPage,

        PrevFolder,
        NextFolder,

        PrevHistory,
        NextHistory,

        ToggleFolderOrder,
        SetFolderOrderByFileName,
        SetFolderOrderByTimeStamp,
        SetFolderOrderByRandom,

        ToggleTopmost,
        ToggleHideMenu,
        ToggleHidePanel,
        ToggleHideTitleBar, // 欠番
        ToggleVisibleTitleBar,
        ToggleVisibleAddressBar,
        ToggleVisibleFileInfo,
        ToggleVisibleFolderList,
        ToggleVisibleBookmarkList,
        ToggleVisibleHistoryList,
        ToggleVisiblePageList,

        ToggleVisibleThumbnailList,
        ToggleHideThumbnailList,

        ToggleFullScreen,
        SetFullScreen,
        CancelFullScreen,

        ToggleSlideShow,

        ToggleStretchMode,
        ToggleStretchModeReverse,
        SetStretchModeNone,
        SetStretchModeInside,
        SetStretchModeOutside,
        SetStretchModeUniform,
        SetStretchModeUniformToFill,
        SetStretchModeUniformToSize,
        SetStretchModeUniformToVertical,

        ToggleIsEnabledNearestNeighbor,

        ToggleBackground,
        SetBackgroundBlack,
        SetBackgroundWhite,
        SetBackgroundAuto,
        SetBackgroundCheck,

        TogglePageMode,
        SetPageMode1,
        SetPageMode2,

        ToggleBookReadOrder,
        SetBookReadOrderRight,
        SetBookReadOrderLeft,

        ToggleIsSupportedDividePage,
        ToggleIsSupportedWidePage,
        ToggleIsSupportedSingleFirstPage,
        ToggleIsSupportedSingleLastPage,

        ToggleIsRecursiveFolder,

        ToggleSortMode,
        SetSortModeFileName,
        SetSortModeFileNameDescending,
        SetSortModeTimeStamp,
        SetSortModeTimeStampDescending,
        SetSortModeRandom,

        Bookmark,
        ToggleBookmark,

        ToggleIsReverseSort, // 欠番

        ViewScrollUp,
        ViewScrollDown,
        ViewScaleUp,
        ViewScaleDown,
        ViewRotateLeft,
        ViewRotateRight,
        ToggleViewFlipHorizontal,
        ViewFlipHorizontalOn,
        ViewFlipHorizontalOff,
        ToggleViewFlipVertical,
        ViewFlipVerticalOn,
        ViewFlipVerticalOff,
        ViewReset,

        //SetEffectNone,
        //SetEffectGrayscale,
        ToggleEffectGrayscale,

        HelpOnline,
        HelpCommandList,
        HelpMainMenu,
    }

    public static class CommandTypeExtensions
    {
        // TODO: 判定法整備
        public static bool IsDisable(this CommandType type)
        {
            return (type == CommandType.None || type == CommandType.ToggleIsReverseSort || type == CommandType.ToggleHideTitleBar);
        }

        public static string ToDispString(this CommandType type)
        {
            return ModelContext.CommandTable[type].Text;
        }

        public static string ToDispLongString(this CommandType type)
        {
            var command = ModelContext.CommandTable[type];
            return command.Group + "/" + command.Text;
        }

        public static string ToMenuString(this CommandType type)
        {
            return ModelContext.CommandTable[type].MenuText;
        }
    }
}
