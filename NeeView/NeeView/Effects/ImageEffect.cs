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
            Effects[EffectType.Level] = Config.Current.Effect.ImageEffects.Level;
            Effects[EffectType.Hsv] = Config.Current.Effect.ImageEffects.Hsv;
            Effects[EffectType.ColorSelect] = Config.Current.Effect.ImageEffects.ColorSelect;
            Effects[EffectType.Blur] = Config.Current.Effect.ImageEffects.Blur;
            Effects[EffectType.Bloom] = Config.Current.Effect.ImageEffects.Bloom;
            Effects[EffectType.Monochrome] = Config.Current.Effect.ImageEffects.Monochrome;
            Effects[EffectType.ColorTone] = Config.Current.Effect.ImageEffects.ColorTone;
            Effects[EffectType.Sharpen] = Config.Current.Effect.ImageEffects.Sharpen;
            Effects[EffectType.Embossed] = Config.Current.Effect.ImageEffects.Embossed;
            Effects[EffectType.Pixelate] = Config.Current.Effect.ImageEffects.Pixelate;
            Effects[EffectType.Magnify] = Config.Current.Effect.ImageEffects.Magnify;
            Effects[EffectType.Ripple] = Config.Current.Effect.ImageEffects.Ripple;
            Effects[EffectType.Swirl] = Config.Current.Effect.ImageEffects.Swirl;

            Config.Current.Effect.AddPropertyChanged(nameof(EffectConfig.IsEnabled), (s, e) =>
            {
                RaisePropertyChanged(nameof(Effect));
            });

            Config.Current.Effect.AddPropertyChanged(nameof(EffectConfig.EffectType), (s, e) =>
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
        public Effect Effect => Config.Current.Effect.IsEnabled ? Effects[Config.Current.Effect.EffectType]?.GetEffect() : null;

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
            if (Effects[Config.Current.Effect.EffectType] == null)
            {
                EffectParameters = null;
            }
            else
            {
                EffectParameters = new PropertyDocument(Effects[Config.Current.Effect.EffectType]);
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
                config.Effect.IsEnabled = IsEnabled;
                config.Effect.EffectType = EffectType;
                config.Effect.IsHsvMode = IsHsvMode;

                if (Effects != null)
                {
                    MargeEffect(config.Effect.ImageEffects.Level, Effects[EffectType.Level]);
                    MargeEffect(config.Effect.ImageEffects.Hsv, Effects[EffectType.Hsv]);
                    MargeEffect(config.Effect.ImageEffects.ColorSelect, Effects[EffectType.ColorSelect]);
                    MargeEffect(config.Effect.ImageEffects.Blur, Effects[EffectType.Blur]);
                    MargeEffect(config.Effect.ImageEffects.Bloom, Effects[EffectType.Bloom]);
                    MargeEffect(config.Effect.ImageEffects.Monochrome, Effects[EffectType.Monochrome]);
                    MargeEffect(config.Effect.ImageEffects.ColorTone, Effects[EffectType.ColorTone]);
                    MargeEffect(config.Effect.ImageEffects.Sharpen, Effects[EffectType.Sharpen]);
                    MargeEffect(config.Effect.ImageEffects.Embossed, Effects[EffectType.Embossed]);
                    MargeEffect(config.Effect.ImageEffects.Pixelate, Effects[EffectType.Pixelate]);
                    MargeEffect(config.Effect.ImageEffects.Magnify, Effects[EffectType.Magnify]);
                    MargeEffect(config.Effect.ImageEffects.Ripple, Effects[EffectType.Ripple]);
                    MargeEffect(config.Effect.ImageEffects.Swirl, Effects[EffectType.Swirl]);

                    void MargeEffect(EffectUnit unit, string json) => ObjectMerge.Merge(unit, Json.Deserialize(json, unit.GetType()));
                }
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.EffectType = Config.Current.Effect.EffectType;
            memento.IsHsvMode = Config.Current.Effect.IsHsvMode;
            memento.IsEnabled = Config.Current.Effect.IsEnabled;

            memento.Effects = new Dictionary<EffectType, string>();
            foreach (var effect in Effects)
            {
                if (effect.Value != null)
                {
                    memento.Effects.Add(effect.Key, Json.Serialize(effect.Value, this.Effects[effect.Key].GetType()));
                }
            }

            return memento;
        }

        #endregion
    }
}
