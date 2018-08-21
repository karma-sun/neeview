namespace NeeView
{
    /// <summary>
    /// 通常画像をストリームで取得
    /// </summary>
    class DefaultPictureStream : IPictureStream
    {
        public NamedStream Create(ArchiveEntry entry)
        {
            if (!entry.IsIgnoreFileExtension && !PictureProfile.Current.IsDefaultSupported(entry.EntryName)) return null;

            return new NamedStream(entry.OpenEntry(), null);
        }
    }
}
