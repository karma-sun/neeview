// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// Preference accessor
    /// </summary>
    public class PreferenceAccessor : BindableBase
    {
        /// <summary>
        /// system object
        /// </summary>
        private static PreferenceAccessor _current;
        public static PreferenceAccessor Current
        {
            get
            {
                _current = _current ?? new PreferenceAccessor(Preference.Current);
                return _current;
            }
        }
        

        /// <summary>
        /// Reflesh
        /// </summary>
        public void Reflesh()
        {
            RaisePropertyChanged(null);
        }


        /// <summary>
        /// preference
        /// </summary>
        private Preference _preference;

        /// <summary>
        /// constuctor
        /// </summary>
        /// <param name="preference"></param>
        public PreferenceAccessor(Preference preference)
        {
            _preference = preference;
        }

        /// <summary>
        /// FilePermitCommand property.
        /// </summary>
        public bool FilePermitCommand
        {
            get { return _preference.file_permit_command; }
            set { if (_preference.file_permit_command != value) { _preference.file_permit_command = value; RaisePropertyChanged(); } }
        }
    }
}
