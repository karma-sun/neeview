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

    public enum InformationGroupCategory
    {
        File,
        Image,
        Metadata,
    }

    public static class InformationGroupExtensions
    {
        public static InformationGroupCategory ToInformationGroupCategory(this InformationGroup self)
        {
            switch (self)
            {
                case InformationGroup.File:
                    return InformationGroupCategory.File;
                case InformationGroup.Image:
                    return InformationGroupCategory.Image;
                case InformationGroup.Description:
                case InformationGroup.Origin:
                case InformationGroup.Camera:
                case InformationGroup.AdvancedPhoto:
                case InformationGroup.Gps:
                    return InformationGroupCategory.Metadata;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
