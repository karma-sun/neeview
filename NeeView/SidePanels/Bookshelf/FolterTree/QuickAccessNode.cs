using NeeLaboratory.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace NeeView
{
    public class QuickAccessNode : FolderTreeNodeBase
    {
        public QuickAccessNode(QuickAccess source, RootQuickAccessNode parent)
        {
            Source = source;
            Parent = parent;
        }

        public QuickAccess QuickAccessSource => (QuickAccess)Source;

        public override string Name { get => QuickAccessSource.Name; set { } }

        public override string DispName { get => Name; set { } }

        public override IImageSourceCollection Icon => PathToPlaceIconConverter.Convert(new QueryPath(QuickAccessSource.Path));


        public void Rename(string newName)
        {
            if (this.DispName != newName)
            {
                QuickAccessSource.Name = newName;
                RaisePropertyChanged(nameof(Name));
                RaisePropertyChanged(nameof(DispName));
            }
        }
    }
}
