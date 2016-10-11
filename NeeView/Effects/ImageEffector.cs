// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using Microsoft.Expression.Media.Effects;
using NeeLaboratory.Property;
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
    public class ImageEffector : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = "")
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

        /// <summary>
        /// Property: EffectType
        /// </summary>
        private EffectType _effectType;
        public EffectType EffectType
        {
            get { return _effectType; }
            set { if (_effectType != value) { _effectType = value; OnPropertyChanged(); OnPropertyChanged(nameof(Effect)); UpdateEffectParameters(); } }
        }

        /// <summary>
        /// Property: EffectParameters
        /// </summary>
        private PropertyDocument _effectParameters;
        public PropertyDocument EffectParameters
        {
            get { return _effectParameters; }
            set { if (_effectParameters != value) { _effectParameters = value; OnPropertyChanged(); } }
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
        public ImageEffector()
        {
            Effects = new Dictionary<EffectType, EffectUnit>();

            Effects[EffectType.None] = null;
            Effects[EffectType.Blur] = new BlurEffectUnit();
            Effects[EffectType.Bloom] = new BloomEffectUnit();
            Effects[EffectType.Monochrome] = new MonochromeEffectUnit();
            Effects[EffectType.ColorTone] = new ColorToneEffectUnit();
            Effects[EffectType.Embossed] = new EmbossedEffectUnit();
            Effects[EffectType.Pixelate] = new PixelateEffectUnit();
            Effects[EffectType.Sharpen] = new SharpenEffectUnit();
            Effects[EffectType.Magnify] = new MagnifyEffectUnit();
            Effects[EffectType.Ripple] = new RippleEffectUnit();
            Effects[EffectType.Swirl] = new SwirlEffectUnit();

            //Effects[EffectType.MyGrayscale] = new GrayscaleEffectUnit();
        }

        //

        [DataContract]
        public class Memento
        {
            [DataMember]
            public EffectType EffectType { get; set; }

            [DataMember]
            public Dictionary<EffectType, string> Effects { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();

            memento.EffectType = this.EffectType;

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

        //
        public void Restore(Memento memento)
        {
            this.EffectType = memento.EffectType;

            if (memento.Effects != null)
            {
                foreach (var effect in memento.Effects)
                {
                    this.Effects[effect.Key] = (EffectUnit)Utility.Json.Deserialize(effect.Value, this.Effects[effect.Key].GetType());
                }
            }
        }


        //
    }
}
