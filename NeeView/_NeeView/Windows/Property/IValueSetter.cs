// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

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
        object GetValue();
        void SetValue(object value);
    }
}
