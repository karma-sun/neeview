// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace NeeView
{
    /// <summary>
    /// 分数表現
    /// </summary>
    public class Fraction
    {
        public Fraction(int numerator, int denominator)
        {
            this.Numerator = numerator;
            this.Denominator = denominator;
        }

        public int Numerator { get; private set; } // 分子
        public int Denominator { get; private set; } // 分母

        public double Value => (double)Numerator / Denominator;

        // 約分
        public void Reduction()
        {
            int gcd = GreatestCommonDivisor(Numerator, Denominator);
            Numerator /= gcd;
            Denominator /= gcd;
        }

        // 最大公約数
        private int GreatestCommonDivisor(int x, int y)
        {
            while (true)
            {
                x = x % y;
                if (x == 0)
                    return y;
                y = y % x;
                if (y == 0)
                    return x;
            }
        }
    }

    /// <summary>
    /// EXIF アクセサ
    /// </summary>
    public class ExifAccessor
    {
        // Schema - https://msdn.microsoft.com/en-us/library/windows/desktop/ee719904(v=vs.85).aspx

        private BitmapMetadata _meta;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="meta">BitmapMetadata</param>
        public ExifAccessor(BitmapMetadata meta)
        {
            _meta = meta;
        }

        //
        private object GetExifParam(string query)
        {
            if (_meta.ContainsQuery(query))
            {
                return _meta.GetQuery(query);
            }
            else
            {
                return null;
            }
        }

        //
        private ushort GetExifParamUShort(string query)
        {
            try
            {
                var value = GetExifParam(query);
                if (value != null)
                {
                    return (ushort)value;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return 0;
            }
        }

        //
        private Fraction GetExifParamRational(string query)
        {
            try
            {
                var value = GetExifParam(query);
                if (value != null)
                {
                    var data = (ulong)value;
                    var f = new Fraction((int)(data & 0xffffffff), (int)((data >> 32) & 0xffffffff));
                    return f;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }
        }

        //
        private string GetExifParamString(string query)
        {
            try
            {
                return (string)GetExifParam(query);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }
        }


        // 幅
        public int ImageWidth => GetExifParamUShort("/app1/ifd/exif:{uint=256}");

        // 高さ
        public int ImageHeight => GetExifParamUShort("/app1/ifd/exif:{uint=257}");

        // カメラメーカー
        public string Maker => GetExifParamString("/app1/ifd/exif:{uint=271}");

        // カメラモデル
        public string Model => GetExifParamString("/app1/ifd/exif:{uint=272}");

        // 画像の方向
        public ushort Orientation => GetExifParamUShort("/app1/ifd/exif:{uint=274}");

        // 変更日時
        public string DateTime => GetExifParamString("/app1/ifd/exif:{uint=306}");

        // ISO
        public ushort ISOSpeedRatings => GetExifParamUShort("/app1/ifd/exif/subifd:{uint=34855}");

        // 	露出時間（秒）
        public Fraction ExposureTime => GetExifParamRational("/app1/ifd/exif/subifd:{uint=33434}");

        // F値
        public Fraction FNumber => GetExifParamRational("/app1/ifd/exif/subifd:{uint=33437}");

        // レンズの焦点距離（mm）
        public Fraction FocalLength => GetExifParamRational("/app1/ifd/exif/subifd:{uint=37386}");
    }
}
