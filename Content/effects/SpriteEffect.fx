#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

#include "common.fxh"
#include "common_lightsAndShadows.fxh"
#include "common_BlueNoise.fxh"
#include "common_Animations.fxh"

matrix ViewProjection;

int IsShadow;
float ShadowSkew;

float2 ObjectTextureSize;

sampler2D SpriteTextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
    MinFilter = POINT;
    MagFilter = POINT;
    MipFilter = POINT;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

sampler2D GlowMapSampler = sampler_state
{
    Texture = <GlowMap>;
    MinFilter = POINT;
    MagFilter = POINT;
    MipFilter = POINT;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;

    float2 WorldPos : TEXCOORD1;
    float4 TexRect : TEXCOORD2;
    float IsStanding : TEXCOORD3;
    float IsGlowing : TEXCOORD4;
    float3 AnimationData : TEXCOORD5;
    float IsHidden : TEXCOORD6;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float2 WorldPos : TEXCOORD1;
    float IsGlowing : TEXCOORD2;
    float RowIndex : TEXCOORD3;
    float IsHidden : TEXCOORD4;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

    float c = cos(20.0 * 3.1415 / 180.0);
    float s = sin(20.0 * 3.1415 / 180.0);
    
    float4x4 rotation = float4x4(
        1.0, 0.0, 0.0, 0.0,
        0.0,   c,   s, 0.0,
        0.0,  -s,   c, 0.0,
        0.0, 0.0, 0.0, 1.0
    );

    float4x4 translation = float4x4(
        1.0, 0.0, 0.0, 0.0,
        0.0, 1.0, 0.0, 0.0,
        0.0, 0.0, 1.0, 0.0,
        input.WorldPos.x, input.WorldPos.y, 0.0, 1.0
    );

    float4x4 world;
    if (IsShadow || !input.IsStanding)
    {
        world = translation;
    }
    else
    {
        world = mul(rotation, translation);
    }

    float animY = getAnimationY(input.AnimationData, input.TexRect);

    float tex_x = input.TexRect.x / ObjectTextureSize.x;
    float tex_y = animY / ObjectTextureSize.y;
    float tex_width = input.TexRect.z / ObjectTextureSize.x;
    float tex_height = input.TexRect.w / ObjectTextureSize.y;

    float obj_width = input.TexRect.z / CellSize;
    float obj_height = input.TexRect.w / CellSize;
    
    float4 adjustedPos = float4(input.Position.x * obj_width, input.Position.y * obj_height, input.Position.z, input.Position.w);
    float2 adjustedTexCoord = float2(tex_x + input.TexCoord.x * tex_width, tex_y + input.TexCoord.y * tex_height);

    float4x4 worldViewProjection = mul(world, ViewProjection);
    
    
    float rowIndex = input.WorldPos.y;
    
    
    if (IsShadow)
    {
        float y = adjustedPos.y * 1.25;
        float x = adjustedPos.x + ShadowSkew * y;
        float z = -0.01 * rowIndex;
        output.Position = mul(float4(x, y, z, 1.0), worldViewProjection);
    }
    else
    {
        output.Position = mul(adjustedPos, worldViewProjection);
    }

    output.TexCoord = adjustedTexCoord;
    output.WorldPos = mul(adjustedPos, world).xy;
    output.IsGlowing = input.IsGlowing;
    output.RowIndex = rowIndex;
    output.IsHidden = input.IsHidden;
    
    output.Position = output.Position * (1.0 - input.IsHidden);

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    clip(-input.IsHidden);
    
    float4 color = tex2D(SpriteTextureSampler, input.TexCoord);
    clip(-step(color.a, 0.1));
    if (IsShadow)
    {
        clip(-step(color.a, 0.9));
        return float4(input.RowIndex + 1.0, 0.0, 0.0, 1.0);
    }
    else
    {
        float glow = tex2D(GlowMapSampler, input.TexCoord).r * input.IsGlowing;
        
        float3 litColor = applyLights(input.WorldPos, color.rgb, glow);
        float3 shadowedLitColor = applyShadows(input.WorldPos, litColor, glow, input.RowIndex, ShadowSkew);
        return float4(shadowedLitColor * (1.0 + glow * 0.5), color.a);
    }
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};