using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NeeView
{
    /// <summary>
    /// FileContent
    /// </summary>
    public class FileContent : PageContent
    {
        public FileContent(ArchiveEntry entry, FilePageIcon icon, string message) : base(entry)
        {
            SetPageMessage(new PageMessage()
            {
                Icon = icon,
                Message = message,
            });
        }

        public override IContentLoader CreateContentLoader()
        {
            return new FileContentLoader(this);
        }
    }
}
