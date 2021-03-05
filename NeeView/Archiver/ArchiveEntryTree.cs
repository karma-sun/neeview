using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NeeView
{
    /// <summary>
    /// ArchiveEntryのフォルダー構造作成。
    /// ArchiveEntryを追加することによって階層を構成していく。
    /// 階層構造のみ、ArchiveEntryのリーフは持たない。
    /// </summary>
    public class ArchiveEntryTree
    {
        ArchiveEntryTreeNode _root;

        public ArchiveEntryTree()
        {
            _root = new ArchiveEntryTreeNode(null, null);
        }

        public void AddRange(IEnumerable<ArchiveEntry> entries)
        {
            foreach (var entry in entries)
            {
                Add(entry);
            }
        }

        public void Add(ArchiveEntry entry)
        {
            var path = entry.IsDirectory ? entry.EntryName : LoosePath.GetDirectoryName(entry.EntryName);
            var parts = LoosePath.Split(path);

            var node = _root;
            foreach (var part in parts)
            {
                var child = node.Children?.FirstOrDefault(e => e.Name == part);
                if (child == null)
                {
                    child = new ArchiveEntryTreeNode(node, part);
                    node.Children.Add(child);
                }

                if (!entry.IsDirectory)
                {
                    child.HasChild = true;
                }

                child.CreationTime = entry.CreationTime;
                child.LastWriteTime = entry.LastWriteTime;

                node = child;
            }
        }

        public List<ArchiveEntryTreeNode> GetDirectories()
        {
            return _root.Where(e => e != _root).ToList();
        }

        [Conditional("DEBUG")]
        public void Dump()
        {
            Debug.WriteLine("");
            foreach (var dir in GetDirectories())
            {
                Debug.WriteLine($"Directory: {dir.Path}: {dir.HasChild}");
            }
        }
    }
}
