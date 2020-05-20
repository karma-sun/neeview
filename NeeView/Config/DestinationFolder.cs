using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeeView
{
    public class DestinationFolder : ICloneable, IEquatable<DestinationFolder>
    {
        private string _name = "";
        private string _path = "";

        public DestinationFolder()
        {
        }

        public DestinationFolder(string name, string path)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            if (path == null) throw new ArgumentNullException(nameof(path));

            Name = name;
            Path = path;
        }


        public string Name
        {
            get { return string.IsNullOrWhiteSpace(_name) ? LoosePath.GetFileName(_path) : _name; }
            set { _name = value; }
        }

        public string Path
        {
            get => _path;
            set => _path = value;
        }


        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(_path);
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public bool Equals(DestinationFolder other)
        {
            if (other == null)
                return false;

            if (this.Name == other.Name && this.Path == other.Path)
                return true;
            else
                return false;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DestinationFolder);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Path.GetHashCode();
        }
    }
}