using System.Linq;

namespace NeeView.Media.Imaging.Metadata
{
    public class ExifRating
    {
        private int _value;

        public ExifRating(int value)
        {
            _value = value;
        }

        public string ToFormatString()
        {
            return new string(Enumerable.Range(1, 5).Select(e => e <= _value ? '★' : '☆').ToArray());
        }

        public override string ToString()
        {
            return ToFormatString();
        }
    }

}
