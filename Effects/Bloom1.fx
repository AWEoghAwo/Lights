sampler uImage0 : register(s0);
texture2D tex0;
sampler2D uImage1 = sampler_state
{
    Texture = <tex0>;
    MinFilter = Point;
    MagFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};
float m;
float luminance(float4 c)
{
    return max(c.r,max(c.g,c.b));
}
float luminance2(float4 c)
{
    return c.r*0.3+c.g*0.6+c.b*0.1;
}
float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0 //提取明度超过m的部分
{
    float4 c = tex2D(uImage0,coords);
    if (luminance(c) > m)
            return c;
        else
            return float4(0, 0, 0, 0);
}

float p;
float m2;
float2 uScreenResolution;
float4 Blend(float2 coords : TEXCOORD0):COLOR0
{
    float4 c1 = tex2D(uImage0, coords);
    

    float2 dxy = 1 / uScreenResolution.xy;
    float4 c2 = tex2D(uImage1, coords);
    float4 orig = c2;
    

    float lum = luminance2(c2);
    float3 alpha = pow(1-lum, p);
    alpha = clamp(alpha, 0, 1);

    float4 bloomLight = c1 * m2;
    bloomLight.rgb*=alpha;
    float4 returnColor = orig + bloomLight ;
    return returnColor;
}
technique Technique1
{
	pass Bloom
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
    pass Blend
    {
        PixelShader = compile ps_2_0 Blend();
    }
}