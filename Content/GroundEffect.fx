#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float2 GridSize;
float CellSize;
float2 GroundTilesTextureSize;

sampler2D GroundTilesTextureSampler = sampler_state
{
    Texture = <GroundTilesTexture>;
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

matrix WorldViewProjection;

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
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

    output.Position = mul(input.Position, WorldViewProjection);
    output.Color = input.Color;
    output.Tex = input.Tex;

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    int gridX = int(input.Tex.x * GridSize.x);
    int gridY = int(input.Tex.y * GridSize.y);
    
    //float2 gridTextureTexelSize = (GridSize * 1.0 / CellSize);
    
    float2 gridTextureUV = float2(gridX / GridSize.x, gridY / GridSize.y);
    int groundType = int(tex2D(TerrainGridTextureSampler, gridTextureUV).r / 256.0 + 0.5);
    
    float x = frac(input.Tex.x * GridSize.x) * CellSize / GroundTilesTextureSize.x;
    float y = frac(input.Tex.y * GridSize.y) * CellSize / GroundTilesTextureSize.y;
    
    float4 color = tex2D(GroundTilesTextureSampler, float2(x, y) + float2(groundType / 8.0, 0.0));

    return color;//    input.Color * (step(0.9, y) + step(0.9, x));
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};