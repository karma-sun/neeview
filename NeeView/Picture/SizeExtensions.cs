// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System.Windows;

namespace NeeView
{
    public static class SizeExtensions
    {
        public static Size ToInteger(this Size self)
        {
            return self.IsEmpty ? self : new Size((int)self.Width, (int)self.Height);
        }
    }

}
