using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using PhotoSauce.MagicScaler;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NeeView
{
    public enum ResizeInterpolation
    {
        NearestNeighbor,
        Average,
        Linear,
        Quadratic,
        Hermite,
        Mitchell,
        CatmullRom,
        Cubic,
        CubicSmoother,
        Lanczos, // default.
        Spline36,
    }

    public static class ResizeInterpolationExtensions
    {
        public static List<ResizeInterpolation> ResizeInterpolationList { get; } =
            Enum.GetValues(typeof(ResizeInterpolation)).Cast<ResizeInterpolation>().Where(e => e != ResizeInterpolation.NearestNeighbor).ToList();
    }



    /// <summary>
    /// 画像フィルター
    /// </summary>
    public class ImageFilter : BindableBase
    {
        static ImageFilter() => Current = new ImageFilter();
        public static ImageFilter Current { get; }

        #region Memento

        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public ResizeInterpolation ResizeInterpolation { get; set; }
            [DataMember]
            public bool Sharpen { get; set; }
            [DataMember]
            public UnsharpMaskConfig UnsharpMaskProfile { get; set; }

            public void RestoreConfig(Config config)
            {
                config.ImageResizeFilter.ResizeInterpolation = ResizeInterpolation;
                config.ImageResizeFilter.Sharpen = Sharpen;

                config.ImageResizeFilter.UnsharpMask.Amount = UnsharpMaskProfile.Amount;
                config.ImageResizeFilter.UnsharpMask.Radius = UnsharpMaskProfile.Radius;
                config.ImageResizeFilter.UnsharpMask.Threshold = UnsharpMaskProfile.Threshold;
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.ResizeInterpolation = Config.Current.ImageResizeFilter.ResizeInterpolation;
            memento.Sharpen = Config.Current.ImageResizeFilter.Sharpen;
            memento.UnsharpMaskProfile = (UnsharpMaskConfig)Config.Current.ImageResizeFilter.UnsharpMask.Clone();

            return memento;
        }

        [Obsolete]
        public void Restore(Memento memento)
        {
        }

        #endregion
    }
}
