namespace NeeView
{
    public class ScriptNotice
    {
        public ScriptNotice(string source, int line, string message)
        {
            Source = source;
            Line = line;
            Message = message;
        }


        public string Source { get; private set; }
        public int Line { get; private set; }
        public string Message { get; private set; }


        public override string ToString()
        {
            if (Source is null)
            {
                if (Line <= 0)
                {
                    return Message;
                }
                else
                {
                    return $"Line {Line}: {Message}";
                }
            }
            else
            {
                var filename = LoosePath.GetFileName(Source);
                if (Line < 0)
                {
                    return $"{filename}: {Message}";
                }
                else
                {
                    return $"{filename}({Line}): {Message}";
                }
            }
        }
    }
}
