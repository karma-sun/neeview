// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System.Collections.Generic;

namespace NeeView
{
    /// <summary>
    /// コマンドの種類
    /// </summary>
    public enum CommandType
    {
        None, // 

        //OpenContextMenu,

        OpenSettingWindow,
        OpenSettingFilesFolder,
        OpenVersionWindow,
        CloseApplication,

        LoadAs,
        ReLoad,
        Unload,
        OpenApplication,
        OpenFilePlace,
        Export,
        Print,
        DeleteFile,
        CopyFile,
        CopyImage,
        Paste,
        OpenContextMenu,

        ClearHistory,

        PrevPage,
        NextPage,
        PrevOnePage,
        NextOnePage,
        PrevScrollPage,
        NextScrollPage,
        MovePageWithCursor,

        PrevSizePage,
        NextSizePage,
        FirstPage,
        LastPage,

        PrevFolder,
        NextFolder,

        PrevHistory,
        NextHistory,

        ToggleFolderOrder,
        SetFolderOrderByFileName,
        SetFolderOrderByTimeStamp,
        SetFolderOrderBySize,
        SetFolderOrderByRandom,

        ToggleTopmost,
        ToggleHideMenu,
        ToggleHidePageSlider,
        ToggleHidePanel,
        ToggleHideTitleBar, // 欠番
        ToggleVisibleTitleBar,
        ToggleVisibleAddressBar,
        ToggleVisibleSideBar,
        ToggleVisibleFileInfo,
        ToggleVisibleEffectInfo,
        ToggleVisibleFolderList,
        ToggleVisibleBookmarkList,
        ToggleVisiblePagemarkList,
        ToggleVisibleHistoryList,
        ToggleVisiblePageList,
        TogglePanelStyle, // 欠番
        TogglePageListStyle, // 欠番

        ToggleVisibleThumbnailList,
        ToggleHideThumbnailList,

        ToggleFullScreen,
        SetFullScreen,
        CancelFullScreen,
        ToggleWindowMinimize,
        ToggleWindowMaximize,

        ShowHiddenPanels,

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
        SetBackgroundCustom,

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

        SetDefaultPageSetting,

        Bookmark, // 欠番
        ToggleBookmark,
        PrevBookmark,
        NextBookmark,

        TogglePagemark,
        PrevPagemark,
        NextPagemark,
        PrevPagemarkInBook,
        NextPagemarkInBook,

        ToggleIsReverseSort, // 欠番

        ViewScrollUp,
        ViewScrollDown,
        ViewScrollLeft,
        ViewScrollRight,
        ViewScaleUp,
        ViewScaleDown,
        ViewRotateLeft,
        ViewRotateRight,
        ToggleIsAutoRotate,
        ToggleViewFlipHorizontal,
        ViewFlipHorizontalOn,
        ViewFlipHorizontalOff,
        ToggleViewFlipVertical,
        ViewFlipVerticalOn,
        ViewFlipVerticalOff,
        ViewReset,

        ToggleEffectGrayscale, // 欠番

        ToggleIsLoupe,
        LoupeOn,
        LoupeOff,

        //LoupeZoomIn,
        //LoupeZoomOut,

        TogglePermitFileCommand,

        HelpOnline,
        HelpCommandList,
        HelpMainMenu,

        ExportBackup,
        ImportBackup
    }

    public static class CommandTypeExtensions
    {
        // 無効なコマンドID
        public static List<CommandType> IgnoreCommandTypes = new List<CommandType>()
        {
            CommandType.Bookmark,
            CommandType.ToggleIsReverseSort,
            CommandType.ToggleHideTitleBar,
            CommandType.ToggleEffectGrayscale,
            CommandType.TogglePanelStyle,
            CommandType.TogglePageListStyle,
        };

        // TODO: 判定法整備
        public static bool IsDisable(this CommandType type)
        {
            return (type == CommandType.None || IgnoreCommandTypes.Contains(type));
        }

        public static string ToDispString(this CommandType type)
        {
            return CommandTable.Current[type].Text;
        }

        public static string ToDispLongString(this CommandType type)
        {
            var command = CommandTable.Current[type];
            return command.Group + "/" + command.Text;
        }

        public static string ToMenuString(this CommandType type)
        {
            return CommandTable.Current[type].MenuText;
        }
    }
}
