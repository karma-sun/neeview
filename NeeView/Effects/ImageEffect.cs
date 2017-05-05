// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using Microsoft.Expression.Media.Effects;
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
    //
    public class ImageEffect : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        //
        public Dictionary<EffectType, EffectUnit> Effects { get; private set; }

        /// <summary>
        /// Property: Effect
        /// </summary>
        public Effect Effect => Effects[_effectType]?.Effect;

        ///
        public bool IsRecoveryEffectType { get; set; }

        /// <summary>
        /// Property: EffectType
        /// </summary>
        private EffectType _effectType;
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
        public bool IsHsvMode
        {
            get { return _isHsvMode; }
            set { if (_isHsvMode != value) { _isHsvMode = value; RaisePropertyChanged(); } }
        }



        //
        private void UpdateEffectParameters()
        {
            if (Effects[_effectType] == null)
            {
                EffectParameters = null;
            }
            else
            {
                EffectParameters = PropertyDocument.Create(Effects[_effectType]);
            }
        }

        //
        public ImageEffect()
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

        }

        //

        [DataContract]
        public class Memento
        {
            [DataMember]
            public EffectType EffectType { get; set; }

            [DataMember]
            public bool IsRecoveryEffectType { get; set; }

            [DataMember]
            public Dictionary<EffectType, string> Effects { get; set; }

            [DataMember]
            public bool IsHsvMode { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.EffectType = this.EffectType;
            memento.IsRecoveryEffectType = this.IsRecoveryEffectType;
            memento.IsHsvMode = this.IsHsvMode;

            memento.Effects = new Dictionary<EffectType, string>();
            foreach (var effect in Effects)
            {
                if (effect.Value != null)
                {
                    memento.Effects.Add(effect.Key, Utility.Json.Serialize(effect.Value, this.Effects[effect.Key].GetType()));
                }
            }

            return memento;
        }

        /// <summary>
        /// TODO: fromLoad、IsRecovertyEffectType は このレベルでなく、さらに上位の設定のはず
        /// </summary>
        /// <param name="memento"></param>
        /// <param name="fromLoad"></param>
        public void Restore(Memento memento, bool fromLoad)
        {
            if (memento == null) return;

            this.EffectType = (fromLoad && !memento.IsRecoveryEffectType) ? EffectType.None : memento.EffectType;
            this.IsRecoveryEffectType = memento.IsRecoveryEffectType;
            this.IsHsvMode = memento.IsHsvMode;

            if (memento.Effects != null)
            {
                foreach (var effect in memento.Effects)
                {
                    if (this.Effects.ContainsKey(effect.Key))
                    {
                        this.Effects[effect.Key] = (EffectUnit)Utility.Json.Deserialize(effect.Value, this.Effects[effect.Key].GetType());
                    }
                }
            }
        }

        //
    }
}
