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

int BlueNoiseChannel;
float2 BlueNoiseOffset;
float2 BlueNoiseTextureSize;
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

sampler2D TerrainGridTextureSampler = sampler_state
{
    Texture = <TerrainGridTexture>;
    MinFilter = POINT;
    MagFilter = POINT;
    MipFilter = POINT;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

sampler2D BlueNoiseTextureSampler = sampler_state
{
    Texture = <BlueNoiseTexture>;
    MinFilter = POINT;
    MagFilter = POINT;
    MipFilter = POINT;
    AddressU = WRAP;
    AddressV = WRAP;
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

int getGroundType(float2 uv)
{
    int gridX = int(uv.x * GridSize.x);
    int gridY = int(uv.y * GridSize.y);

    float2 gridTextureUV = float2(gridX / GridSize.x, gridY / GridSize.y);
    int groundType = int(tex2D(TerrainGridTextureSampler, gridTextureUV).r / 256.0 + 0.5);
    
    return groundType;
}

float3 getGroundTypeColor(float2 uv, int groundType)
{
    float x = frac(uv.x * GridSize.x) * CellSize / GroundTilesTextureSize.x;
    float y = frac(uv.y * GridSize.y) * CellSize / GroundTilesTextureSize.y;
    
    float3 color = tex2D(GroundTilesTextureSampler, float2(x, y) + float2(groundType / 8.0, 0.0)).rgb;

    return color;
}

float getRandomFromBlueNoise(float2 uv)
{
    float2 bluenoiseUV = int2(uv * GridSize * CellSize) / BlueNoiseTextureSize;
    float bluenoise = tex2D(BlueNoiseTextureSampler, bluenoiseUV + BlueNoiseOffset)[BlueNoiseChannel];
    return bluenoise;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 gridTexelSize = 1.0 / (GridSize * CellSize);

    int blurFullSize = BlurHalfSize * 2 + 1;
    
    float bluenoise = getRandomFromBlueNoise(input.Tex);
    int pixelIndex = int(bluenoise * (blurFullSize * blurFullSize));

    int pixelX = pixelIndex / blurFullSize;
    int pixelY = pixelIndex % blurFullSize;

    int groundType = getGroundType(input.Tex + float2(pixelX - BlurHalfSize, pixelY - BlurHalfSize) * gridTexelSize);
    float3 color = getGroundTypeColor(input.Tex, groundType);

    return float4(color, 1.0);
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};