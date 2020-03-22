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
            Effects[EffectType.Level] = new LevelEffectUnit();
            Effects[EffectType.Hsv] = new HsvEffectUnit();
            Effects[EffectType.ColorSelect] = new ColorSelectEffectUnit();
            Effects[EffectType.Blur] = new BlurEffectUnit();
            Effects[EffectType.Bloom] = new BloomEffectUnit();
            Effects[EffectType.Monochrome] = new MonochromeEffectUnit();
            Effects[EffectType.ColorTone] = new ColorToneEffectUnit();
            Effects[EffectType.Sharpen] = new SharpenEffectUnit();
            Effects[EffectType.Embossed] = new EmbossedEffectUnit();
            Effects[EffectType.Pixelate] = new PixelateEffectUnit();
            Effects[EffectType.Magnify] = new MagnifyEffectUnit();
            Effects[EffectType.Ripple] = new RippleEffectUnit();
            Effects[EffectType.Swirl] = new SwirlEffectUnit();

            UpdateEffectParameters();
        }

        #endregion

        #region Properties

        //
        public Dictionary<EffectType, EffectUnit> Effects { get; private set; }

        /// <summary>
        /// Property: Effect
        /// </summary>
        public Effect Effect => this.IsEnabled ? Effects[_effectType]?.Effect : null;

        /// <summary>
        /// Property: EffectType
        /// </summary>
        private EffectType _effectType = EffectType.Level;
        public EffectType EffectType
        {
            get { return _effectType; }
            set { if (_effectType != value) { _effectType = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(Effect)); UpdateEffectParameters(); } }
        }

        /// <summary>
        /// Property: EffectParameters
        /// </summary>
        private PropertyDocument _effectParameters;
        public PropertyDocument EffectParameters
        {
            get { return _effectParameters; }
            set { if (_effectParameters != value) { _effectParameters = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// Property: IsHsvMode
        /// </summary>
        private bool _isHsvMode;
        [PropertyMember("@ParamImageEffectIsHsvMode", Tips = "@ParamImageEffectIsHsvModeTips")]
        public bool IsHsvMode
        {
            get { return _isHsvMode; }
            set { if (_isHsvMode != value) { _isHsvMode = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// IsEnabled property.
        /// </summary>
        private bool _isEnabled;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { if (_isEnabled != value) { _isEnabled = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(Effect)); } }
        }

        #endregion

        #region Methods

        //
        private void UpdateEffectParameters()
        {
            if (Effects[_effectType] == null)
            {
                EffectParameters = null;
            }
            else
            {
                EffectParameters = new PropertyDocument(Effects[_effectType]);
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
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.EffectType = this.EffectType;
            memento.IsHsvMode = this.IsHsvMode;
            memento.IsEnabled = this.IsEnabled;

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

        public void Restore(Memento memento)
        {
            if (memento == null) return;

            this.EffectType = memento.EffectType;
            this.IsHsvMode = memento.IsHsvMode;
            this.IsEnabled = memento.IsEnabled;

            if (memento.Effects != null)
            {
                foreach (var effect in memento.Effects)
                {
                    if (this.Effects.ContainsKey(effect.Key))
                    {
                        this.Effects[effect.Key] = (EffectUnit)Json.Deserialize(effect.Value, this.Effects[effect.Key].GetType());
                    }
                }
            }
        }
        #endregion
    }
}
