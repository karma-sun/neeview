namespace NeeView
{
    /// <summary>
    /// アーカイバーの種類
    /// </summary>
    public enum ArchiverType
    {
        None,

        FolderArchive,
        ZipArchiver,
        SevenZipArchiver,
        PdfArchiver,
        SusieArchiver,
        MediaArchiver,
    }

    public static class ArchiverTypeExtensions
    {
        // 多重圧縮ファイルが可能なアーカイブであるか
        public static bool IsRecursiveSupported(this ArchiverType self)
        {
            switch (self)
            {
                case ArchiverType.PdfArchiver:
                case ArchiverType.MediaArchiver:
                    return false;
                default:
                    return true;
            }
        }
    }
}

