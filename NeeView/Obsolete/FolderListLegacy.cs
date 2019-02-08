using NeeLaboratory.ComponentModel;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace NeeView
{
    /// <summary>
    /// FolderList.Memento old version (before 33.0)
    /// </summary>
    [Obsolete]
    public static class FolderListLegacy
    {
        [DataContract]
        public class Memento
        {
            [DataMember]
            public PanelListItemStyle PanelListItemStyle { get; set; }

            [DataMember]
            public bool IsVisibleHistoryMark { get; set; }

            [DataMember]
            public bool IsVisibleBookmarkMark { get; set; }

            [DataMember]
            public string Home { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsInsertItem { get; set; }

            [DataMember]
            public bool IsMultipleRarFilterEnabled { get; set; }

            [DataMember]
            public string ExcludePattern { get; set; }

            [DataMember]
            public bool IsCruise { get; set; }

            [DataMember]
            public bool IsCloseBookWhenMove { get; set; }

            [DataMember, DefaultValue(FolderTreeLayout.Left)]
            public FolderTreeLayout FolderTreeLayout { get; set; }

            [DataMember, DefaultValue(72.0)]
            public double FolderTreeAreaHeight { get; set; }

            [DataMember, DefaultValue(192.0)]
            public double FolderTreeAreaWidth { get; set; }

            [DataMember, DefaultValue(true)]
            public bool IsFolderTreeVisible { get; set; }

            [DataMember]
            public bool IsSyncFolderTree { get; set; }

            [OnDeserializing]
            private void Deserializing(StreamingContext c)
            {
                this.InitializePropertyDefaultValues();
            }
        }

        public static BookshelfFolderList.Memento ConvertFrom(Memento memento)
        {
            var target = new BookshelfFolderList.Memento();

            target.FolderList = new FolderList.Memento();
            target.FolderList.PanelListItemStyle = memento.PanelListItemStyle;
            target.FolderList.FolderTreeLayout = memento.FolderTreeLayout;
            target.FolderList.FolderTreeAreaHeight = memento.FolderTreeAreaHeight;
            target.FolderList.FolderTreeAreaWidth = memento.FolderTreeAreaWidth;
            target.FolderList.IsFolderTreeVisible = memento.IsFolderTreeVisible;
            target.FolderList.IsSyncFolderTree = memento.IsSyncFolderTree;

            target.IsVisibleHistoryMark = memento.IsVisibleHistoryMark;
            target.IsVisibleBookmarkMark = memento.IsVisibleBookmarkMark;
            target.Home = memento.Home;
            target.IsInsertItem = memento.IsInsertItem;
            target.IsMultipleRarFilterEnabled = memento.IsMultipleRarFilterEnabled;
            target.ExcludePattern = memento.ExcludePattern;
            target.IsCruise = memento.IsCruise;
            target.IsCloseBookWhenMove = memento.IsCloseBookWhenMove;

            return target;
        }
    }

}
