using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Windows.Property
{
    /// <summary>
    /// Setterメソッド装備
    /// </summary>
    public interface IValueSetter
    {
        string Name { get; }
        object GetValue();
        void SetValue(object value);
    }
}
