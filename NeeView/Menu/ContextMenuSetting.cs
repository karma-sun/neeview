using NeeLaboratory.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NeeView
{
    [DataContract]
    public class ContextMenuSetting : BindableBase
    {
        private ContextMenu _contextMenu;
        private bool _isDarty;

        [DataMember]
        private MenuTree _sourceTree;

        [DataMember]
        public int _Version { get; set; } = Config.Current.ProductVersionNumber;

        public ContextMenu ContextMenu
        {
            get
            {
                _contextMenu = this.IsDarty ? SourceTree.CreateContextMenu() : _contextMenu;
                _isDarty = false;
                return _contextMenu;
            }
        }

        public MenuTree SourceTree
        {
            get { return _sourceTree ?? MenuTree.CreateDefault(); }
            set
            {
                _sourceTree = value;
                _contextMenu = null;
                _isDarty = true;
            }
        }

        public bool IsDarty
        {
            get { return _isDarty || _contextMenu == null; }
            set { _isDarty = value; }
        }

        public ContextMenuSetting Clone()
        {
            var clone = (ContextMenuSetting)this.MemberwiseClone();
            clone._sourceTree = _sourceTree?.Clone();
            clone._contextMenu = null;
            return clone;
        }

        public void Validate()
        {
            _sourceTree?.Validate();
        }


        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (_sourceTree == null) return;

            // before 37.0
            if (_Version < Config.GenerateProductVersionNumber(37, 0, 0))
            {
                var renameMap = new Dictionary<string, string>()
                {
                    ["OpenApplicaion"] = "OpenExternalApp",
                    ["OpenFilePlace"] = "OpenExplorer",
                    ["Export"] = "ExportImageAs",
                    ["PrevFolder"] = "PrevBook",
                    ["NextFolder"] = "NextBook",
                    ["SetPageMode1"] = "SetPageModeOne",
                    ["SetPageMode2"] = "SetPageModeTwo",
                };

                foreach (var node in _sourceTree)
                {
                    if (node.MenuElementType == MenuElementType.Command)
                    {
                        if (renameMap.TryGetValue(node.CommandName, out string newName))
                        {
                            node.CommandName = newName;
                        }
                    }
                }
            }
        }
    }
}
