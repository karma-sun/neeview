using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    /// <summary>
    /// FileContent
    /// </summary>
    public class FileContent : PageContent
    {
        public FileContent(ArchiveEntry entry, FilePageIcon icon, string message) : base(entry)
        {
            PageMessage = new PageMessage()
            {
                Icon = icon,
                Message = message,
            };
        }
    }
}
