using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeeView
{
    /// <summary>
    /// アニメーションコンテンツ
    /// </summary>
    public class AnimatedContent : BitmapContent
    {
        public AnimatedContent(ArchiveEntry entry) : base(entry)
        {
        }


        public override bool IsLoaded => base.IsLoaded && FileProxy != null;

        public override bool IsViewReady => IsLoaded; 

        public override bool CanResize => false;

        public bool IsAnimated => this.PictureInfo?.BitmapInfo.FrameCount > 1;


        public override IContentLoader CreateContentLoader()
        {
            return new AnimatedContentLoader(this);
        }
    }
}
