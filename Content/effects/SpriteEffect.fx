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
#include "common_FogLayer.fxh"

matrix ViewProjection;

int IsShadow;
float ShadowSkew;

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
    float ShaderAnim : TEXCOORD6;
    float4 Rand : TEXCOORD7;
    float IsHidden : TEXCOORD8;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float2 WorldPos : TEXCOORD1;
    float IsGlowing : TEXCOORD2;
    float RowIndex : TEXCOORD3;
    float ShaderAnim : TEXCOORD4;
    float IsHidden : TEXCOORD5;
    float4 UVBounds : TEXCOORD6;
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
    
    float rowIndex = input.WorldPos.y;
    float2 worldPos = applyVertexShaderAnimation(input.WorldPos, input.Rand, int(input.ShaderAnim + 0.5));
    if (input.IsStanding)
    {
        worldPos.y += 0.25;
    }

    float4x4 translation = float4x4(
        1.0, 0.0, 0.0, 0.0,
        0.0, 1.0, 0.0, 0.0,
        0.0, 0.0, 1.0, 0.0,
        worldPos.x, worldPos.y, 0.0, 1.0
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
    output.ShaderAnim = input.ShaderAnim;
    output.IsHidden = input.IsHidden;
    
    output.Position = output.Position * (1.0 - input.IsHidden);
    
    output.UVBounds = float4(tex_x, tex_y, tex_x + tex_width, tex_y + tex_height);

	return output;
}

float checkBounds(float2 uv, float4 uvBounds)
{
    if (uv.x < uvBounds.x || uv.x > uvBounds.z
     || uv.y < uvBounds.y || uv.y > uvBounds.w)
    {
        return -1;
    }
    else
    {
        return 1;
    }
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    clip(-input.IsHidden);

    float fogValue = getFogValue(input.WorldPos / GridSize);
    
    int shaderAnim = int(input.ShaderAnim + 0.5);

    float2 uv = applyPixelShaderUVAnimation(input.TexCoord, input.UVBounds, shaderAnim);

    clip(checkBounds(uv, input.UVBounds));

    float4 color = tex2D(SpriteTextureSampler, uv);
    clip(-step(color.a, 0.1));
    if (IsShadow)
    {
        clip(-step(color.a, 0.9));
        return fogValue * float4(input.RowIndex + 1.0, 0.0, 0.0, 1.0);
    }
    else
    {
        float glow = tex2D(GlowMapSampler, uv).r * input.IsGlowing;

        float3 litColor = applyLights(input.WorldPos, color.rgb, glow);
        float3 shadowedLitColor = applyShadows(input.WorldPos, litColor, glow, input.RowIndex, ShadowSkew);

        float3 finalColor = applyPixelShaderColorAnimation(uv, input.UVBounds, shadowedLitColor, glow, shaderAnim);

        return fogValue * float4(finalColor, color.a);
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