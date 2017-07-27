// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php


namespace NeeView
{
    /// <summary>
    /// 通常画像をストリームで取得
    /// </summary>
    class DefaultPictureStream : IPictureStream
    {
        public NamedStream Create(ArchiveEntry entry)
        {
            return new NamedStream(entry.OpenEntry(), null);
        }
    }
}
