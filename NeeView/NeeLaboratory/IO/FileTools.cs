using System.IO;
using System.Threading.Tasks;

namespace NeeLaboratory.IO
{
    public static class FileTools
    {
        public static async Task<byte[]> ReadAllBytesAsync(string path)
        {
            byte[] result;
            using (FileStream stream = File.Open(path, FileMode.Open))
            {
                result = new byte[stream.Length];
                await stream.ReadAsync(result, 0, (int)stream.Length);
            }

            return result;
        }

        public static async Task WriteAllBytesAsync(string path, byte[] bytes)
        {
            using (FileStream stream = File.Open(path, FileMode.Create, FileAccess.Write))
            {
                await stream.WriteAsync(bytes, 0, bytes.Length);
            }
        }
    }

}
