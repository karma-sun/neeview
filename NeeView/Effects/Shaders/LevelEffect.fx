sampler2D input : register(S0);  
float black : register(C0);
float white : register(C1);
float center: register(C2);
float hue : register(C3);


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

  // hue
  hsv.x += hue;

  // level
  //hsv.z = (hsv.z - black) / (white - black); 
  float gray = (white - black) * center + black;
  hsv.z = (hsv.z < center? (hsv.z - black) / (gray- black) : (hsv.z - gray) / (white - gray) + 1 ) * 0.5;
  
  color.rgb = hsv2rgb(hsv);
  
  return color;
}



