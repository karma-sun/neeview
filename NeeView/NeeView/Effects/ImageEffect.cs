using Microsoft.Expression.Media.Effects;
using NeeLaboratory.ComponentModel;
using NeeView.Data;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    /// <summary>
    /// 画像エフェクト
    /// </summary>
    public class ImageEffect : BindableBase
    {
        static ImageEffect() => Current = new ImageEffect();
        public static ImageEffect Current { get; }

        #region Constructors

        private ImageEffect()
        {
            Effects = new Dictionary<EffectType, EffectUnit>();

            Effects[EffectType.None] = null;
            Effects[EffectType.Level] = Config.Current.ImageEffect.LevelEffect;
            Effects[EffectType.Hsv] = Config.Current.ImageEffect.HsvEffect;
            Effects[EffectType.ColorSelect] = Config.Current.ImageEffect.ColorSelectEffect;
            Effects[EffectType.Blur] = Config.Current.ImageEffect.BlurEffect;
            Effects[EffectType.Bloom] = Config.Current.ImageEffect.BloomEffect;
            Effects[EffectType.Monochrome] = Config.Current.ImageEffect.MonochromeEffect;
            Effects[EffectType.ColorTone] = Config.Current.ImageEffect.ColorToneEffect;
            Effects[EffectType.Sharpen] = Config.Current.ImageEffect.SharpenEffect;
            Effects[EffectType.Embossed] = Config.Current.ImageEffect.EmbossedEffect;
            Effects[EffectType.Pixelate] = Config.Current.ImageEffect.PixelateEffect;
            Effects[EffectType.Magnify] = Config.Current.ImageEffect.MagnifyEffect;
            Effects[EffectType.Ripple] = Config.Current.ImageEffect.RippleEffect;
            Effects[EffectType.Swirl] = Config.Current.ImageEffect.SwirlEffect;

            Config.Current.ImageEffect.AddPropertyChanged(nameof(ImageEffectConfig.IsEnabled), (s, e) =>
            {
                RaisePropertyChanged(nameof(Effect));
            });

            Config.Current.ImageEffect.AddPropertyChanged(nameof(ImageEffectConfig.EffectType), (s, e) =>
            {
                RaisePropertyChanged(nameof(Effect));
                UpdateEffectParameters();
            });

            UpdateEffectParameters();
        }

        #endregion

        #region Properties

        //
        public Dictionary<EffectType, EffectUnit> Effects { get; private set; }

        /// <summary>
        /// Property: Effect
        /// </summary>
        public Effect Effect => Config.Current.ImageEffect.IsEnabled ? Effects[Config.Current.ImageEffect.EffectType]?.GetEffect() : null;

        /// <summary>
        /// Property: EffectParameters
        /// </summary>
        private PropertyDocument _effectParameters;
        public PropertyDocument EffectParameters
        {
            get { return _effectParameters; }
            set { if (_effectParameters != value) { _effectParameters = value; RaisePropertyChanged(); } }
        }

        #endregion

        #region Methods

        //
        private void UpdateEffectParameters()
        {
            if (Effects[Config.Current.ImageEffect.EffectType] == null)
            {
                EffectParameters = null;
            }
            else
            {
                EffectParameters = new PropertyDocument(Effects[Config.Current.ImageEffect.EffectType]);
            }
        }

        #endregion

        #region Memento

        [DataContract]
        public class Memento : IMemento
        {
            [DataMember]
            public EffectType EffectType { get; set; }

            [DataMember]
            public Dictionary<EffectType, string> Effects { get; set; }

            [DataMember]
            public bool IsHsvMode { get; set; }

            [DataMember]
            public bool IsEnabled { get; set; }


            [OnDeserialized]
            private void OnDeserialized(StreamingContext context)
            {
                // 補正
                if (EffectType == EffectType.None)
                {
                    EffectType = EffectType.Level;
                }
            }

            public void RestoreConfig(Config config)
            {
                config.ImageEffect.IsEnabled = IsEnabled;
                config.ImageEffect.EffectType = EffectType;
                config.ImageEffect.IsHsvMode = IsHsvMode;

                if (Effects != null)
                {
                    MargeEffect(config.ImageEffect.LevelEffect, Effects[EffectType.Level]);
                    MargeEffect(config.ImageEffect.HsvEffect, Effects[EffectType.Hsv]);
                    MargeEffect(config.ImageEffect.ColorSelectEffect, Effects[EffectType.ColorSelect]);
                    MargeEffect(config.ImageEffect.BlurEffect, Effects[EffectType.Blur]);
                    MargeEffect(config.ImageEffect.BloomEffect, Effects[EffectType.Bloom]);
                    MargeEffect(config.ImageEffect.MonochromeEffect, Effects[EffectType.Monochrome]);
                    MargeEffect(config.ImageEffect.ColorToneEffect, Effects[EffectType.ColorTone]);
                    MargeEffect(config.ImageEffect.SharpenEffect, Effects[EffectType.Sharpen]);
                    MargeEffect(config.ImageEffect.EmbossedEffect, Effects[EffectType.Embossed]);
                    MargeEffect(config.ImageEffect.PixelateEffect, Effects[EffectType.Pixelate]);
                    MargeEffect(config.ImageEffect.MagnifyEffect, Effects[EffectType.Magnify]);
                    MargeEffect(config.ImageEffect.RippleEffect, Effects[EffectType.Ripple]);
                    MargeEffect(config.ImageEffect.SwirlEffect, Effects[EffectType.Swirl]);

                    void MargeEffect(EffectUnit unit, string json) => ObjectMerge.Merge(unit, Json.Deserialize(json, unit.GetType()));
                }
            }
        }

        #endregion
    }
}
