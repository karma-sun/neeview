using System;
using System.Text;

namespace NeeView
{
    /// <summary>
    /// string builder with indent
    /// </summary>
    public class IndentStringBuilder
    {
        private static string[] _separator = new[] { "\r\n", "\n", "\r" };

        private StringBuilder _builder = new StringBuilder();
        private bool _isNewLine = true;

        public int Indent { get; private set; }

        public IndentStringBuilder Append(string s)
        {
            var lines = s.Split(_separator, StringSplitOptions.None);

            for (var i = 0; i < lines.Length; ++i)
            {
                AppendInner(lines[i]);

                if (i < lines.Length - 1)
                {
                    AppendLine();
                }
            }

            return this;
        }

        private IndentStringBuilder AppendInner(string s)
        {
            if (_isNewLine)
            {
                _builder.Append(IndentString());
                _isNewLine = false;
            }

            _builder.Append(s);
            return this;
        }

        public IndentStringBuilder AppendLine()
        {
            _builder.AppendLine();
            _isNewLine = true;
            return this;
        }

        public IndentStringBuilder IndentUp()
        {
            Indent++;
            if (!_isNewLine)
            {
                AppendLine();
            }
            return this;
        }
        public IndentStringBuilder IndentDown()
        {
            Indent--;
            if (!_isNewLine)
            {
                AppendLine();
            }
            return this;
        }

        private string IndentString()
        {
            return new string(' ', Indent * 2);
        }

        public override string ToString()
        {
            return _builder.ToString();
        }
    }
}
