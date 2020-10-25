using NeeLaboratory.ComponentModel;
using NeeView.Windows;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml.XPath;

namespace NeeView.Runtime.LayoutPanel
{
    public class LayoutPanel : IHasDragGhost
    {
        public LayoutPanel(string key)
        {
            Key = key;
        }

        public string Key { get; set; }
        public string Title { get; set; }
        public FrameworkElement DragGhost { get; set; }
        public object Content { get; set; }

        public GridLength GridLength { get; set; } = new GridLength(1, GridUnitType.Star);
        public WindowPlacement WindowPlacement { get; set; } = WindowPlacement.None;


        public override string ToString()
        {
            return Key ?? base.ToString();
        }

        #region support IHasDragGhost

        public FrameworkElement GetDragGhost()
        {
            return DragGhost;
        }

        #endregion support IHasDragGhost

        #region Memento

        public class Memento
        {
            public GridLength GridLength { get; set; }
            public WindowPlacement WindowPlacement { get; set; }
        }

        public Memento CreateMemento()
        {
            var memento = new Memento();
            memento.GridLength = GridLength;
            memento.WindowPlacement = WindowPlacement;
            return memento;
        }

        public void Restore(Memento memento)
        {
            if (memento == null) return;
            GridLength = memento.GridLength;
            WindowPlacement = memento.WindowPlacement;
        }


        #endregion
    }


}
