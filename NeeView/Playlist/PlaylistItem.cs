using System.Text.Json.Serialization;

namespace NeeView
{
    public class PlaylistItem
    {
        private string _name;

        public PlaylistItem()
        {
        }

        public PlaylistItem(string path)
        {
            Path = path;
        }

        public PlaylistItem(string path, string name)
        {
            Path = path;
            Name = name;
        }

        public string Path { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string Name
        {
            get { return _name; }
            set { _name = (string.IsNullOrEmpty(value) || value == LoosePath.GetFileName(Path)) ? null : value; }
        }
    }

}
