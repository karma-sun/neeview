using System.Collections.Generic;

namespace NeeView.Collections
{
    public interface IHasName
    {
        string Name { get; }
    }

    public class NameComparer : IComparer<IHasName>
    {
        public int Compare(IHasName x, IHasName y)
        {
            return NativeMethods.StrCmpLogicalW(x.Name, y.Name);
        }
    }
}
