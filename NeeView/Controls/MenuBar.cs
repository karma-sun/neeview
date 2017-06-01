// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using NeeView.ComponentModel;
using NeeView.Windows.Property;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace NeeView
{
    /// <summary>
    /// MenuBar : Model
    /// </summary>
    public class MenuBar : BindableBase
    {
        public static MenuBar Current { get; private set; }

        /// <summary>
        /// constructor
        /// </summary>
        public MenuBar()
        {
            Current = this;
            MainMenuSource = MenuTree.CreateDefault();

            BookHub.Current.BookChanged +=
                (s, e) => UpdateLastFiles();

            RoutedCommandTable.Current.Changed +=
                (s, e) => Reflesh();
        }


        #region events

        public event EventHandler CommandGestureChanged;

        #endregion


        //
        public bool IsCaptionEmulateInFullScreen { get; set; }


        //
        public MenuTree MainMenuSource { get; set; }


        #region LastFiles
        // TODO: Historyで管理すべき

        // 最近使ったフォルダー
        private List<Book.Memento> _lastFiles = new List<Book.Memento>();
        public List<Book.Memento> LastFiles
        {
            get { return _lastFiles; }
            set { _lastFiles = value; RaisePropertyChanged(); RaisePropertyChanged(nameof(IsEnableLastFiles)); }
        }

        // 最近使ったフォルダーの有効フラグ
        public bool IsEnableLastFiles { get { return LastFiles.Count > 0; } }

        // 最近使ったファイル 更新
        public void UpdateLastFiles()
        {
            LastFiles = BookHistory.Current.ListUp(10);
        }

        #endregion


        //
        public void Reflesh()
        {
            CommandGestureChanged?.Invoke(this, null);
        }

        //
        public void OpenMainMenuHelp()
        {
            var groups = new Dictionary<string, List<MenuTree.TableData>>();

            //
            foreach (var group in MainMenuSource.Children)
            {
                groups.Add(group.Label, group.GetTable(0));
            }

            // 
            System.IO.Directory.CreateDirectory(Temporary.TempSystemDirectory);
            string fileName = System.IO.Path.Combine(Temporary.TempSystemDirectory, "MainMenuList.html");


            //
            using (var writer = new System.IO.StreamWriter(fileName, false))
            {
                var regex = new Regex(@"\(_(\w)\)");
                var regexReplace = @"($1)";

                writer.WriteLine(NVUtility.HtmlHelpHeader("NeeView MainMenu List"));

                writer.WriteLine("<body><h1>NeeView メインメニュー</h1>");

                foreach (var pair in groups)
                {
                    writer.WriteLine($"<h3>{regex.Replace(pair.Key, regexReplace)}</h3>");
                    writer.WriteLine("<table>");
                    writer.WriteLine($"<th>項目<th>説明<tr>");
                    foreach (var item in pair.Value)
                    {
                        string name = new string('　', item.Depth * 2) + regex.Replace(item.Element.Label, regexReplace);

                        writer.WriteLine($"<td>{name}<td>{item.Element.Note}<tr>");
                    }
                    writer.WriteLine("</table>");
                }
                writer.WriteLine("</body>");

                writer.WriteLine(NVUtility.HtmlHelpFooter());
            }

            System.Diagnostics.Process.Start(fileName);
        }


        #region Memento
        [DataContract]
        public class Memento
        {
            [DataMember, DefaultValue(false)]
            [PropertyMember("フルスクリーン時のタイトルバー操作", Tips = "フルスクリーン時のメニュー上でのタイトルバー操作(ダブルクリックやドラッグ)を有効にします")]
            public bool CaptionEmulateInFullScreen { get; set; }
        }

        //
        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.CaptionEmulateInFullScreen = this.IsCaptionEmulateInFullScreen;
            return memento;
        }

        //
        public void Restore(Memento memento)
        {
            if (memento == null) return;
            this.IsCaptionEmulateInFullScreen = memento.CaptionEmulateInFullScreen;
        }
        #endregion

    }


}
