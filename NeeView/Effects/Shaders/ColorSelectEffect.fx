// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

sampler2D input : register(S0);  
float hue : register(C0);
float range : register(C1);
float curve : register(C2);

// RGB <-> HSV convert
// http://lolengine.net/blog/2013/07/27/rgb-to-hsv-in-glsl
float3 rgb2hsv(float3 c)
{
    float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
    float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return float3 (abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

float3 hsv2rgb(float3 c)
{
    float4 K = float4 (1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

//------------
// main
//------------

float4 main(float2 uv : TEXCOORD) : COLOR  
{
	float4 color = tex2D(input, uv);
	float3 hsv = rgb2hsv(color.rgb);

	float distance = abs(frac(frac(hue / 360 - hsv.x + 1) + 0.5) - 0.5);
	float rad = clamp(3.14 * (distance - range * 0.5) / curve, 0, 3.14);
	hsv.y = hsv.y * (cos(rad) * 0.5 + 0.5);

	color.rgb = hsv2rgb(hsv);
	return color;
}



