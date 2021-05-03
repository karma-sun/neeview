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
            ExternalProcess.Start(fileName);
        }


        private string CreateScriptManualText()
        {
            var builder = new StringBuilder();

            builder.Append(HtmlHelpUtility.CraeteHeader("NeeView Script Manual"));
            builder.Append($"<body>");

            builder.Append(Properties.Resources._Document_ScriptManual_html);

            AppendScriptReference(builder);

            AppendConfigList(builder);

            AppendCommandList(builder);

            builder.Append(Properties.Resources._Document_ScriptManualExample_html);

            builder.Append("</body>");
            builder.Append(HtmlHelpUtility.CreateFooter());

            return builder.ToString();
        }

        private StringBuilder AppendScriptReference(StringBuilder builder)
        {
            builder.Append($"<h1 class=\"sub\">{ResourceService.GetString("@ScriptReference")}</h1>");
            builder.Append($"<p>{ResourceService.GetString("@ScriptReference.Summary")}</p>").AppendLine();

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

            htmlBuilder.Append(typeof(PlaylistPanelAccessor));
            htmlBuilder.Append(typeof(PlaylistItemAccessor));

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
            builder.Append($"<h1 class=\"sub\" id=\"ConfigList\">{Properties.Resources.Word_ConfigList}</h1>");
            builder.Append("<table class=\"table-slim table-topless\">");
            builder.Append($"<tr><th>{Properties.Resources.Word_Name}</th><th>{Properties.Resources.Word_Type}</th><th>{Properties.Resources.Word_Summary}</th></th>");
            builder.Append(ConfigMap.Current.Map.CreateHelpHtml("nv.Config"));
            builder.Append("</table>");
            return builder;
        }

        private StringBuilder AppendCommandList(StringBuilder builder)
        {
            var executeMethodArgTypes = new Type[] { typeof(object), typeof(CommandContext) };

            builder.Append($"<h1 class=\"sub\" id=\"CommandList\">{Properties.Resources.Word_CommandList}</h1>");
            builder.Append("<table class=\"table-slim table-topless\">");
            builder.Append($"<tr><th>{Properties.Resources.Word_Group}</th><th>{Properties.Resources.Word_Command}</th><th>{Properties.Resources.Word_CommandName}</th><th>{Properties.Resources.Word_Argument}</th><th>{Properties.Resources.Word_CommandParameter}</th><th>{Properties.Resources.Word_Summary}</th></tr>");
            foreach (var command in CommandTable.Current.Values.OrderBy(e => e.Order))
            {
                string argument = "";
                {
                    var type = command.GetType();
                    var info = type.GetMethod(nameof(command.Execute), executeMethodArgTypes);
                    var attribute = (MethodArgumentAttribute)Attribute.GetCustomAttributes(info, typeof(MethodArgumentAttribute)).FirstOrDefault();
                    if (attribute != null)
                    {
                        var tokens = MethodArgumentAttributeExtensions.GetMethodNote(info, attribute).Split('|');
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
                        properties = "<p style=\"color:red\">" + string.Format(Properties.Resources.CommandParameter_Share, command.Share.Name) + "</p>";
                    }

                    foreach (PropertyInfo info in type.GetProperties())
                    {
                        var attribute = (PropertyMemberAttribute)Attribute.GetCustomAttributes(info, typeof(PropertyMemberAttribute)).FirstOrDefault();
                        if (attribute != null && attribute.IsVisible)
                        {
                            var titleString = PropertyMemberAttributeExtensions.GetPropertyTitle(info, attribute);
                            if (titleString != null)
                            {
                                title = titleString + " / ";
                            }

                            var enums = "";
                            if (info.PropertyType.IsEnum)
                            {
                                enums = string.Join(" / ", info.PropertyType.VisibledAliasNameDictionary().Select(e => $"\"{e.Key}\": {e.Value}")) + "<br/>";
                            }

                            var propertyName = PropertyMemberAttributeExtensions.GetPropertyName(info, attribute).TrimEnd(Properties.Resources.Word_Period.ToArray()) + Properties.Resources.Word_Period;
                            var text = title + propertyName;

                            var propertyTips = PropertyMemberAttributeExtensions.GetPropertyTips(info, attribute);
                            if (propertyTips != null)
                            {
                                text = text + " " + propertyTips;
                            }

                            properties = properties + $"<dt><b>{info.Name}</b>: {info.PropertyType.ToManualString()}</dt><dd>{enums + text}<dd/>";
                        }
                    }
                    if (!string.IsNullOrEmpty(properties))
                    {
                        properties = "<dl>" + properties + "</dl>";
                    }
                }

                builder.Append($"<tr><td>{command.Group}</td><td>{command.Text}</td><td><b>{command.Name}</b></td><td>{argument}</td><td>{properties}</td><td>{command.Remarks}</td></tr>");
            }
            builder.Append("</table>");

            return builder;
        }
    }
}
