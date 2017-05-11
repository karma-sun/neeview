// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public class InfoMessage
    {
        // System Object
        public static InfoMessage Current { get; private set; }

        //
        public InfoMessage()
        {
            Current = this;
        }

        //
        public NormalInfoMessage NormalInfoMessage { get; } = new NormalInfoMessage();

        //
        public TinyInfoMessage TinyInfoMessage { get; } = new TinyInfoMessage();

        //
        public void SetMessage(ShowMessageStyle style, string message, string tinyMessage = null, double dispTime = 1.0, BookMementoType bookmarkType = BookMementoType.None)
        {
            switch (style)
            {
                case ShowMessageStyle.Normal:
                    this.NormalInfoMessage.SetMessage(message, dispTime, bookmarkType);
                    break;
                case ShowMessageStyle.Tiny:
                    this.TinyInfoMessage.SetMessage(tinyMessage ?? message);
                    break;
            }
        }
    }
}
