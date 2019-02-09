using NeeLaboratory.ComponentModel;
using NeeView.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace NeeView
{
    public interface IBookmarkEntry : IHasName
    {
    }


    [DataContract]
    public class Bookmark : BindableBase, IBookmarkEntry
    {
        private string _place;

        public Bookmark()
        {
        }

        public Bookmark(BookMementoUnit unit)
        {
            Place = unit.Place;
            Unit = unit;
        }

        [DataMember]
        public string Place
        {
            get { return _place; }
            set
            {
                if (SetProperty(ref _place, value))
                {
                    _unit = null;
                    RaisePropertyChanged(null);
                }
            }
        }

        [DataMember(EmitDefaultValue = false)]
        public DateTime EntryTime { get; set; }

        public string Name => Unit.Memento.Name;

        private BookMementoUnit _unit;
        public BookMementoUnit Unit
        {
            get { return _unit = _unit ?? BookMementoCollection.Current.Set(Place); }
            private set { _unit = value; }
        }

        public bool IsEqual(IBookmarkEntry entry)
        {
            return entry is Bookmark bookmark && this.Name == bookmark.Name && this.Place == bookmark.Place;
        }

        public override string ToString()
        {
            return base.ToString() + " Name:" + Name;
        }

    }

}
