using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public enum QueryScheme
    {
        [AliasName("PC")]
        File = 0,

        [AliasName("@WordBookmark")]
        Bookmark,

        [AliasName("@WordPagemark")]
        Pagemark,

        [AliasName("@WordQuickAccess")]
        QuickAccess,
    }

    public static class QuerySchemeExtensions
    {
        static readonly Dictionary<QueryScheme, string> _map = new Dictionary<QueryScheme, string>()
        {
            [QueryScheme.File] = "file:",
            [QueryScheme.Bookmark] = "bookmark:",
            [QueryScheme.Pagemark] = "pagemark:",
            [QueryScheme.QuickAccess] = "quickaccess:",
        };

        public static string ToSchemeString(this QueryScheme scheme)
        {
            return _map[scheme];
        }

        public static QueryScheme GetScheme(string path)
        {
            return _map.FirstOrDefault(e => path.StartsWith(e.Value)).Key;
        }
    }

    /// <summary>
    /// パスのクエリパラメータを分解する
    /// </summary>
    public class QueryPath
    {
        static readonly string _querySearch = "?search=";

        public QueryPath(string source)
        {
            var rest = source;
            rest = TakeQuerySearch(rest, out _search);
            rest = TakeScheme(rest, out _scheme);
            _path = GetValidatePath(rest, _scheme);
        }

        public QueryPath(string source, string search)
        {
            var rest = source;
            _search = string.IsNullOrWhiteSpace(search) ? null : search;
            rest = TakeScheme(rest, out _scheme);
            _path = GetValidatePath(rest, _scheme);
        }

        public QueryPath(QueryScheme scheme, string path, string search)
        {
            _search = string.IsNullOrWhiteSpace(search) ? null : search;
            _scheme = scheme;
            _path = GetValidatePath(path, _scheme);
        }


        private QueryScheme _scheme;
        public QueryScheme Scheme
        {
            get { return _scheme; }
            private set { _scheme = value; }
        }

        private string _path;
        public string Path
        {
            get { return _path; }
            private set { _path = value; }
        }

        private string _search;
        public string Search
        {
            get { return _search; }
            private set { _search = value; }
        }


        /// <summary>
        /// 完全パス
        /// </summary>
        public string FulPath => _scheme.ToSchemeString() + '\\' + _path;

        /// <summary>
        /// 簡略化したパス
        /// </summary>
        public string SimplePath => _scheme == QueryScheme.File ? _path : FulPath;

        /// <summary>
        /// 完全クエリ
        /// </summary>
        public string FullQuery => FulPath + (_search != null ? _querySearch + _search : null);

        /// <summary>
        /// 簡略化したクエリ
        /// </summary>
        public string SimpleQuery => SimplePath + (_search != null ? _querySearch + _search : null);


        private string TakeQuerySearch(string source, out string searchWord)
        {
            if (source != null)
            {
                var index = source.IndexOf(_querySearch);
                if (index >= 0)
                {
                    searchWord = source.Substring(index + _querySearch.Length);
                    return source.Substring(0, index);
                }
            }

            searchWord = null;
            return source;
        }

        private string TakeScheme(string source, out QueryScheme scheme)
        {
            if (source != null)
            {
                scheme = QuerySchemeExtensions.GetScheme(source);
                var schemeString = scheme.ToSchemeString();
                if (source.StartsWith(schemeString))
                {
                    var length = schemeString.Length;
                    if (length < source.Length && (source[length] == '\\' || source[length] == '/'))
                    {
                        length++;
                    }
                    return source.Substring(length);
                }
            }
            else
            {
                scheme = QueryScheme.File;
            }

            return source;
        }

        private string GetValidatePath(string source, QueryScheme scheme)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return null;
            }

            var s = LoosePath.NormalizeSeparator(source).Trim().TrimEnd('\\');

            if (scheme == QueryScheme.File)
            {
                // is drive
                if (s.Length == 2 && s[1] == ':')
                {
                    return char.ToUpper(s[0]) + ":\\";
                }
            }

            return s;
        }

        public override string ToString()
        {
            return FullQuery;
        }

    }
}
