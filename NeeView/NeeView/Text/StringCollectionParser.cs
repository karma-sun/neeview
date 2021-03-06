using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeeView.Text
{
    public class StringCollectionParser
    {

        private enum CharType
        {
            End,
            Splitter,
            DoubleQuat,
            Any,
        }


        private class Context
        {
            private string _source;
            private int _index = -1;
            private StringBuilder _work = new StringBuilder();
            private List<string> _tokens = new List<string>();


            public Context(string source)
            {
                _source = source;
            }

            public string Source => _source;

            public List<string> Tokens => _tokens;


            public char GetCurrentChar()
            {
                if (_index < 0 || _source.Length <= _index)
                {
                    return '\0';
                }

                return _source[_index];
            }

            public void Next()
            {
                _index++;
            }

            public void Push()
            {
                var c = GetCurrentChar();
                if (c == '\0') throw new InvalidOperationException();
                _work.Append(c);
            }

            public void Take()
            {
                var s = _work.ToString().Trim();
                if (!string.IsNullOrEmpty(s))
                {
                    _tokens.Add(s);
                }
                _work.Clear();
            }
        }


        private class State
        {
            public State(Action<Context> action )
            {
                Action = action;
            }

            public Action<Context> Action { get; private set; }
            public List<State> NextStates { get; set; }
        }


        private static State s00 = new State(StateAction_Next);
        private static State s01 = new State(StateAction_Take);
        private static State s02 = new State(StateAction_Next);
        private static State s03 = new State(StateAction_Push);
        private static State s04 = new State(StateAction_Take);
        private static State s05 = new State(StateAction_Next);
        private static State s06 = new State(StateAction_Push);
        private static State s07 = new State(StateAction_Next);
        private static State err = new State(StateAction_Error);

        static StringCollectionParser()
        {
            s00.NextStates = new List<State> { s04, s01, s02, s03 };
            s01.NextStates = new List<State>() { s00, s00, s00, s00 };
            s02.NextStates = new List<State>() { err, s06, s05, s06 };
            s03.NextStates = new List<State>() { s07, s07, s07, s07 };
            s04.NextStates = new List<State>() { null, null, null, null };
            s05.NextStates = new List<State>() { s04, s01, s06, err };
            s06.NextStates = new List<State>() { s02, s02, s02, s02 };
            s07.NextStates = new List<State>() { s04, s01, err, s03 };
            err.NextStates = new List<State>() { null, null, null, null };
        }


        public static string Create(IEnumerable<string> items)
        {
            if (items is null) return "";

            return string.Join(";", items.Select(e => e.Contains(';') || e.Contains('"') ? ("\"" + e.Replace("\"", "\"\"") + "\"") : e));
        }

        public static List<string> Parse(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return new List<string>();
            }

            var context = new Context(source);
            var state = s00;

            while (state != null)
            {
                state.Action(context);
                state = state.NextStates[(int)GetCharType(context.GetCurrentChar())];
            }

            return context.Tokens;
        }

        private static CharType GetCharType(char c)
        {
            switch (c)
            {
                case '\0':
                    return CharType.End;
                case ';':
                    return CharType.Splitter;
                case '"':
                    return CharType.DoubleQuat;
                default:
                    return CharType.Any;
            }
        }

        private static void StateAction_Next(Context context)
        {
            context.Next();
        }

        private static void StateAction_Push(Context context)
        {
            context.Push();
        }

        private static void StateAction_Take(Context context)
        {
            context.Take();
        }

        private static void StateAction_Error(Context context)
        {
            throw new FormatException($"StringCollectionParser failed: \"{context.Source}\"");
        }

    }
}
