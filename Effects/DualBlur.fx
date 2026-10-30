sampler uImage0 : register(s0);

// Offsets are measured in half texels of the source texture for both passes.
float2 uHalfTexelSize;

float4 Downsample(float2 coords : TEXCOORD0) : COLOR0
{
    float2 h = uHalfTexelSize;
    float4 color = tex2D(uImage0, coords) * 4;
    color += tex2D(uImage0, coords + float2(-h.x, -h.y));
    color += tex2D(uImage0, coords + float2(h.x, -h.y));
    color += tex2D(uImage0, coords + float2(-h.x, h.y));
    color += tex2D(uImage0, coords + float2(h.x, h.y));
    return color / 8;
}

float4 Upsample(float2 coords : TEXCOORD0) : COLOR0
{
    float2 h = uHalfTexelSize;
    float4 color = tex2D(uImage0, coords + float2(-2 * h.x, 0));
    color += tex2D(uImage0, coords + float2(-h.x, h.y)) * 2;
    color += tex2D(uImage0, coords + float2(0, 2 * h.y));
    color += tex2D(uImage0, coords + float2(h.x, h.y)) * 2;
    color += tex2D(uImage0, coords + float2(2 * h.x, 0));
    color += tex2D(uImage0, coords + float2(h.x, -h.y)) * 2;
    color += tex2D(uImage0, coords + float2(0, -2 * h.y));
    color += tex2D(uImage0, coords + float2(-h.x, -h.y)) * 2;
    return color / 12;
}

technique Technique1
{
    pass Downsample
    {
        PixelShader = compile ps_2_0 Downsample();
    }
    pass Upsample
    {
        PixelShader = compile ps_2_0 Upsample();
    }
}
