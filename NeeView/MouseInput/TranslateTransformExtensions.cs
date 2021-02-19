using System.Windows;
using System.Windows.Media;

namespace NeeView
{
    public static class TranslateTransformExtensions
    {
        public static Point GetPoint(this TranslateTransform self)
        {
            return new Point(self.X, self.Y);
        }

        public static void SetPoint(this TranslateTransform self, Point point)
        {
            self.X = point.X;
            self.Y = point.Y;
        }
    }
}
