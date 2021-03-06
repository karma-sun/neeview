sampler2D input : register(S0);
float black : register(C0);
float white : register(C1);
float center: register(C2);
float minimum : register(C3);
float maximum : register(C4);

// ペジェ曲線(1次元, 0-1限定)
float bezier(float p2, float t)
{
	return 2 * (1 - t) * t * p2 + t * t;
}

// 偏り
float bias(float p2, float t)
{
	return t < p2 ? t * (1 - p2) / p2 : (t - p2) * p2 / (1 - p2) + (1 - p2);
}

//------------
// main
//------------

float4 main(float2 uv : TEXCOORD) : COLOR  
{
  float4 color = tex2D(input, uv);
   
  color.rgb = saturate((color.rgb - black) / (white - black));

  color.r = bias(center, color.r);
  color.g = bias(center, color.g);
  color.b = bias(center, color.b);

  color.rgb = lerp(minimum, maximum, color.rgb) * color.a;

  return color;
}



