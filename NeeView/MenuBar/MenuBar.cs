using NeeLaboratory.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace NeeView
{
    /// <summary>
    /// MenuBar : Model
    /// </summary>
    public class MenuBar : BindableBase
    {
        static MenuBar() => Current = new MenuBar();
        public static MenuBar Current { get; }


        private MenuBar()
        {
            MainMenuSource = MenuTree.CreateDefault();

            BookHub.Current.BookChanged +=
                (s, e) => UpdateLastFiles();

            RoutedCommandTable.Current.Changed +=
                (s, e) => Refresh();

            BookHistoryCollection.Current.HistoryChanged +=
                (s, e) =>
                {
                    switch (e.HistoryChangedType)
                    {
                        case BookMementoCollectionChangedType.Clear:
                        case BookMementoCollectionChangedType.Load:
                            UpdateLastFiles();
                            break;
                    }
                };
        }


        public event EventHandler CommandGestureChanged;


        public MenuTree MainMenuSource { get; set; }


        #region LastFiles
        // TODO: Historyで管理すべき

        // 最近使ったフォルダー
        private List<BookHistory> _lastFiles = new List<BookHistory>();
        public List<BookHistory> LastFiles
        {
            get { return _lastFiles; }
            set { _lastFiles = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(IsEnableLastFiles)); }
        }

        // 最近使ったフォルダーの有効フラグ
        public bool IsEnableLastFiles { get { return LastFiles.Count > 0; } }

        // 最近使ったファイル 更新
        private void UpdateLastFiles()
        {
            LastFiles = BookHistoryCollection.Current.ListUp(10);
        }

        #endregion


        public void Refresh()
        {
            CommandGestureChanged?.Invoke(this, null);
        }

        public void OpenMainMenuHelp()
        {
            var groups = new Dictionary<string, List<MenuTree.TableData>>();

            foreach (var group in MainMenuSource.Children)
            {
                groups.Add(group.Label, group.GetTable(0));
            }

            System.IO.Directory.CreateDirectory(Temporary.Current.TempSystemDirectory);
            string fileName = System.IO.Path.Combine(Temporary.Current.TempSystemDirectory, "MainMenuList.html");

            using (var writer = new System.IO.StreamWriter(fileName, false))
            {
                writer.WriteLine(HtmlHelpUtility.CraeteHeader("NeeView MainMenu List"));

                writer.WriteLine($"<body><h1>NeeView {Properties.Resources.WordMainMenu}</h1>");

                foreach (var pair in groups)
                {
                    writer.WriteLine($"<h3>{pair.Key.Replace("_", "")}</h3>");
                    writer.WriteLine("<table>");
                    writer.WriteLine($"<th>{Properties.Resources.WordItem}<th>{Properties.Resources.WordDescription}<tr>");
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

            System.Diagnostics.Process.Start(fileName);
        }

        public void OpenSearchOptionHelp()
        {
            System.IO.Directory.CreateDirectory(Temporary.Current.TempSystemDirectory);
            string fileName = System.IO.Path.Combine(Temporary.Current.TempSystemDirectory, "SearchOptions.html");

            using (var writer = new System.IO.StreamWriter(fileName, false))
            {
                writer.WriteLine(HtmlHelpUtility.GetSearchHelp());
            }

            System.Diagnostics.Process.Start(fileName);
        }

        #region Memento
        [DataContract]
        public class Memento : IMemento
        {
            [DataMember, DefaultValue(false)]
            public bool CaptionEmulateInFullScreen { get; set; }

            [DataMember]
            public bool IsHamburgerMenu { get; set; }

            public void RestoreConfig(Config config)
            {
                config.Window.IsCaptionEmulateInFullScreen = CaptionEmulateInFullScreen;
                config.MenuBar.IsHamburgerMenu = IsHamburgerMenu;
            }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.CaptionEmulateInFullScreen = Config.Current.Window.IsCaptionEmulateInFullScreen;
            memento.IsHamburgerMenu = Config.Current.MenuBar.IsHamburgerMenu;
            return memento;
        }

        #endregion

    }
}
