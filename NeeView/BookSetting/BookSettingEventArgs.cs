using System;

namespace NeeView
{
    public class BookSettingEventArgs : EventArgs
    {
        public BookSettingEventArgs()
        {
        }

        public BookSettingEventArgs(BookSettingKey key)
        {
            Key = key;
        }

        public BookSettingKey Key { get; set; }
    }
}
