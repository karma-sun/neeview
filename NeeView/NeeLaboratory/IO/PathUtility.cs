using System.IO;
using System.Text.RegularExpressions;


namespace NeeLaboratory.IO
{
    public static class PathUtility
    {
        /// <summary>
        /// ファイル名重複を回避する
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string CreateUniquePath(string source)
        {
            var path = source;

            var directory = Path.GetDirectoryName(path);
            var filename = Path.GetFileNameWithoutExtension(path);
            var extension = Path.GetExtension(path);
            int count = 1;

            var regex = new Regex(@"^(.+)\((\d+)\)$");
            var match = regex.Match(filename);
            if (match.Success)
            {
                filename = match.Groups[1].Value.Trim();
                count = int.Parse(match.Groups[2].Value);
            }

            // ファイル名作成
            while (File.Exists(path) || Directory.Exists(path))
            {
                count++;
                path = Path.Combine(directory, $"{filename} ({count}){extension}");
            }

            return path;
        }
    }
}

