using System;




namespace NeeView
{
    public class PlaceString
    {
        private string _path;

        public PlaceString(string path)
        {
            _path = path;
        }

        public override string ToString()
        {
            if (_path == null)
            {
                return "PC";
            }
            else if (_path.StartsWith("http://") || _path.StartsWith("https://"))
            {
                return new Uri(_path).Host;
            }
            else if (_path.StartsWith("data:"))
            {
                return Properties.Resources.WordEmbeddedImage;
            }
            else
            {
                return LoosePath.GetFileName(_path);
            }
        }
    }
}


