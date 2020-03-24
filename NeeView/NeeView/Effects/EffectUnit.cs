using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media.Effects;

namespace NeeView.Effects
{
    [DataContract]
    public class EffectUnit : BindableBase
    {
        public virtual Effect GetEffect() => null;

        protected void RaiseEffectPropertyChanged()
        {
            RaisePropertyChanged(nameof(Effect));
        }
    }
}
