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
                config.ImageCustomSize.IsUniformed = IsUniformed;
                config.ImageCustomSize.Size = Size;
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.IsEnabled = Config.Current.ImageCustomSize.IsEnabled;
            memento.IsUniformed = Config.Current.ImageCustomSize.IsUniformed;
            memento.Size = Config.Current.ImageCustomSize.Size;

            return memento;
        }

        #endregion
    }
}
