using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// word interpolation
    /// </summary>
    public class WordTree
    {
        public WordNode _root;


        public WordTree(WordNode root)
        {
            _root = root;
        }


        public IEnumerable<string> Interpolate(string source)
        {
            source = source?.TrimEnd() ?? "";

            var tokens = source.Split(' ').Last().Split('.');

            var node = SearchWordNode(_root, tokens);
            if (node == null)
            {
                yield return source;
                yield break;
            }

            var word = tokens.Last();
            var prefix = source.Substring(0, source.Length - word.Length);

            var words = InterpolateWord(word, node);
            if (words.Any())
            {
                foreach (var select in words)
                {
                    yield return prefix + select;
                }
            }
            else
            {
                yield return source;
            }
        }

        private WordNode SearchWordNode(WordNode node, IEnumerable<string> tokens)
        {
            if (tokens.Count() == 1)
            {
                return node;
            }
            if (node.Children == null)
            {
                return null;
            }
            else
            {
                var word = tokens.First();
                var subNode = node.Children.FirstOrDefault(e => e.Word == word);
                if (subNode != null)
                {
                    return SearchWordNode(subNode, tokens.Skip(1));
                }
                else
                {
                    return null;
                }
            }
        }

        private IEnumerable<string> InterpolateWord(string source, WordNode node)
        {
            if (node.Children == null)
            {
                yield break;
            }

            var selects = InterpolateWord(source, node.Children.Select(e => e.Word));
            if (selects.Count() == 0)
            {
                yield break;
            }
            else
            {
                foreach (var select in selects)
                {
                    yield return select;
                }
            }
        }

        private IEnumerable<string> InterpolateWord(string source, IEnumerable<string> candidates)
        {
            return candidates.Where(e => e.StartsWith(source, StringComparison.OrdinalIgnoreCase));
        }
    }
}
