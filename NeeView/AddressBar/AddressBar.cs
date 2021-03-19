using NeeLaboratory.ComponentModel;
using NeeView.IO;
using System.Windows.Controls;

namespace NeeView
{
    /// <summary>
    /// AddressBar : Model
    /// </summary>
    public class AddressBar : BindableBase
    {
        private string _address;


        public AddressBar()
        {
            BookHub.Current.AddressChanged +=
                (s, e) => SetAddress(BookHub.Current.Address);

            BookHub.Current.BookChanged +=
                (s, e) => SetAddress(BookHub.Current.Address);

            BookHub.Current.BookmarkChanged +=
                (s, e) => RaisePropertyChanged(nameof(IsBookmark));
        }


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
            }
        }

        public bool IsBookmark
        {
            get { return BookmarkCollection.Current.Contains(Address); }
        }

        public string BookName => LoosePath.GetFileName(_address);

        public bool IsBookEnabled => BookHub.Current.Book != null;

        public string BookDetail
        {
            get
            {
                var text = BookHub.Current.Book?.Source.GetDetail();
                if (text is null)
                {
                    var query = new QueryPath(_address);
                    if (query.Scheme == QueryScheme.Bookmark)
                    {
                        text = Properties.Resources.BookAddressInfo_Bookmark;
                    }
                    else if (query.Scheme == QueryScheme.Pagemark)
                    {
                        text = Properties.Resources.BookAddressInfo_Pagemark;
                    }
                }
                return text ?? Properties.Resources.BookAddressInfo_Invalid;
            }
        }




        private void SetAddress(string address)
        {
            _address = address;
            RaisePropertyChanged(null);
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

            BookHub.Current.RequestLoad(this, path, null, option | BookLoadOption.IsBook, true);
        }
    }
}
