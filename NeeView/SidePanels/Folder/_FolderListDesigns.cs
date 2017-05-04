// デザイン用データ

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// 
    /// </summary>
    public class Thumbnail_Design
    {
        public BitmapImage BitmapSource => new BitmapImage(new Uri(@"E:\Work\test.png"));
        public bool IsUniqueImage => true;
    }

    /// <summary>
    /// 
    /// </summary>
    public class FolderItem_Design
    {
        public string Name => "ある晴れた昼下がり市場へ続く道荷馬車がゴトゴト子牛をのせてゆく";

        public bool IsDirectory => true;
        public bool IsShortcut => true;
        public bool IsOverlayChecked => true;
        public bool IsOverlayStar => false;
        public bool IsDisable => false;

        public object ArchivePage => new
        {
            LastWriteTime = DateTime.Now,
            Length = 12345678L,
            Thumbnail = new Thumbnail_Design(),
        };
    }

    /// <summary>
    /// 
    /// </summary>
    public class FolderListDataContext_Design
    {
        public PanelListItemStyle PanelListItemStyle => PanelListItemStyle.Content;
        public FolderIconLayout FolderIconLayout => FolderIconLayout.Right;

        public object FolderCollection => new
        {
            Items = new List<FolderItem_Design>()
            {
                new FolderItem_Design(),
                new FolderItem_Design(),
            },
        };
    }
}
