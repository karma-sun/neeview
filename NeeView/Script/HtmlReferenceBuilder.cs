using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace NeeView
{
    public class HtmlReferenceBuilder
    {
        private StringBuilder builder;

        public HtmlReferenceBuilder() : this(new StringBuilder())
        {
        }

        public HtmlReferenceBuilder(StringBuilder builder)
        {
            this.builder = builder;
        }

        public StringBuilder ToStringBuilder()
        {
            return builder;
        }

        public override string ToString()
        {
            return builder.ToString();
        }

        public HtmlReferenceBuilder Append(string src)
        {
            builder.Append(src);
            return this;
        }

        public HtmlReferenceBuilder AppendLine()
        {
            builder.AppendLine();
            return this;
        }

        /// <summary>
        /// 指定した型のリファレンスを作成する
        /// </summary>
        /// <param name="type">対象の型</param>
        /// <param name="name">型の表示面を指定。nullで標準</param>
        public HtmlReferenceBuilder Append(Type type, string name = null)
        {
            if (type.IsEnum)
            {
                return AppendEnum(type, name);
            }
            else
            {
                return AppendClass(type, name);
            }
        }

        /// <summary>
        /// 指定したEnum型のリファレンスを作成する
        /// </summary>
        public HtmlReferenceBuilder AppendEnum(Type type, string name)
        {
            if (!type.IsEnum) throw new ArgumentException();

            var title = name ?? $"[Enum] {type.Name}";

            builder.Append($"<h2 id=\"{type.Name}\">{title}</h2>").AppendLine();

            AppendSummary(type.Name);

            builder.Append($"<h4>{ResourceService.GetString("@Word.Fields")}</h4>").AppendLine();

            AppendDictionary(type.VisibledAliasNameDictionary().ToDictionary(e => e.Key.ToString(), e => e.Value));

            return this;
        }

        /// <summary>
        /// DictionaryをHTMLテーブルとして出力する
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="style">tableのclass。nullで標準</param>
        /// <returns></returns>
        private HtmlReferenceBuilder AppendDictionary(Dictionary<string, string> dictionary, string style = null)
        {
            style = style ?? "table-slim";
            builder.Append($"<p><table class=\"{style}\">").AppendLine();
            foreach (var member in dictionary)
            {
                builder.Append($"<tr><td>{member.Key}</td><td>{member.Value}</td></tr>").AppendLine();
            }
            builder.Append("</table></p>").AppendLine();

            return this;
        }

        /// <summary>
        /// DataTableをHTMLテーブルとして出力する
        /// </summary>
        /// <param name="dataTable"></param>
        /// <param name="isHeader">ヘッダ行を出力するか</param>
        /// <returns></returns>
        private HtmlReferenceBuilder AppendDataTable(DataTable dataTable, bool isHeader)
        {
            var tableClass = "table-slim" + (isHeader ? " table-topless" : "");
            builder.Append($"<p><table class=\"{tableClass}\">").AppendLine();

            if (isHeader)
            {
                builder.Append("<tr>");
                foreach (DataColumn col in dataTable.Columns)
                {
                    builder.Append($"<th>{col.Caption}</th>");
                }
                builder.Append("</tr>").AppendLine();
            }

            foreach (DataRow row in dataTable.Rows)
            {
                builder.Append("<tr>");
                foreach (DataColumn col in dataTable.Columns)
                {
                    builder.Append($"<td>{ row[col]}</td>");
                }
                builder.Append("</tr>").AppendLine();
            }

            builder.Append("</table></p>").AppendLine();

            return this;
        }


        /// <summary>
        /// Summary,RemarksをHTMLに出力
        /// </summary>
        /// <param name="name">対象のリソーステキスト名</param>
        /// <param name="isRemarks">Remarksを含める</param>
        /// <returns></returns>
        private HtmlReferenceBuilder AppendSummary(string name, bool isRemarks = true)
        {
            var summary = GetHtmlDocument(name, "");
            builder.Append($"<p>").Append(summary).Append("</p>").AppendLine();

            if (isRemarks)
            {
                var remarks = GetHtmlDocument(name, "#Remarks", false);
                if (remarks != null)
                {
                    builder.Append($"<p>").Append(remarks).Append("</p>").AppendLine();
                }
            }

            return this;
        }


        /// <summary>
        /// 指定したClass型のリファレンスを作成する
        /// </summary>
        public HtmlReferenceBuilder AppendClass(Type type, string name = null)
        {
            var className = name ?? type.Name;
            var title = name ?? $"[Class] {type.Name}";

            builder.Append($"<h2 id=\"{type.Name}\">{title}</h2>").AppendLine();

            // summary
            AppendSummary(type.Name);

            // property
            builder.Append($"<h4>{ResourceService.GetString("@Word.Properties")}</h4>").AppendLine();
            var properties = type.GetProperties().Where(e => e.GetCustomAttribute<DocumentableAttribute>() != null);
            AppendDataTable(PropertiesToDataTable(properties), false);

            // examples
            AppendExamples(properties.Select(e => e.DeclaringType.Name + "." + e.Name).Prepend(type.Name));

            // method
            var methods = type.GetMethods().Where(e => e.GetCustomAttribute<DocumentableAttribute>() != null);
            foreach (var method in methods)
            {
                AppendMethod(method, className);
            }

            return this;
        }

        /// <summary>
        /// 指定したクラスのメソッドたちをリファレンス化する
        /// </summary>
        public HtmlReferenceBuilder CreateMethods(Type type, string prefix)
        {
            var methods = type.GetMethods().Where(e => e.GetCustomAttribute<DocumentableAttribute>() != null);
            foreach (var method in methods)
            {
                AppendMethod(method, prefix);
            }

            return this;
        }

        /// <summary>
        /// メソッドのリファレンス化
        /// </summary>
        private HtmlReferenceBuilder AppendMethod(MethodInfo method, string prefix)
        {
            var name = method.DeclaringType.Name + "." + method.Name;

            var documentable = method.GetCustomAttribute<DocumentableAttribute>();

            var title = (string.IsNullOrEmpty(prefix) ? "" : prefix + ".") + (documentable?.Name ?? method.Name) + "(" + string.Join(", ", method.GetParameters().Select(e => e.Name)) + ")";
            builder.Append($"<h3>{title}</h3>").AppendLine();

            AppendSummary(name);

            var parameters = method.GetParameters();
            if (parameters.Length > 0)
            {
                builder.Append($"<h4>{ResourceService.GetString("@Word.Parameters")}</h4>").AppendLine();
                AppendDataTable(ParametersToDataTable(method, parameters), false);
            }

            if (method.ReturnType != typeof(void))
            {
                builder.Append($"<h4>{ResourceService.GetString("@Word.Returns")}</h4>").AppendLine();
                var typeString = TypeToString(method.ReturnType);
                var summary = GetHtmlDocument(name, "#Returns");
                AppendDictionary(new Dictionary<string, string> { [typeString] = summary }, "table-none");
            }

            AppendExample(name);

            return this;
        }

        /// <summary>
        /// 使用例たちの出力
        /// </summary>
        private HtmlReferenceBuilder AppendExamples(IEnumerable<string> names)
        {
            var examples = names.Select(e => GetDocument(e, "#Example", false)?.Trim()).Where(e => !string.IsNullOrEmpty(e));
            if (examples.Any())
            {
                builder.Append($"<h4>{ResourceService.GetString("@Word.Example")}</h4>").AppendLine();
                foreach (var example in examples)
                {
                    builder.Append("<p><pre><code class=\"example\">");
                    builder.Append(example);
                    builder.Append("</code></pre></p>").AppendLine();
                }
            }

            return this;
        }

        /// <summary>
        /// 使用例の出力
        /// </summary>
        private HtmlReferenceBuilder AppendExample(string name)
        {
            return AppendExamples(new string[] { name });
        }

        /// <summary>
        /// パラメーターのDataTable化
        /// </summary>
        private DataTable ParametersToDataTable(MethodInfo method, IEnumerable<ParameterInfo> parameters)
        {
            var dataTable = new DataTable("Parameters");
            dataTable.Columns.Add(new DataColumn("name", typeof(string)));
            dataTable.Columns.Add(new DataColumn("type", typeof(string)));
            dataTable.Columns.Add(new DataColumn("summary", typeof(string)));

            foreach (var parameter in parameters)
            {
                var name = string.Join(".", new string[] { method.DeclaringType.Name, method.Name, parameter.Name });
                var typeString = TypeToString(parameter.ParameterType);
                var summary = GetHtmlDocument(name, "");

                dataTable.Rows.Add(parameter.Name, typeString, summary);
            }

            return dataTable;
        }

        /// <summary>
        /// プロパティのDataTable化
        /// </summary>
        private DataTable PropertiesToDataTable(IEnumerable<PropertyInfo> properties)
        {
            var dataTable = new DataTable("Properties");
            dataTable.Columns.Add(new DataColumn("name", typeof(string)));
            dataTable.Columns.Add(new DataColumn("type", typeof(string)));
            dataTable.Columns.Add(new DataColumn("rw", typeof(string)));
            dataTable.Columns.Add(new DataColumn("summary", typeof(string)));

            foreach (var property in properties)
            {
                var name = property.DeclaringType.Name + "." + property.Name;
                var attribute = property.GetCustomAttribute<DocumentableAttribute>();
                var typeString = TypeToString(property.PropertyType) + (attribute.DocumentType != null ? $" ({TypeToString(attribute.DocumentType)})" : "");
                var rw = (property.CanRead ? "r" : "") + (property.CanWrite ? "w" : "");
                var summary = GetHtmlDocument(name, "");

                dataTable.Rows.Add(property.Name, typeString, rw, summary);
            }

            return dataTable;
        }

        /// <summary>
        /// プレーンテキストのHTML化
        /// </summary>
        /// <remarks>
        /// 改行変換だけの簡単なもの
        /// </remarks>
        private string TextToHtmlFormat(string src)
        {
            if (src is null) return null;

            var regex = new Regex(@"[\r\n]+");
            return regex.Replace(src, "<br />");
        }

        /// <summary>
        /// リソースキーから文字列を取得する
        /// </summary>
        /// <param name="name">リソース名</param>
        /// <param name="postfix">リソース属性名 (e.g. #Remarks)</param>
        /// <param name="notNull">trueの場合、リソースが存在しなければリソース名を返す</param>
        /// <returns>取得された文字列</returns>
        private string GetDocument(string name, string postfix, bool notNull = true)
        {
            var resourceId = $"@{name}{postfix}";
            var text = ResourceService.GetResourceString(resourceId);
            if (text is null && notNull)
            {
                text = resourceId;
            }
            return text;
        }

        /// <summary>
        /// リソースキーからHTML文字列を取得する
        /// </summary>
        /// <param name="name">リソース名</param>
        /// <param name="postfix">リソース属性名 (e.g. #Remarks)</param>
        /// <param name="notNull">trueの場合、リソースが存在しなければリソース名を返す</param>
        /// <returns>HTML化された文字列</returns>
        private string GetHtmlDocument(string name, string postfix, bool notNull = true)
        {
            var text = GetDocument(name, postfix, notNull);
            return TextToHtmlFormat(text);
        }

        /// <summary>
        /// リファレンスに適した型名を取得する
        /// </summary>
        private string TypeToString(Type type)
        {
            if (type == typeof(void))
            {
                return "void";
            }

            if (type.IsEnum)
            {
                return TypeAnchor(type);
            }

            if (type.IsArray)
            {
                var elementTypeString = TypeToString(type.GetElementType());
                return elementTypeString + "[]";
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return "bool";
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return "int";
                case TypeCode.Double:
                    return "double";
                case TypeCode.String:
                    return "string";
            }

            if (type == typeof(object))
            {
                return "object";
            }

            var propertyIndexer = type.GetProperties().Where(p => p.GetIndexParameters().Length != 0).FirstOrDefault();
            if (propertyIndexer != null)
            {
                if (propertyIndexer.PropertyType != typeof(object) && propertyIndexer.PropertyType != typeof(string))
                {
                    return ToAnchor(propertyIndexer.PropertyType.Name) + "[]";
                }

                return "dictionary";
            }

            return TypeAnchor(type);
        }

        /// <summary>
        /// 型をアンカー付きテキストに変換
        /// </summary>
        private string TypeAnchor(Type type)
        {
            if (type.IsArray)
            {
                return ToAnchor(type.GetElementType().Name) + "[]";
            }
            else
            {
                return ToAnchor(type.Name);
            }
        }

        /// <summary>
        /// 文字列のアンカー化
        /// </summary>
        /// <param name="src"></param>
        /// <param name="id">参照先。nullで標準</param>
        private string ToAnchor(string src, string id = null)
        {
            id = id ?? "#" + src;
            return $"<a href=\"{id}\">{src}</a>";
        }
    }
}
