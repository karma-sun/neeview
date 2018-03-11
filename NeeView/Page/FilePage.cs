using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;

namespace NeeView
{
    /// <summary>
    /// ファイルページ
    /// </summary>
    public class FilePage : Page
    {
        public FilePage(ArchiveEntry entry, FilePageIcon icon, string message = null)
        {
            Entry = entry;
            Content = new FileContent(entry, icon, message);
        }
    }

}
