namespace NeeView
{
    /// <summary>
    /// パスのクエリパラメータを分解する
    /// </summary>
    public class QueryPath
    {
        const string _query = "?search=";


        public QueryPath(string source)
        {
            if (source == null)
            {
                return;
            }

            var index = source.IndexOf(_query);
            if (index >= 0)
            {
                Path = source.Substring(0, index);
                Search = source.Substring(index + _query.Length);
            }
            else
            {
                Path = source;
                Search = null;
            }
        }

        public QueryPath(string path, string search)
        {
            Path = path;
            Search = string.IsNullOrWhiteSpace(search) ? null : search;
        }


        public string Path { get; private set; }
        public string Search { get; private set; }

        public string FullPath => Search == null ? Path : Path + _query + Search;


        public override string ToString()
        {
            return FullPath;
        }

        public string ToDispString()
        {
            if (Search != null)
            {
                return LoosePath.GetDispName(Path) + " (" + Search + ")";
            }
            else
            {
                return LoosePath.GetDispName(Path);
            }
        }

        public string ToDetailString()
        {
            if (Search != null)
            {
                return Path + "\n" + Properties.Resources.WordSearchWord + ": " + Search;
            }
            else
            {
                return Path;
            }
        }
    }

}
