using System;

namespace NeeView
{
    // BookMementoCollectionChangedイベントの種類
    public enum BookMementoCollectionChangedType
    {
        Load,
        Clear,
        Add,
        Update,
        Remove,
    }

    // BookMementoCollectionChangedイベントの引数
    public class BookMementoCollectionChangedArgs : EventArgs
    {
        public BookMementoCollectionChangedType HistoryChangedType { get; set; }
        public string Key { get; set; }

        public BookMementoCollectionChangedArgs(BookMementoCollectionChangedType type, string key)
        {
            HistoryChangedType = type;
            Key = key;
        }
    }
}
