// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public class MainWindowModel : BindableBase
    {
        public static MainWindowModel Current { get; private set; }

        public Models Models { get; private set; }

        /// <summary>
        /// constructor
        /// </summary>
        public MainWindowModel()
        {
            Current = this;

            // Models
            this.Models = new Models();
        }

        // TODO:

    }
}
