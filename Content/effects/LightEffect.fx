#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

#include "common.fxh"

matrix ViewProjection;

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;

    float2 WorldPos : TEXCOORD1;
    float4 LightColor : TEXCOORD2;
    float LightAnim : TEXCOORD3;
    float4 Rand : TEXCOORD4;
    float IsHidden : TEXCOORD5;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float2 WorldPos : TEXCOORD1;
    float4 LightColor : TEXCOORD2;
    float LightAnim : TEXCOORD3;
    float4 Rand : TEXCOORD4;
    float IsHidden : TEXCOORD5;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

    float rowIndex = input.WorldPos.y;
    float2 worldPos = input.WorldPos;

    float4x4 translation = float4x4(
        1.0, 0.0, 0.0, 0.0,
        0.0, 1.0, 0.0, 0.0,
        0.0, 0.0, 1.0, 0.0,
        worldPos.x, worldPos.y, 0.0, 1.0
    );

    float4x4 world = translation;
    
    float4 adjustedPos = float4(input.Position.xy * input.LightColor.a, input.Position.zw);

    float4x4 worldViewProjection = mul(world, ViewProjection);
    
    output.Position = mul(adjustedPos, worldViewProjection);
    output.TexCoord = input.TexCoord;
    output.WorldPos = mul(adjustedPos, world).xy;
    output.LightColor = input.LightColor;
    output.LightAnim = input.LightAnim;
    output.Rand = input.Rand;
    output.IsHidden = input.IsHidden;

    output.Position = output.Position * (1.0 - input.IsHidden);

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    clip(-input.IsHidden);
    float distSqrd = calcDistSqrd(float2(0,0), (input.TexCoord - float2(0.5, 0.5)) * 2);
    float strength = clamp(1.0 - distSqrd, 0.0, 1.0);
    //clip(strength - 0.1);
    return float4(input.LightColor.xyz, strength);

}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};