sampler uImage0 : register(s0);
texture2D tex0;
sampler2D uColor = sampler_state
{
    Texture = <tex0>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};
float2 uScreenResolution;
float2 uPos;
float t;
float intensity;
float4 PSFunction(float2 coords:TEXCOORD0): COLOR0
{
    float2 pos = uPos; //中心的位置
    float2 offset = (coords - pos);
    float2 rpos = offset * float2(uScreenResolution.x / uScreenResolution.y, 1);
    float ls = length(rpos); //取距离
    //length到3时衰减为0
    float i = (3 - ls)/3;
    return tex2D(uColor, float2(t,0)) * i*intensity;
    
}
technique Technique1
{
    pass Light
    {
        PixelShader = compile ps_2_0 PSFunction();
    }
}