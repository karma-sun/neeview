// Copyright (c) 2016-2018 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeLaboratory.ComponentModel;
using NeeView.IO;

namespace NeeView
{
    /// <summary>
    /// AddressBar : Model
    /// </summary>
    public class AddressBar : BindableBase
    {
        public static AddressBar Current { get; private set; }

        /// <summary>
        /// constructor
        /// </summary>
        public AddressBar()
        {
            Current = this;

            BookHub.Current.AddressChanged +=
                (s, e) => SetAddress(BookHub.Current.Address);

            BookHub.Current.BookChanged +=
                (s, e) => RaisePropertyChanged(nameof(IsBookmark));

            BookHub.Current.BookmarkChanged +=
                (s, e) => RaisePropertyChanged(nameof(IsBookmark));
        }

        //
        private string _address;
        public string Address
        {
            get { return _address; }
            set
            {
                if (string.IsNullOrWhiteSpace(value)) return;

                if (_address != value)
                {
                    SetAddress(value);
                    if (_address != BookHub.Current.Address)
                    {
                        Load(value);
                    }
                }
                RaisePropertyChanged(nameof(IsBookmark));
            }
        }

        private void SetAddress(string address)
        {
            _address = address;
            RaisePropertyChanged(nameof(Address));
        }

        //
        public bool IsBookmark
        {
            get { return BookMementoCollection.Current.Find(Address)?.BookmarkNode != null; }
        }


        // フォルダー読み込み
        // TODO: BookHubへ？
        public void Load(string path, BookLoadOption option = BookLoadOption.None)
        {
            if (FileShortcut.IsShortcut(path) && (System.IO.File.Exists(path) || System.IO.Directory.Exists(path)))
            {
                var shortcut = new FileShortcut(path);
                path = shortcut.TargetPath;
            }

            BookHub.Current.RequestLoad(path, null, option | BookLoadOption.IsBook, true);
        }
    }
}
