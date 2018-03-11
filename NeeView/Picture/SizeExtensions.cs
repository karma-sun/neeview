using System;
using System.Windows;

namespace NeeView
{
    public static class SizeExtensions
    {
        public static Size Zero = new Size(0, 0);

        // ゼロスケール判定
        public static bool IsZero(this Size self)
        {
            return self.Width == 0 || self.Height == 0;
        }

        // Empty,ゼロスケール判定
        public static bool IsEmptyOrZero(this Size self)
        {
            return self.IsEmpty || self.IsZero();
        }

        // スケール
        public static Size Multi(this Size self, double scale)
        {
            return new Size(self.Width * scale, self.Height * scale);
        }

        // 少数切り捨てサイズを返す(おおよそ)
        public static Size Truncate(this Size self)
        {
            if (self.IsEmpty) return self;

            return new Size((int)self.Width, (int)self.Height);
        }

        // 画像アスペクト比を保つ最大のサイズを返す
        public static Size Uniformed(this Size self, Size target)
        {
            if (self.IsEmpty || target.IsEmpty) return self;

            var rateX = self.Width / target.Width;
            var rateY = self.Height / target.Height;

            var scale = 1.0 / (rateX > rateY ? rateX : rateY);

            return new Size(self.Width * scale, self.Height * scale);
        }

        //
        public static bool IsContains(this Size self, Size target)
        {
            if (self.IsEmpty || target.IsEmpty) return false;

            return (target.Width <= self.Width && target.Height <= self.Height);
        }

        // 指定範囲内に収まるサイズを返す
        public static Size Limit(this Size self, Size target)
        {
            if (self.IsEmpty || target.IsEmpty || target.IsContains(self)) return self;

            return Uniformed(self, target);
        }

        // ほぼ同じサイズ？
        public static bool IsEqualMaybe(this Size self, Size target)
        {
            if (self.IsEmpty || target.IsEmpty) return false;

            // アスペクト比固定のため、Heightでのみ判定
            const double margin = 1.1;
            return Math.Abs(self.Height - target.Height) < margin;
        }

        // 転置
        public static Size Transpose(this Size self)
        {
            if (self.IsEmpty) return self;

            return new Size(self.Height, self.Width);
        }

        // Suze -> Drawing.Size
        public static System.Drawing.Size ToDrawingSize(this Size self)
        {
            if (self.IsEmpty) return System.Drawing.Size.Empty;

            return new System.Drawing.Size((int)self.Width, (int)self.Height);
        }

        // Drawing.Size -> Size
        public static Size FromDrawingSize(System.Drawing.Size size)
        {
            if (size.IsEmpty) return Size.Empty;

            return new Size(size.Width, size.Height);
        }

        // Drawing.SizeF -> Size
        public static Size FromDrawingSize(System.Drawing.SizeF size)
        {
            if (size.IsEmpty) return Size.Empty;

            return new Size(size.Width, size.Height);
        }

        // 大きいサイズを返す
        public static Size Max(Size size0, Size size1)
        {
            return size0.Width * size0.Height < size1.Width * size1.Height ? size1 : size0;
        }

        // サイズがない場合の置き換え
        public static Size EmptyOrZeroCoalesce(this Size self, Size target)
        {
            return self.IsEmptyOrZero() ? target : self;
        }
    }

}
