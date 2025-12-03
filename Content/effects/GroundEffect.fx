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

matrix WorldViewProjection;
float2 GroundTilesTextureSize;
int BlurHalfSize;

sampler2D GroundTilesTextureSampler = sampler_state
{
    Texture = <GroundTilesTexture>;
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

sampler2D TerrainGridTextureSampler = sampler_state
{
    Texture = <TerrainGridTexture>;
    MinFilter = POINT;
    MagFilter = POINT;
    MipFilter = POINT;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 Tex : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 Tex : TEXCOORD0;
    float2 WorldPos : TEXCOORD1;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

    output.Position = mul(input.Position, WorldViewProjection);
    output.Color = input.Color;
    output.Tex = input.Tex;
    output.WorldPos = input.Position.xy;

	return output;
}

int getGroundType(float2 uv)
{
    float2 gridTexelSize = 1.0 / (GridSize * CellSize);
    uv = int2(uv / gridTexelSize) * gridTexelSize;
    
    float bluenoise1 = getRandomFromBlueNoise(uv / CellSize, BlueNoiseSineXOffset, BlueNoiseSineXChannel);
    float bluenoise2 = getRandomFromBlueNoise(uv / CellSize, BlueNoiseSineYOffset, BlueNoiseSineYChannel);

    float wavy_x = uv.x + (sin(uv.y * GridSize.x * 3.1415) / GridSize.x) * 0.25 * bluenoise1;
    float wavy_y = uv.y + (sin(uv.x * GridSize.y * 3.1415) / GridSize.y) * 0.25 * bluenoise2;

    int gridX = int(wavy_x * GridSize.x);
    int gridY = int(wavy_y * GridSize.y);

    float2 gridTextureUV = float2(gridX / GridSize.x, gridY / GridSize.y);
    int groundType = int(tex2D(TerrainGridTextureSampler, gridTextureUV).r / 256.0 + 0.5) - 1.0;
    
    return groundType;
}

float4 getGroundTypeColorAndGlow(float2 uv, int groundType)
{
    float x = frac(uv.x * GridSize.x) * CellSize / GroundTilesTextureSize.x;
    float y = (1.0 - frac(uv.y * GridSize.y)) * CellSize / GroundTilesTextureSize.y;

    // TODO: proper variants
    int numVariants = 4;

    float bluenoise = getRandomFromBlueNoise(uv / CellSize, BlueNoiseVariantOffset, BlueNoiseVariantChannel);
    int variantIndex = int(bluenoise * numVariants);

    float groundTypeColumn = float(groundType) / (GroundTilesTextureSize.x / CellSize);
    float groundTypeVariantRow = float(variantIndex) / (GroundTilesTextureSize.y / CellSize);
    float3 color = tex2D(GroundTilesTextureSampler, float2(x, y) + float2(groundTypeColumn, groundTypeVariantRow)).rgb;
    float glow = tex2D(GlowMapSampler, float2(x, y) + float2(groundTypeColumn, groundTypeVariantRow)).r;

    return float4(color, glow);
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 gridTexelSize = 1.0 / (GridSize * CellSize);

    int blurFullSize = BlurHalfSize * 2 + 1;
    
    float bluenoise = getRandomFromBlueNoise(input.Tex, BlueNoiseDitherOffset, BlueNoiseDitherChannel);
    int pixelIndex = int(bluenoise * (blurFullSize * blurFullSize));

    int pixelX = pixelIndex / blurFullSize;
    int pixelY = pixelIndex % blurFullSize;

    int groundType = getGroundType(input.Tex + float2(pixelX - BlurHalfSize, pixelY - BlurHalfSize) * gridTexelSize);
    clip(groundType);

    float2 uv = animateWater(input.Tex, groundType);
    float4 color_and_glow = getGroundTypeColorAndGlow(uv, groundType);
    
    float3 litColor = applyLights(input.WorldPos, color_and_glow.rgb, color_and_glow.a);
    float3 shadowedLitColor = applyShadows(input.WorldPos, litColor, color_and_glow.a, -1, 0);

    return float4(shadowedLitColor * (1.0 + color_and_glow.a * 0.5), 1.0);
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};