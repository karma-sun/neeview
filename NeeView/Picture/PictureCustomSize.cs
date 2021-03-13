using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows;

namespace NeeView
{
    /// <summary>
    /// 画像指定サイズ
    /// </summary>
    public class PictureCustomSize : BindableBase
    {
        #region Memento

        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public bool IsEnabled { get; set; }
            [DataMember]
            public bool IsUniformed { get; set; }
            [DataMember]
            public Size Size { get; set; }

            public void RestoreConfig(Config config)
            {
                config.ImageCustomSize.IsEnabled = IsEnabled;
                config.ImageCustomSize.AspectRatio = IsUniformed ? CustomSizeAspectRatio.Origin : CustomSizeAspectRatio.None;
                config.ImageCustomSize.Size = Size;
            }
        }

        #endregion
    }
}
