using System;
using System.Collections;
using System.Collections.Generic;

namespace NeeView
{
    /// <summary>
    /// ArchiveEntryTree用ノード
    /// </summary>
    public class ArchiveEntryTreeNode : IEnumerable<ArchiveEntryTreeNode>
    {
        public ArchiveEntryTreeNode(ArchiveEntryTreeNode parent, string name)
        {
            Parent = parent;
            Name = name;
            Children = new List<ArchiveEntryTreeNode>();

            if (Parent != null)
            {
                Parent.HasChild = true;
            }
        }

        public string Name { get; private set; }

        public DateTime CreationTime { get; set; }
        public DateTime LastWriteTime { get; set; }

        /// <summary>
        /// 小要素があるかのフラグ。
        /// 空フォルダー判定に使用
        /// </summary>
        public bool HasChild { get; set; }

        public ArchiveEntryTreeNode Parent { get; private set; }
        public List<ArchiveEntryTreeNode> Children { get; private set; }

        public string Path => LoosePath.Combine(Parent?.Path, Name);

        public IEnumerator<ArchiveEntryTreeNode> GetEnumerator()
        {
            yield return this;
            foreach (var child in Children)
            {
                foreach (var subChid in child)
                {
                    yield return subChid;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
