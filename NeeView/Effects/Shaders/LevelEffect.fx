// Copyright (c) 2016 Mitsuhiro Ito (nee)
//
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php

sampler2D input : register(S0);
float black : register(C0);
float white : register(C1);
float center: register(C2);
float minimum : register(C3);
float maximum : register(C4);

// ƒyƒWƒF‹Èü(1ŸŒ³, 0-1ŒÀ’è)
float bezier(float p2, float t)
{
	return 2 * (1 - t) * t * p2 + t * t;
}

//------------
// main
//------------

float4 main(float2 uv : TEXCOORD) : COLOR  
{
  float4 color = tex2D(input, uv);
   
  float t;
  float rate;

  color.r = lerp(black, white, bezier(center, color.r));
  color.g = lerp(black, white, bezier(center, color.g));
  color.b = lerp(black, white, bezier(center, color.b));

  color.rgb = lerp(minimum, maximum, clamp(color.rgb, 0, 1)) * color.a;

  return color;
}



