using System.Collections.Generic;

namespace NeeView
{
    public class ExternalAppCollection : List<ExternalApp>
    {
        public ExternalAppCollection()
        {
        }

        public ExternalAppCollection(IEnumerable<ExternalApp> collection) : base(collection)
        {
        }
    }


}
