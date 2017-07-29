// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace NeeView
{
    /// <summary>
    /// AnyKey Pressed
    /// </summary>
    public class AnyKey : BindableBase
    {
        private LinkedList<Key> _keys;

        /// <summary>
        /// IsPressed property.
        /// </summary>
        public bool IsPressed
        {
            get
            {
                if (_keys.Any() && _keys.All(e => Keyboard.IsKeyUp(e)))
                {
                    _keys.Clear();
                    return false;
                }
                else
                {
                    ////if (_keys.Any()) Debug.WriteLine("AnyKey: " + string.Join(",", _keys));
                    return _keys.Any();
                }
            }
        }

        //
        public AnyKey()
        {
            _keys = new LinkedList<Key>();
        }

        //
        public void KeyDown(Key key)
        {
            if (!RoutedCommandTable.Current.IsUsedKey(key)) return;

            if (_keys.Contains(key)) return;
            _keys.AddLast(key);
        }

        //
        public void KeyUp(Key key)
        {
            _keys.Remove(key);
        }
    }
}
