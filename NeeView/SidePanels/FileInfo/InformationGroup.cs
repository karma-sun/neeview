using System;

namespace NeeView
{
    public enum InformationGroup
    {
        File,
        Image,
        Description,
        Origin,
        Camera,
        AdvancedPhoto,
        Gps,
    }

    public enum InformationCategory
    {
        File,
        Image,
        Metadata,
    }

    public static class InformationGroupExtensions
    {
        public static InformationCategory ToInformationCategory(this InformationGroup self)
        {
            switch (self)
            {
                case InformationGroup.File:
                    return InformationCategory.File;
                case InformationGroup.Image:
                    return InformationCategory.Image;
                case InformationGroup.Description:
                case InformationGroup.Origin:
                case InformationGroup.Camera:
                case InformationGroup.AdvancedPhoto:
                case InformationGroup.Gps:
                    return InformationCategory.Metadata;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
