using NeeView.Collections.Generic;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace NeeView.Data
{
    /// <summary>
    /// Json
    /// </summary>
    public static class Json
    {
        /// <summary>
        /// シリアライズ
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static string Serialize(object element, Type type)
        {
            var serializer = new DataContractJsonSerializer(type);

            using (var stream = new System.IO.MemoryStream())
            {
                serializer.WriteObject(stream, element);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }


        /// <summary>
        /// シリアライズ
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static string Serialize<T>(T element) where T : class
        {
            var serializer = new DataContractJsonSerializer(typeof(T));

            using (var stream = new System.IO.MemoryStream())
            {
                serializer.WriteObject(stream, element);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }


        /// <summary>
        /// デシリアライズ
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object Deserialize(System.IO.Stream stream, Type type)
        {
            var serializer = new DataContractJsonSerializer(type);
            return serializer.ReadObject(stream);
        }

        /// <summary>
        /// デシリアライズ
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static object Deserialize(string json, Type type)
        {
            using (var stream = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                return Deserialize(stream, type);
            }
        }


        /// <summary>
        /// デシリアライズ
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T Deserialize<T>(System.IO.Stream stream) where T : class
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            return (T)serializer.ReadObject(stream);
        }

        /// <summary>
        /// デシリアライズ
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string json) where T : class
        {
            using (var stream = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                return Deserialize<T>(stream);
            }
        }

        /// <summary>
        /// デシリアライズ
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T Deserialize<T>(byte[] json) where T : class
        {
            using (var stream = new System.IO.MemoryStream(json))
            {
                return Deserialize<T>(stream);
            }
        }






        /// <summary>
        /// シリアライズ(GZip)
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static byte[] SerializeGZip<T>(T element) where T : class
        {
            // 入力用ストリーム
            using (var inStream = new System.IO.MemoryStream())
            {
                // JSON Serialize
                var serializer = new DataContractJsonSerializer(typeof(T));
                serializer.WriteObject(inStream, element);

                // 出力用ストリーム
                using (var outStream = new System.IO.MemoryStream())
                {
                    // 出力用ストリームに gzip ストリームのフィルターを付ける
                    using (GZipStream gzout = new GZipStream(outStream, CompressionMode.Compress))
                    {
                        inStream.Seek(0, System.IO.SeekOrigin.Begin);
                        inStream.CopyTo(gzout);
                    }

                    return outStream.ToArray();
                }
            }
        }

        /// <summary>
        /// デシリアライズ(GZip)
        /// </summary>
        /// <param name="buff"></param>
        /// <returns></returns>
        public static T DeserializeGZip<T>(byte[] buff) where T : class
        {
            // 入力用ストリーム
            using (var inStream = new System.IO.MemoryStream(buff))
            {
                // 出力用ストリーム
                using (var outStream = new System.IO.MemoryStream())
                {
                    // 入力用ストリームに gzip ストリームのフィルターを付ける
                    using (GZipStream gzin = new GZipStream(inStream, CompressionMode.Decompress))
                    {
                        gzin.CopyTo(outStream);
                    }

                    // JSON Deserialize
                    var serializer = new DataContractJsonSerializer(typeof(T));
                    outStream.Seek(0, System.IO.SeekOrigin.Begin);
                    return (T)serializer.ReadObject(outStream);
                }
            }
        }


        /// <summary>
        /// Clone by JSON
        /// </summary>
        /// <param name="source"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object Clone(object source, Type type)
        {
            return Deserialize(Serialize(source, type), type);
        }


        /// <summary>
        /// Clone by JSON
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T Clone<T>(T source) where T : class
        {
            return Deserialize<T>(Serialize(source));
        }


        #region JSON formatter

        //
        private const string INDENT_STRING = "    ";

        /// <summary>
        /// Jsonのフォーマット (開発用)
        /// http://stackoverflow.com/questions/4580397/json-formatter-in-c/19534474
        /// </summary>
        public static string Format(string json)
        {
            var indent = 0;
            var quoted = false;
            var sb = new StringBuilder();
            for (var i = 0; i < json.Length; i++)
            {
                var ch = json[i];
                switch (ch)
                {
                    case '{':
                    case '[':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, ++indent).ForEach(item => sb.Append(INDENT_STRING));
                        }
                        break;
                    case '}':
                    case ']':
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, --indent).ForEach(item => sb.Append(INDENT_STRING));
                        }
                        sb.Append(ch);
                        break;
                    case '"':
                        sb.Append(ch);
                        bool escaped = false;
                        var index = i;
                        while (index > 0 && json[--index] == '\\')
                            escaped = !escaped;
                        if (!escaped)
                            quoted = !quoted;
                        break;
                    case ',':
                        sb.Append(ch);
                        if (!quoted)
                        {
                            sb.AppendLine();
                            Enumerable.Range(0, indent).ForEach(item => sb.Append(INDENT_STRING));
                        }
                        break;
                    case ':':
                        sb.Append(ch);
                        if (!quoted)
                            sb.Append(" ");
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }
            return sb.ToString();
        }
    }


    #endregion
}
