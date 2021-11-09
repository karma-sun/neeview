using NeeView.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace NeeView
{
    public enum QueryScheme
    {
        [AliasName]
        File = 0,

        [AliasName]
        Root,

        [AliasName]
        Bookmark,

        [AliasName]
        QuickAccess,
    }

    public static class QuerySchemeExtensions
    {
        static readonly Dictionary<QueryScheme, string> _map = new Dictionary<QueryScheme, string>()
        {
            [QueryScheme.File] = "file:",
            [QueryScheme.Root] = "root:",
            [QueryScheme.Bookmark] = "bookmark:",
            [QueryScheme.QuickAccess] = "quickaccess:",
        };

        static Dictionary<QueryScheme, ImageSource> _imageMap;

        static Dictionary<QueryScheme, ImageSource> _thumbnailImageMap;


        private static void InitializeImageMap()
        {
            if (_imageMap != null) return;

            _imageMap = new Dictionary<QueryScheme, ImageSource>()
            {
                [QueryScheme.File] = MainWindow.Current.Resources["ic_desktop_windows_24px"] as ImageSource,
                [QueryScheme.Root] = MainWindow.Current.Resources["ic_bookshelf"] as ImageSource,
                [QueryScheme.Bookmark] = MainWindow.Current.Resources["ic_grade_24px"] as ImageSource,
                [QueryScheme.QuickAccess] = MainWindow.Current.Resources["ic_lightning"] as ImageSource,
            };

            _thumbnailImageMap = new Dictionary<QueryScheme, ImageSource>()
            {
                [QueryScheme.File] = MainWindow.Current.Resources["ic_desktop_windows_24px_t"] as ImageSource,
                [QueryScheme.Root] = MainWindow.Current.Resources["ic_bookshelf"] as ImageSource,
                [QueryScheme.Bookmark] = MainWindow.Current.Resources["ic_grade_24px_t"] as ImageSource,
                [QueryScheme.QuickAccess] = MainWindow.Current.Resources["ic_lightning"] as ImageSource,
            };

        }

        public static string ToSchemeString(this QueryScheme scheme)
        {
            return _map[scheme];
        }

        public static QueryScheme GetScheme(string path)
        {
            return _map.FirstOrDefault(e => path.StartsWith(e.Value)).Key;
        }

        public static ImageSource ToImage(this QueryScheme scheme)
        {
            InitializeImageMap();
            return _imageMap[scheme];
        }

        public static ImageSource ToThumbnailImage(this QueryScheme scheme)
        {
            InitializeImageMap();
            return _thumbnailImageMap[scheme];
        }

        public static bool IsMatch(this QueryScheme scheme, string path)
        {
            return path.StartsWith(scheme.ToSchemeString());
        }
    }

    /// <summary>
    /// パスのクエリパラメータを分解する.
    /// immutable.
    /// </summary>
    [Serializable]
    public sealed class QueryPath : IEquatable<QueryPath>
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

        public QueryPath(QueryScheme scheme, string path)
        {
            _search = null;
            _scheme = scheme;
            _path = GetValidatePath(path, _scheme);
        }

        public QueryPath(QueryScheme scheme)
        {
            _search = null;
            _scheme = scheme;
            _path = null;
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

        public bool IsEmpty => _path is null;


        /// <summary>
        /// 完全クエリ
        /// </summary>
        public string FullQuery => FullPath + (_search != null ? _querySearch + _search : null);

        /// <summary>
        /// 簡略化したクエリ
        /// </summary>
        public string SimpleQuery => SimplePath + (_search != null ? _querySearch + _search : null);


        /// <summary>
        /// 完全パス
        /// </summary>
        public string FullPath => _scheme.ToSchemeString() + '\\' + _path;

        /// <summary>
        /// 簡略化したパス
        /// </summary>
        public string SimplePath => _scheme == QueryScheme.File ? _path : FullPath;


        public string FileName => LoosePath.GetFileName(_path);

        public string DispName => (_path == null) ? _scheme.ToAliasName() : FileName;

        public string DispPath => (_path == null) ? _scheme.ToAliasName() : SimplePath;

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

            var s = LoosePath.NormalizeSeparator(source).Trim(LoosePath.AsciiSpaces).TrimEnd('\\');

            if (scheme == QueryScheme.File)
            {
                // is drive
                if (s.Length == 2 && s[1] == ':')
                {
                    return char.ToUpper(s[0]) + ":\\";
                }
            }

            return string.IsNullOrWhiteSpace(s) ? null : s;
        }

        public QueryPath ReplacePath(string path)
        {
            var query = (QueryPath)this.MemberwiseClone();
            query.Path = string.IsNullOrWhiteSpace(path) ? null : path;
            return query;
        }

        public QueryPath ReplaceSearch(string search)
        {
            var query = (QueryPath)this.MemberwiseClone();
            query.Search = string.IsNullOrWhiteSpace(search) ? null : search;
            return query;
        }

        public QueryPath GetParent()
        {
            if (_path == null)
            {
                return null;
            }

            var parent = LoosePath.GetDirectoryName(_path);
            return new QueryPath(this.Scheme, parent, null);
        }

        public bool Include(QueryPath target)
        {
            var pathX = this.FullPath;
            var pathY = target.FullPath;

            var lengthX = pathX.Length;
            var lengthY = pathY.Length;

            if (lengthX > lengthY)
            {
                return false;
            }
            else if (lengthX == lengthY)
            {
                return pathX == pathY;
            }
            else
            {
                return pathY.StartsWith(pathX) && pathY[lengthX] == '\\';
            }
        }

        public bool IsRoot(QueryScheme scheme)
        {
            return Scheme == scheme && Path == null && Search == null;
        }

        public override string ToString()
        {
            return FullQuery;
        }

        #region IEquatable Support

        public override int GetHashCode()
        {
            return _scheme.GetHashCode() ^ (_path == null ? 0 : _path.GetHashCode()) ^ (_search == null ? 0 : _search.GetHashCode());
        }

        public bool Equals(QueryPath obj)
        {
            if (obj is null)
            {
                return false;
            }

            return _scheme == obj._scheme && _path == obj._path && _search == obj._search;
        }

        public override bool Equals(object obj)
        {
            if (obj is null || this.GetType() != obj.GetType())
            {
                return false;
            }

            return this.Equals((QueryPath)obj);
        }


        public static bool Equals(QueryPath a, QueryPath b)
        {
            if ((object)a == (object)b)
            {
                return true;
            }

            if ((object)a == null || (object)b == null)
            {
                return false;
            }

            return a.Equals(b);
        }

        // HACK: 等号の再定義はあまりよろしくない。
        public static bool operator ==(QueryPath x, QueryPath y)
        {
            return Equals(x, y);
        }

        public static bool operator !=(QueryPath x, QueryPath y)
        {
            return !(Equals(x, y));
        }

        #endregion IEquatable Support
    }


    public static class QueryPathExtensions
    {
        /// <summary>
        /// ショートカットならば実体のパスに変換する
        /// </summary>
        public static QueryPath ToEntityPath(this QueryPath source)
        {
            if (source.Scheme == QueryScheme.File && FileShortcut.IsShortcut(source.SimplePath))
            {
                var shortcut = new FileShortcut(source.SimplePath);
                if (shortcut.IsValid)
                {
                    return new QueryPath(shortcut.TargetPath);
                }
            }

            return source;
        }
    }
}
