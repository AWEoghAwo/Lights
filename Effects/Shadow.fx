sampler uImage0 : register(s0);

texture2D tex0; //Pass1使用 传入做好的阴影
sampler2D uImage1 = sampler_state 
{
    Texture = <tex0>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Wrap; 
    AddressV = Wrap;
};

float2 uScreenResolution;
float m;
float uIntensity;
float2 uPos;
float uCloudDiff;
float uCloudMul;

float colorDiff(float4 c1, float4 c2)
{
    float3 c = c1.rgb - c2.rgb;
    return sqrt(dot(c, c));
}

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0//提取蓝色通道小于阈值m的部分
{
    float4 c = tex2D(uImage0,coords);
    float4 finalColor = float4(0, 0, 0, 0);
    //float2 offset = (coords - uPos) * float2(uScreenResolution.x / uScreenResolution.y, 1);
    if (c.b < m)
        finalColor = float4(1, 1, 1, pow(1 - c.b,0.75));

    return finalColor;
}

float4 UseShadow(float2 coords : TEXCOORD0) : COLOR0 //做减法
{
    float4 color = tex2D(uImage0,coords);
    color.rgb -= tex2D(uImage1, coords).rgb;
    return color;
}
technique Technique1
{
	pass GetShadow
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
    pass UseShadow
    {
        PixelShader = compile ps_2_0 UseShadow();
    }
}