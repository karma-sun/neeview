using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public static class Documents
    {
        public static void OpenMainMenuHelp()
        {
            var groups = new Dictionary<string, List<MenuTree.TableData>>();

            foreach (var group in MainMenu.Current.MenuSource.Children)
            {
                groups.Add(group.Label, group.GetTable(0));
            }

            System.IO.Directory.CreateDirectory(Temporary.Current.TempSystemDirectory);
            string fileName = System.IO.Path.Combine(Temporary.Current.TempSystemDirectory, "MainMenuList.html");

            using (var writer = new System.IO.StreamWriter(fileName, false))
            {
                writer.WriteLine(HtmlHelpUtility.CraeteHeader("NeeView MainMenu List"));

                writer.WriteLine($"<body><h1>NeeView {Properties.Resources.Word_MainMenu}</h1>");

                foreach (var pair in groups)
                {
                    writer.WriteLine($"<h3>{pair.Key.Replace("_", "")}</h3>");
                    writer.WriteLine("<table class=\"table-slim\">");
                    foreach (var item in pair.Value)
                    {
                        string name = string.Concat(Enumerable.Repeat("&nbsp;", item.Depth * 2)) + item.Element.DispLabel;

                        writer.WriteLine($"<td>{name}<td>{item.Element.Note}<tr>");
                    }
                    writer.WriteLine("</table>");
                }
                writer.WriteLine("</body>");

                writer.WriteLine(HtmlHelpUtility.CreateFooter());
            }

            ExternalProcess.Start(fileName);
        }

        public static void OpenSearchOptionHelp()
        {
            System.IO.Directory.CreateDirectory(Temporary.Current.TempSystemDirectory);
            string fileName = System.IO.Path.Combine(Temporary.Current.TempSystemDirectory, "SearchOptions.html");

            using (var writer = new System.IO.StreamWriter(fileName, false))
            {
                writer.WriteLine(HtmlHelpUtility.GetSearchHelp());
            }

            ExternalProcess.Start(fileName);
        }
    }
}
