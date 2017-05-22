using NeeView.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public class Development : BindableBase
    {
        public static Development Current { get; private set; }

        //
        public Development()
        {
            Current = this;
        }

        /// <summary>
        /// IsVisibleDevPageList property.
        /// </summary>
        private bool _IsVisibleDevPageList;
        public bool IsVisibleDevPageList
        {
            get { return _IsVisibleDevPageList; }
            set { if (_IsVisibleDevPageList != value) { _IsVisibleDevPageList = value; RaisePropertyChanged(); } }
        }

        /// <summary>
        /// IsVisibleDevInfo property.
        /// </summary>
        private bool _IsVisibleDevInfo;
        public bool IsVisibleDevInfo
        {
            get { return _IsVisibleDevInfo; }
            set { if (_IsVisibleDevInfo != value) { _IsVisibleDevInfo = value; RaisePropertyChanged(); } }
        }

    }
}
