using System;
using System.IO;
using System.Text.RegularExpressions;

namespace NeeView
{
    public class ScriptCommandSource
    {
        public const string Extension = ".nvjs";
        public const string OnBookLoadedFilename = "OnBookLoaded";
        public const string OnPageChangedFilename = "OnPageChanged";

        private static Regex _regexCommentLine = new Regex(@"^\s*/{2,}");
        private static Regex _regexDocComment = new Regex(@"^\s*/{2,}\s*(@\w+)\s+(.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);


        public ScriptCommandSource(string path)
        {
            Path = path;
        }


        public string Path { get; private set; }
        public string Name => LoosePath.GetFileNameWithoutExtension(Path);
        public bool IsCloneable { get; private set; }
        public string Text { get; private set; }
        public string Remarks { get; private set; }
        public string ShortCutKey { get; private set; }
        public string MouseGesture { get; private set; }
        public string TouchGesture { get; private set; }



        public static ScriptCommandSource Create(string path)
        {
            var source = new ScriptCommandSource(path);
            
            var filename = LoosePath.GetFileNameWithoutExtension(path);
            source.Text = filename;

            switch (filename)
            {
                case OnBookLoadedFilename:
                    source.Remarks = Properties.Resources.ScriptOnBookLoadedCommand_Remarks;
                    break;

                case OnPageChangedFilename:
                    source.Remarks = Properties.Resources.ScriptOnPageChangedCommand_Remarks;
                    break;

                default:
                    source.Remarks = Properties.Resources.ScriptCommand_Remarks;
                    source.IsCloneable = true;
                    break;
            }

            using (var reader = new StreamReader(path))
            {
                bool isComment = false;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (_regexCommentLine.IsMatch(line))
                    {
                        isComment = true;
                        var match = _regexDocComment.Match(line);
                        if (match.Success)
                        {
                            var key = match.Groups[1].Value.ToLower();
                            var value = match.Groups[2].Value.Trim();
                            switch (key)
                            {
                                case "@name":
                                    source.Text = value;
                                    break;
                                case "@description":
                                    source.Remarks = value;
                                    break;
                                case "@shortcutkey":
                                    source.ShortCutKey = value;
                                    break;
                                case "@mousegesture":
                                    source.MouseGesture = value;
                                    break;
                                case "@touchgesture":
                                    source.TouchGesture = value;
                                    break;
                            }
                        }
                    }
                    else if (isComment && !string.IsNullOrWhiteSpace(line))
                    {
                        break;
                    }
                }
            }

            return source;
        }
    }
}
