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
texture2D maskTexture;
sampler2D maskSampler = sampler_state
{
    Texture = <maskTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};
float2 uMaskOffset;
float4 KawaseMask(float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords + float2(-uMaskOffset.x, -uMaskOffset.y));
    color += tex2D(uImage0, coords + float2(uMaskOffset.x, -uMaskOffset.y));
    color += tex2D(uImage0, coords + float2(-uMaskOffset.x, uMaskOffset.y));
    color += tex2D(uImage0, coords + float2(uMaskOffset.x, uMaskOffset.y));
    return color * 0.25;
}
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
    return c * smoothstep(-0.23,0.23,luminance(c) - m);
}

float p;
float m2;
float4 Blend(float2 coords : TEXCOORD0) : COLOR0
{
    float4 original = tex2D(uImage1, coords);
    float3 bloomLight = tex2D(uImage0, coords).rgb * m2;
    float3 mask = tex2D(maskSampler, coords).rgb;
    float brightness = max(mask.r, max(mask.g, mask.b));

    float attenuation1 = pow(saturate(1 - brightness), p);
    float attenuation2 = 1 - smoothstep(0.0, 0.75, brightness);
    float3 result = original.rgb + bloomLight * lerp(attenuation1,attenuation2, 0.9 - brightness);
    return float4(result,original.a);
}

technique Technique1
{
    pass KawaseMask
    {
        PixelShader = compile ps_2_0 KawaseMask();
    }
	pass Bloom
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
    pass Blend
    {
        PixelShader = compile ps_2_0 Blend();
    }
}
