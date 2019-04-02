namespace NeeView
{
    // ページサムネイル寿命管理
    public class PageThumbnailPool : ThumbnailPool
    {
        public override int Limit => ThumbnailProfile.Current.PageCapacity;
    }
}
