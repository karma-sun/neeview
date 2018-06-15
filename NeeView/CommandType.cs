using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace NeeView
{
    /// <summary>
    /// コマンドの種類
    /// </summary>
    [DataContract]
    public enum CommandType
    {
        [EnumMember]
        None, // 

        [EnumMember]
        OpenSettingWindow,
        [EnumMember]
        OpenSettingFilesFolder,
        [EnumMember]
        OpenVersionWindow,
        [EnumMember]
        CloseApplication,

        [EnumMember]
        LoadAs,
        [EnumMember]
        ReLoad,
        [EnumMember]
        Unload,
        [EnumMember]
        OpenApplication,
        [EnumMember]
        OpenFilePlace,
        [EnumMember]
        Export,
        [EnumMember]
        Print,
        [EnumMember]
        DeleteFile,
        [EnumMember]
        DeleteBook,
        [EnumMember]
        CopyFile,
        [EnumMember]
        CopyImage,
        [EnumMember]
        Paste,
        [EnumMember]
        OpenContextMenu,

        [EnumMember]
        ClearHistory,
        [EnumMember]
        ClearHistoryInPlace,

        [EnumMember]
        PrevPage,
        [EnumMember]
        NextPage,
        [EnumMember]
        PrevOnePage,
        [EnumMember]
        NextOnePage,
        [EnumMember]
        PrevScrollPage,
        [EnumMember]
        NextScrollPage,
        [Obsolete, EnumMember]
        MovePageWithCursor,

        [EnumMember]
        PrevSizePage,
        [EnumMember]
        NextSizePage,
        [EnumMember]
        FirstPage,
        [EnumMember]
        LastPage,

        [EnumMember]
        ToggleMediaPlay,

        [EnumMember]
        PrevFolder,
        [EnumMember]
        NextFolder,

        [EnumMember]
        PrevHistory,
        [EnumMember]
        NextHistory,

        [EnumMember]
        ToggleFolderOrder,
        
        [EnumMember(Value = "SetFolderOrderByFileName")]
        SetFolderOrderByFileNameA,
        [EnumMember]
        SetFolderOrderByFileNameD,
        [EnumMember]
        SetFolderOrderByTimeStampA,
        [EnumMember(Value = "SetFolderOrderByTimeStamp")]
        SetFolderOrderByTimeStampD,
        [EnumMember]
        SetFolderOrderBySizeA,
        [EnumMember(Value = "SetFolderOrderBySize")]
        SetFolderOrderBySizeD,
        [EnumMember]
        SetFolderOrderByRandom,

        [EnumMember]
        ToggleTopmost,
        [EnumMember]
        ToggleHideMenu,
        [EnumMember]
        ToggleHidePageSlider,
        [EnumMember]
        ToggleHidePanel,
        [Obsolete, EnumMember]
        ToggleHideTitleBar, // 欠番
        [EnumMember]
        ToggleVisibleTitleBar,
        [EnumMember]
        ToggleVisibleAddressBar,
        [EnumMember]
        ToggleVisibleSideBar,
        [EnumMember]
        ToggleVisibleFileInfo,
        [EnumMember]
        ToggleVisibleEffectInfo,
        [EnumMember]
        ToggleVisibleFolderList,
        [EnumMember]
        ToggleVisibleBookmarkList,
        [EnumMember]
        ToggleVisiblePagemarkList,
        [EnumMember]
        ToggleVisibleHistoryList,
        [EnumMember]
        ToggleVisiblePageList,
        [EnumMember]
        ToggleVisibleFolderSearchBox,

        [Obsolete,EnumMember]
        TogglePanelStyle, // 欠番
        [Obsolete,EnumMember]
        TogglePageListStyle, // 欠番

        [EnumMember]
        ToggleVisibleThumbnailList,
        [EnumMember]
        ToggleHideThumbnailList,

        [EnumMember]
        ToggleFullScreen,
        [EnumMember]
        SetFullScreen,
        [EnumMember]
        CancelFullScreen,
        [EnumMember]
        ToggleWindowMinimize,
        [EnumMember]
        ToggleWindowMaximize,

        [EnumMember]
        ShowHiddenPanels,

        [EnumMember]
        ToggleSlideShow,

        [EnumMember]
        ToggleStretchMode,
        [EnumMember]
        ToggleStretchModeReverse,
        [EnumMember]
        SetStretchModeNone,
        [EnumMember]
        SetStretchModeInside,
        [EnumMember]
        SetStretchModeOutside,
        [EnumMember]
        SetStretchModeUniform,
        [EnumMember]
        SetStretchModeUniformToFill,
        [EnumMember]
        SetStretchModeUniformToSize,
        [EnumMember]
        SetStretchModeUniformToVertical,

        [EnumMember]
        ToggleIsEnabledNearestNeighbor,

        [EnumMember]
        ToggleBackground,
        [EnumMember]
        SetBackgroundBlack,
        [EnumMember]
        SetBackgroundWhite,
        [EnumMember]
        SetBackgroundAuto,
        [EnumMember]
        SetBackgroundCheck,
        [EnumMember]
        SetBackgroundCustom,

        [EnumMember]
        TogglePageMode,
        [EnumMember]
        SetPageMode1,
        [EnumMember]
        SetPageMode2,

        [EnumMember]
        ToggleBookReadOrder,
        [EnumMember]
        SetBookReadOrderRight,
        [EnumMember]
        SetBookReadOrderLeft,

        [EnumMember]
        ToggleIsSupportedDividePage,
        [EnumMember]
        ToggleIsSupportedWidePage,
        [EnumMember]
        ToggleIsSupportedSingleFirstPage,
        [EnumMember]
        ToggleIsSupportedSingleLastPage,

        [EnumMember]
        ToggleIsRecursiveFolder,

        [EnumMember]
        ToggleSortMode,
        [EnumMember]
        SetSortModeFileName,
        [EnumMember]
        SetSortModeFileNameDescending,
        [EnumMember]
        SetSortModeTimeStamp,
        [EnumMember]
        SetSortModeTimeStampDescending,
        [EnumMember]
        SetSortModeSize,
        [EnumMember]
        SetSortModeSizeDescending,
        [EnumMember]
        SetSortModeRandom,

        [EnumMember]
        SetDefaultPageSetting,

        [Obsolete, EnumMember]
        Bookmark, // 欠番

        [EnumMember]
        ToggleBookmark,
        [EnumMember]
        PrevBookmark,
        [EnumMember]
        NextBookmark,

        [EnumMember]
        TogglePagemark,
        [EnumMember]
        PrevPagemark,
        [EnumMember]
        NextPagemark,
        [EnumMember]
        PrevPagemarkInBook,
        [EnumMember]
        NextPagemarkInBook,

        [Obsolete,EnumMember]
        ToggleIsReverseSort, // 欠番

        [EnumMember]
        ViewScrollUp,
        [EnumMember]
        ViewScrollDown,
        [EnumMember]
        ViewScrollLeft,
        [EnumMember]
        ViewScrollRight,
        [EnumMember]
        ViewScaleUp,
        [EnumMember]
        ViewScaleDown,
        [EnumMember]
        ViewRotateLeft,
        [EnumMember]
        ViewRotateRight,
        [EnumMember]
        ToggleIsAutoRotate,
        [EnumMember]
        ToggleViewFlipHorizontal,
        [EnumMember]
        ViewFlipHorizontalOn,
        [EnumMember]
        ViewFlipHorizontalOff,
        [EnumMember]
        ToggleViewFlipVertical,
        [EnumMember]
        ViewFlipVerticalOn,
        [EnumMember]
        ViewFlipVerticalOff,
        [EnumMember]
        ViewReset,

        [Obsolete,EnumMember]
        ToggleEffectGrayscale, // 欠番

        [EnumMember]
        ToggleCustomSize,

        [EnumMember]
        ToggleResizeFilter,
        [EnumMember]
        ToggleGrid,
        [EnumMember]
        ToggleEffect,

        [EnumMember]
        ToggleIsLoupe,
        [EnumMember]
        LoupeOn,
        [EnumMember]
        LoupeOff,
        [EnumMember]
        LoupeScaleUp,
        [EnumMember]
        LoupeScaleDown,

        [EnumMember]
        TogglePermitFileCommand,

        [EnumMember]
        HelpOnline,
        [EnumMember]
        HelpCommandList,
        [EnumMember]
        HelpMainMenu,

        [EnumMember]
        ExportBackup,
        [EnumMember]
        ImportBackup,

        [EnumMember]
        TouchEmulate,
    }

    public static class CommandTypeExtensions
    {
#pragma warning disable CS0612

        // 無効なコマンドID
        public static List<CommandType> IgnoreCommandTypes = new List<CommandType>()
        {
            CommandType.Bookmark,
            CommandType.ToggleIsReverseSort,
            CommandType.ToggleHideTitleBar,
            CommandType.ToggleEffectGrayscale,
            CommandType.TogglePanelStyle,
            CommandType.TogglePageListStyle,
            CommandType.MovePageWithCursor,
        };

#pragma warning restore CS0612


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
