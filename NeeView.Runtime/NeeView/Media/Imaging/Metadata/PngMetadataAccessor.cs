using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media.Imaging;

namespace NeeView.Media.Imaging.Metadata
{
    public class PngMetadataAccessor : BitmapMetadataAccessor
    {
        private static readonly string _strElementPrefix = "/{str=";

        private BitmapMetadata _meta;
        private Dictionary<string, List<string>> _textMap = new Dictionary<string, List<string>>();


        public PngMetadataAccessor(BitmapMetadata meta)
        {
            _meta = meta ?? throw new ArgumentNullException(nameof(meta));
            Debug.Assert(_meta.Format == "png");

            CollectItxtChunks();
            CollectTextChunks();
        }


        private void CollectItxtChunks()
        {
            foreach (var key in _meta.Where(e => e.EndsWith("iTXt", StringComparison.Ordinal)))
            {
                var itxt = _meta.GetQuery(key) as BitmapMetadata;
                if (itxt != null)
                {
                    var keyword = itxt.GetQuery("/Keyword") as string;
                    var text = itxt.GetQuery("/TextEntry") as string;
                    if (keyword != null && text != null)
                    {
                        AddToMap(keyword, text);
                    }
                }
            }
        }

        private void CollectTextChunks()
        {
            foreach (var key in _meta.Where(e => e.EndsWith("tEXt", StringComparison.Ordinal)))
            {
                var chunk = _meta.GetQuery(key) as BitmapMetadata;
                if (chunk != null)
                {

                    foreach (var title in chunk.Where(e => e.StartsWith(_strElementPrefix)))
                    {
                        var keywordLength = title.Length - _strElementPrefix.Length - 1;
                        if (keywordLength > 1)
                        {
                            var keyword = title.Substring(_strElementPrefix.Length, keywordLength);
                            var text = chunk.GetQuery(title) as string;
                            if (text != null)
                            {
                                AddToMap(keyword, text);
                            }
                        }
                    }
                }
            }
        }

        private void AddToMap(string key, string text)
        {
            if (!_textMap.TryGetValue(key, out var strings))
            {
                strings = new List<string>();
                _textMap.Add(key, strings);
            }
            strings.Add(text);
        }

        public override object GetValue(BitmapMetadataKey key)
        {
            switch (key)
            {
                case BitmapMetadataKey.Title: return GetPngText("Title");
                case BitmapMetadataKey.Subject: return GetPngText("Description");
                case BitmapMetadataKey.Rating: return null; // not supprted
                case BitmapMetadataKey.Tags: return null; // not supported
                case BitmapMetadataKey.Comments: return GetPngText("Comment");
                case BitmapMetadataKey.Author: return GetPngTextCollection("Author");
                case BitmapMetadataKey.DateTaken: return GetTime();
                case BitmapMetadataKey.ApplicatoinName: return GetPngText("Software");
                case BitmapMetadataKey.Copyright: return GetPngText("Copyright");

                // NOTE: PNGメタデータテキストの Warning, Disclaimer, Source は対応項目がない

                default: return null;
            }
        }

        private object GetTime()
        {
            var time = _meta.GetQuery("/tIME") as BitmapMetadata;
            if (time != null && false)
            {
                var timeMap = new Dictionary<string, int>();
                foreach (var item in time)
                {
                    timeMap[item] = Convert.ToInt32(time.GetQuery(item));
                }

                timeMap.TryGetValue("/Year", out int year);
                timeMap.TryGetValue("/Month", out int month);
                timeMap.TryGetValue("/Day", out int day);
                timeMap.TryGetValue("/Hour", out int hour);
                timeMap.TryGetValue("/Minute", out int minute);
                timeMap.TryGetValue("/Second", out int second);

                return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc).ToLocalTime();
            }

            var creationTime = GetPngText("Creation Time");
            if (creationTime != null)
            {
                if (DateTime.TryParse(creationTime, out var dateTime))
                {
                    return dateTime;
                }
                return creationTime;
            }

            return null;
        }

        private string GetPngText(string keyword)
        {
            if (_textMap.TryGetValue(keyword, out var strings))
            {
                return string.Join(Environment.NewLine, strings);
            }
            return null;
        }

        private ReadOnlyCollection<string> GetPngTextCollection(string keyword)
        {
            if (_textMap.TryGetValue(keyword, out var strings))
            {
                return new ReadOnlyCollection<string>(strings);
            }
            return null;
        }
    }
}

