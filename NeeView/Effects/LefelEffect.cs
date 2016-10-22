// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

using System;
using System.Windows.Media.Effects;
using System.Windows;
using System.Windows.Media;
using System.Reflection;

namespace NeeView.Effects
{
    public class LevelEffect : ShaderEffect
    {
        private static PixelShader s_pixelShader = new PixelShader()
        {
            UriSource = Tools.MakePackUri(typeof(LevelEffect).Assembly, "Effects/Shaders/LevelEffect.ps")
        };

        public LevelEffect()
        {
            PixelShader = s_pixelShader;

            UpdateShaderValue(InputProperty);
            UpdateShaderValue(BlackProperty);
            UpdateShaderValue(WhiteProperty);
            UpdateShaderValue(CenterProperty);
            UpdateShaderValue(HueProperty);
        }

        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(LevelEffect), 0);
        public Brush Input
        {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        //
        public static readonly DependencyProperty BlackProperty = DependencyProperty.Register("Black", typeof(double), typeof(LevelEffect), new UIPropertyMetadata(0.0, PixelShaderConstantCallback(0)));
        public double Black
        {
            get { return (double)GetValue(BlackProperty); }
            set { SetValue(BlackProperty, value); }
        }

        //
        public static readonly DependencyProperty WhiteProperty = DependencyProperty.Register("White", typeof(double), typeof(LevelEffect), new UIPropertyMetadata(1.0, PixelShaderConstantCallback(1)));
        public double White
        {
            get { return (double)GetValue(WhiteProperty); }
            set { SetValue(WhiteProperty, value); }
        }

        //
        public static readonly DependencyProperty CenterProperty = DependencyProperty.Register("Center", typeof(double), typeof(LevelEffect), new UIPropertyMetadata(0.5, PixelShaderConstantCallback(2)));
        public double Center
        {
            get { return (double)GetValue(CenterProperty); }
            set { SetValue(CenterProperty, value); }
        }

        //
        public static readonly DependencyProperty HueProperty = DependencyProperty.Register("Hue", typeof(double), typeof(LevelEffect), new UIPropertyMetadata(0.0, PixelShaderConstantCallback(3)));
        public double Hue
        {
            get { return (double)GetValue(HueProperty); }
            set { SetValue(HueProperty, value); }
        }


#if false
        //
        private static object CoerceDesaturationFactor(DependencyObject d, object value)
        {
            GrayscaleEffect effect = (GrayscaleEffect)d;
            double newFactor = (double)value;

            if (newFactor < 0.0 || newFactor > 1.0)
            {
                return effect.DesaturationFactor;
            }

            return newFactor;
        }
#endif
    }
}
