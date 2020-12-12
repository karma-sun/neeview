using NeeView.Windows.Property;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Resources;

namespace NeeView
{
    public class ScriptManual
    {
        public void OpenScriptManual()
        {
            Directory.CreateDirectory(Temporary.Current.TempSystemDirectory);
            string fileName = System.IO.Path.Combine(Temporary.Current.TempSystemDirectory, "ScriptManual.html");

            // create html file
            using (var writer = new System.IO.StreamWriter(fileName, false))
            {
                writer.Write(CreateScriptManualText());
            }

            // open in browser
            System.Diagnostics.Process.Start(fileName);
        }


        private string CreateScriptManualText()
        {
            var builder = new StringBuilder();

            builder.Append(HtmlHelpUtility.CraeteHeader("NeeView Script Manual"));
            builder.Append($"<body>");

            AppendResource(builder, $"/Resources/{Config.Current.System.Language.GetCultureName()}/ScriptManual.html");

            AppendScriptReference(builder);

            AppendConfigList(builder);

            AppendCommandList(builder);

            AppendResource(builder, $"/Resources/{Config.Current.System.Language.GetCultureName()}/ScriptManualExample.html");

            builder.Append("</body>");
            builder.Append(HtmlHelpUtility.CreateFooter());

            return builder.ToString();
        }

