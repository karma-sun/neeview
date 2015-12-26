using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{
    public static class ModelContext
    {
        public static BookHistory BookHistory { get; set; }

        public static ArchiverManager ArchiverManager { get; set; }
        public static BitmapLoaderManager BitmapLoaderManager { get; set; }

        public static void Initialize()
        {
            BookHistory = new BookHistory();

            ArchiverManager = new ArchiverManager();
            BitmapLoaderManager = new BitmapLoaderManager();
        }
    }
}
