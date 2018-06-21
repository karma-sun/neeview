using NeeLaboratory.ComponentModel;
using System;
using System.Runtime.Serialization;

namespace NeeView
{
    [DataContract]
    public class QuickAccess : BindableBase, IEquatable<QuickAccess>
    {
        private string _path;

        public QuickAccess(string path)
        {
            _path = path;
        }

        [DataMember]
        public string Path
        {
            get { return _path; }
            private set { SetProperty(ref _path, value); }
        }

        public string Name
        {
            get { return new QueryPath(_path).ToDispString(); }
        }

        public string Detail
        {
            get { return new QueryPath(_path).ToDetailString(); }
        }

        public override string ToString()
        {
            return Name;
        }

        #region IEquatable Suport

        public override bool Equals(object obj)
        {
            return this.Equals(obj as QuickAccess);
        }

        public bool Equals(QuickAccess other)
        {
            if (other == null)
            {
                return false;
            }

            return _path == other._path;
        }

        public override int GetHashCode()
        {
            return _path != null ? _path.GetHashCode() : 0;
        }

        public static bool operator ==(QuickAccess lhs, QuickAccess rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                {
                    return true;
                }

                return false;
            }
            return lhs.Equals(rhs);
        }

        public static bool operator !=(QuickAccess lhs, QuickAccess rhs)
        {
            return !(lhs == rhs);
        }

        #endregion
    }


}