        private StringBuilder AppendScriptReference(StringBuilder builder)
        {
            builder.Append($"<h1 class=\"sub\">{ResourceService.GetString("@ScriptReference#Name")}</h1>");
            builder.Append($"<p>{ResourceService.GetString("@ScriptReference")}</p>").AppendLine();

            var htmlBuilder = new HtmlReferenceBuilder(builder);

            htmlBuilder.CreateMethods(typeof(JavascriptEngine), null);

            htmlBuilder.Append($"<hr/>").AppendLine();

            htmlBuilder.Append(typeof(CommandHost), "nv");

            htmlBuilder.Append(typeof(CommandAccessor));

            htmlBuilder.Append(typeof(BookAccessor));
            htmlBuilder.Append(typeof(BookConfigAccessor));
            htmlBuilder.Append(typeof(PageAccessor));
            htmlBuilder.Append(typeof(ViewPageAccessor));

            htmlBuilder.Append(typeof(BookshelfPanelAccessor));
            htmlBuilder.Append(typeof(BookshelfItemAccessor));

            htmlBuilder.Append(typeof(PageListPanelAccessor));

            htmlBuilder.Append(typeof(BookmarkPanelAccessor));
            htmlBuilder.Append(typeof(BookmarkItemAccessor));

            htmlBuilder.Append(typeof(PagemarkPanelAccessor));
            htmlBuilder.Append(typeof(PagemarkItemAccessor));

            htmlBuilder.Append(typeof(HistoryPanelAccessor));
            htmlBuilder.Append(typeof(HistoryItemAccessor));

            htmlBuilder.Append(typeof(InformationPanelAccessor));

            htmlBuilder.Append(typeof(EffectPanelAccessor));

            htmlBuilder.Append(typeof(NavigatorPanelAccessor));

            htmlBuilder.Append($"<hr/>").AppendLine();

            htmlBuilder.Append(typeof(FolderOrder));
            htmlBuilder.Append(typeof(PageSortMode));
            htmlBuilder.Append(typeof(PageNameFormat));
            htmlBuilder.Append(typeof(PageReadOrder));
            htmlBuilder.Append(typeof(PanelListItemStyle));

            return htmlBuilder.ToStringBuilder();
        }

        private StringBuilder AppendConfigList(StringBuilder builder)
        {
            builder.Append($"<h1 class=\"sub\" id=\"ConfigList\">{Properties.Resources.WordConfigList}</h1>");
            builder.Append("<table class=\"table-slim table-topless\">");
            builder.Append($"<tr><th>{Properties.Resources.WordName}</th><th>{Properties.Resources.WordType}</th><th>{Properties.Resources.Word_Summary}</th></th>");
            builder.Append(ConfigMap.Current.Map.CreateHelpHtml("nv.Config"));
            builder.Append("</table>");
            return builder;
        }

        private StringBuilder AppendCommandList(StringBuilder builder)
        {
            var executeMethodArgTypes = new Type[] { typeof(object), typeof(CommandContext) };

            builder.Append($"<h1 class=\"sub\" id=\"CommandList\">{Properties.Resources.WordCommandList}</h1>");
            builder.Append("<table class=\"table-slim table-topless\">");
            builder.Append($"<tr><th>{Properties.Resources.WordGroup}</th><th>{Properties.Resources.WordCommand}</th><th>{Properties.Resources.WordCommandName}</th><th>{Properties.Resources.WordArgument}</th><th>{Properties.Resources.WordCommandParameter}</th><th>{Properties.Resources.Word_Summary}</th></tr>");
            foreach (var command in CommandTable.Current.Values)
            {
                string argument = "";
                {
                    var type = command.GetType();
                    var info = type.GetMethod(nameof(command.Execute), executeMethodArgTypes);
                    var attribute = (MethodArgumentAttribute)Attribute.GetCustomAttributes(info, typeof(MethodArgumentAttribute)).FirstOrDefault();
                    if (attribute != null)
                    {
                        var tokens = ResourceService.GetString(attribute.Note).Split('|');
                        int index = 0;
                        argument += "<dl>";
                        while (index < tokens.Length)
                        {
                            var dt = tokens.ElementAtOrDefault(index++);
                            var dd = tokens.ElementAtOrDefault(index++);
                            argument += $"<dt>{dt}</dt><dd>{dd}</dd>";
                        }
                        argument += "</dl>";
                    }
                }

                string properties = "";
                if (command.Parameter != null)
                {
                    var type = command.Parameter.GetType();
                    var title = "";

                    if (command.Share != null)
                    {
                        properties = "<p style=\"color:red\">" + string.Format(Properties.Resources.ParamCommandShare, command.Share.Name) + "</p>";
                    }

                    foreach (PropertyInfo info in type.GetProperties())
                    {
                        var attribute = (PropertyMemberAttribute)Attribute.GetCustomAttributes(info, typeof(PropertyMemberAttribute)).FirstOrDefault();
                        if (attribute != null && attribute.IsVisible)
                        {
                            if (attribute.Title != null)
                            {
                                title = ResourceService.GetString(attribute.Title) + " / ";
                            }

                            var enums = "";
                            if (info.PropertyType.IsEnum)
                            {
                                enums = string.Join(" / ", info.PropertyType.VisibledAliasNameDictionary().Select(e => $"\"{e.Key}\": {e.Value}")) + "<br/>";
                            }

                            var text = title + ResourceService.GetString(attribute.Name).TrimEnd(Properties.Resources.WordPeriod.ToArray()) + Properties.Resources.WordPeriod + (attribute.Tips != null ? " " + ResourceService.GetString(attribute.Tips) : "");

                            properties = properties + $"<dt><b>{info.Name}</b>: {info.PropertyType.ToManualString()}</dt><dd>{enums + text}<dd/>";
                        }
                    }
                    if (!string.IsNullOrEmpty(properties))
                    {
                        properties = "<dl>" + properties + "</dl>";
                    }
                }

                builder.Append($"<tr><td>{command.Group}</td><td>{command.Text}</td><td><b>{command.Name}</b></td><td>{argument}</td><td>{properties}</td><td>{command.Note}</td></tr>");
            }
            builder.Append("</table>");

            return builder;
        }

        private StringBuilder AppendResource(StringBuilder builder, string resourcPath)
        {
            Uri fileUri = new Uri(resourcPath, UriKind.Relative);
            StreamResourceInfo info = System.Windows.Application.GetResourceStream(fileUri);
            using (StreamReader sr = new StreamReader(info.Stream))
            {
                builder.Append(sr.ReadToEnd());
            }

            return builder;
        }
    }
}
