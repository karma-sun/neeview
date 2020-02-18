using System.Threading.Tasks;

namespace NeeView
{
    public interface IImageExporter
    {
        bool HasBackground { get; set; }


        ImageExporterContent CreateView();

        void Export(string path, bool isOverwrite, int qualityLevel);

        string CreateFileName();
    }
}