using NeeView.Native;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeeView
{


    class WicDecoders
    {

        /// <summary>
        /// Collect WIC Decoders
        /// </summary>
        /// <returns>friendlyName to fileExtensions dictionary</returns>
        public static Dictionary<string, string> ListUp()
        {
            var collection = new Dictionary<string, string>();

            var friendlyName = new StringBuilder(1024);
            var fileExtensions = new StringBuilder(1024);
            for (uint i = 0; Interop.NVGetImageCodecInfo(i, friendlyName, fileExtensions); ++i)
            {
                ////Debug.WriteLine($"{friendryName}: {fileExtensions}");
                var key = friendlyName.ToString();
                if (collection.ContainsKey(key))
                {
                    collection[key] = collection[key].TrimEnd(',') + ',' + fileExtensions.ToString().ToLower();
                }
                else
                {
                    collection.Add(key, fileExtensions.ToString().ToLower());
                }
            }
            Interop.NVCloseImageCodecInfo();
            Interop.NVFpReset();

            return collection;
        }
    }
}
