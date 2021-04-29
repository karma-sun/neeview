using System;

namespace NeeView.IO
{
    [Flags]
    public enum SingleFileWaterOptions
    {
        None = 0,
        FollowRename = (1 << 0),
    }
}
