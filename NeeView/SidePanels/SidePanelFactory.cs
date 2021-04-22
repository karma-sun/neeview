using NeeView.Effects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    static class SidePanelFactory
    {
        public static List<IPanel> CreatePanels(params string[] keys)
        {
            return keys.Select(e => Create(e)).ToList();
        }

        public static IPanel Create(string key)
        {
            switch (key)
            {
                case nameof(FolderPanel):
                    return new FolderPanel(BookshelfFolderList.Current);

                case nameof(HistoryPanel):
                    return new HistoryPanel(HistoryList.Current);

                case nameof(FileInformationPanel):
                    return new FileInformationPanel(FileInformation.Current);

                case nameof(NavigatePanel):
                    return new NavigatePanel(NavigateModel.Current);

                case nameof(ImageEffectPanel):
                    return new ImageEffectPanel(ImageEffect.Current, ImageFilter.Current);

                case nameof(BookmarkPanel):
                    return new BookmarkPanel(BookmarkFolderList.Current);

                case nameof(PagemarkPanel):
                    return new PagemarkPanel(PagemarkList.Current);

                case nameof(PageListPanel):
                    return new PageListPanel(PageList.Current);

                case nameof(PlaylistPanel):
                    return new PlaylistPanel(PlaylistModel.Current);

                default:
                    throw new NotSupportedException();
            }
        }
    }

}
