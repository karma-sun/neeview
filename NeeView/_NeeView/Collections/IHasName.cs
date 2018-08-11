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
            if (x.Name == null)
            {
                return y.Name == null ? 0 : -1;
            }
            else if (y.Name == null)
            {
                return 1;
            }
            else
            {
                return NativeMethods.StrCmpLogicalW(x.Name, y.Name);
            }
        }
    }
}
