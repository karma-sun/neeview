using NeeLaboratory.ComponentModel;
using System.Collections;
using System.Linq;
using System.Runtime.Serialization;

namespace NeeView
{
    public class Config : BindableBase
    {
        public static Config Current { get; } = new Config();


        public SystemConfig System { get; set; } = new SystemConfig();

        public StartUpConfig StartUp { get; set; } = new StartUpConfig();

        public PerformanceConfig Performance { get; set; } = new PerformanceConfig();

        public ImageConfig Image { get; set; } = new ImageConfig();

        public ArchiveConfig Archive { get; set; } = new ArchiveConfig();

        public SusieConfig Susie { get; set; } = new SusieConfig();

        public HistoryConfig History { get; set; } = new HistoryConfig();

        public BookmarkConfig Bookmark { get; set; } = new BookmarkConfig();

        public PagemarkConfig Pagemark { get; set; } = new PagemarkConfig();

        public WindowConfig Window { get; set; } = new WindowConfig();

        public LayoutConfig Layout { get; set; } = new LayoutConfig();

        public ThumbnailConfig Thumbnail { get; set; } = new ThumbnailConfig();

        public SlideShowConfig SlideShow { get; set; } = new SlideShowConfig();

        public EffectConfig Effect { get; set; } = new EffectConfig();

        public ImageCustomSizeConfig ImageCustomSize { get; set; } = new ImageCustomSizeConfig();

        public ImageDotKeepConfig ImageDotKeep { get; set; } = new ImageDotKeepConfig();

        public ImageGridConfig ImageGrid { get; set; } = new ImageGridConfig();

        public ImageResizeFilterConfig ImageResizeFilter { get; set; } = new ImageResizeFilterConfig();

        public ViewConfig View { get; set; } = new ViewConfig();

        public MouseConfig Mouse { get; set; } = new MouseConfig();

        public TouchConfig Touch { get; set; } = new TouchConfig();

        public LoupeConfig Loupe { get; set; } = new LoupeConfig();

        public CommandConfig Command { get; set; } = new CommandConfig();

        public ScriptConfig Script { get; set; } = new ScriptConfig();
    }

}