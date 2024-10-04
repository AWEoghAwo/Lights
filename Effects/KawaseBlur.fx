sampler uImage0 : register(s0);

float2 uScreenResolution;
float m;
float uIntensity;
int distance;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0//提取亮度超过阈值m的部分
{
    float4 c = tex2D(uImage0,coords);
    if (max(max(c.r, c.g),c.b)>m)
        return c;
    else
        return float4(0,0,0,0);
}
float4 Blur(float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = float4(0, 0, 0, 1);
    float2 dxy = 1 / uScreenResolution.xy;
    float d = distance + 0.5;
    color.rgb += tex2D(uImage0,coords + dxy * float2(1,1) * distance);
    color.rgb += tex2D(uImage0,coords + dxy * float2(-1,1) * distance);
    color.rgb += tex2D(uImage0,coords + dxy * float2(1,-1) * distance);
    color.rgb += tex2D(uImage0,coords + dxy * float2(-1,-1) * distance);
    color.rgb *= uIntensity * 0.25;
    return color;
}
technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_2_0 Blur();
    }
}