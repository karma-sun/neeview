using System;
using System.Collections.Generic;
using System.Linq;

namespace NeeView
{
    public abstract class FolderTreeNode : TreeViewNodeBase
    {
        public abstract string Key { get; }


        /// <summary>
        /// 指定パスのFolderTreeINodeを取得
        /// </summary>
        /// <param name="path">指定パス</param>
        /// <param name="createChildren">まだ生成されていなければChildrenを生成する</param>
        /// <param name="asFarAsPossible">指定パスが存在しない場合、存在する上位フォルダーを返す</param>
        /// <returns></returns>
        public FolderTreeNode GetFolderTreeNode(string path, bool createChildren, bool asFarAsPossible)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            var pathTokens = new Stack<string>(path.TrimEnd(LoosePath.Separator).Split(LoosePath.Separator).Reverse());
            return GetFolderTreeNode(pathTokens, createChildren, asFarAsPossible);
        }

        /// <summary>
        /// 指定パスのFolderTreeNodeを取得
        /// </summary>
        public FolderTreeNode GetFolderTreeNode(Stack<string> pathTokens, bool createChildren, bool asFarAsPossible)
        {
            if (pathTokens.Count == 0)
            {
                return this;
            }

            var token = pathTokens.Pop();

            if (_children == null && createChildren)
            {
                RefreshChildren();
            }

            var child = _children?.Cast<FolderTreeNode>().FirstOrDefault(e => e.Key == token);
            if (child != null)
            {
                return child.GetFolderTreeNode(pathTokens, createChildren, asFarAsPossible);
            }

            return asFarAsPossible ? this : null;
        }
    }
}
