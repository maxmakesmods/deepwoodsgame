#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix ViewProjection;

float3 AmbientLightColor;
float4 Lights[8];
float2 LightPositions[8];
int NumLights;

int IsShadow;
float ShadowSkew;
float ShadowStrength;

float2 ObjectTextureSize;
float CellSize;

float4 ShadowMapBounds;
float2 ShadowMapTileSize;

sampler2D SpriteTextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
    MinFilter = POINT;
    MagFilter = POINT;
    MipFilter = POINT;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

sampler2D ShadowMapSampler = sampler_state
{
    Texture = <ShadowMap>;
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
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float2 WorldPos : TEXCOORD1;
    float IsGlowing : TEXCOORD2;
    float RowIndex : TEXCOORD3;
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

    float tex_x = input.TexRect.x / ObjectTextureSize.x;
    float tex_y = input.TexRect.y / ObjectTextureSize.y;
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

	return output;
}

float calcDistSqrd(float2 p1, float2 p2)
{
    return (p2.x - p1.x) * (p2.x - p1.x) + (p2.y - p1.y) * (p2.y - p1.y);
}

float3 applyLights(float2 pos, float3 color)
{
    float3 light = AmbientLightColor;

    for (int i = 0; i < NumLights; i++)
    {
        float distSqrd = calcDistSqrd(pos, LightPositions[i]);
        float maxDistSqrd = Lights[i].a * Lights[i].a;
        float strength = clamp(1.0 - distSqrd / maxDistSqrd, 0.0, 1.0);
        light += Lights[i].rgb * strength;
    }

    return color * light;
}

float3 applyShadows(float2 pos, float rowIndex, float3 color)
{
    pos.x = pos.x + ShadowSkew * (pos.y - rowIndex);

    if (pos.x < ShadowMapBounds.x || pos.x > ShadowMapBounds.z
        || pos.y < ShadowMapBounds.y || pos.y > ShadowMapBounds.w)
    {
        return color;
    }

    float2 shadowMapUV = (pos - ShadowMapBounds.xy) / ShadowMapTileSize;
    float shadow = tex2D(ShadowMapSampler, float2(shadowMapUV.x, 1.0 - shadowMapUV.y)).r;

    if (!shadow)
    {
        return color;
    }
    
    float shadowRowIndex = shadow - 1.0;
    if (rowIndex < shadowRowIndex + 0.01)
    {
        return color;
    }

    return color * (1.0 - ShadowStrength);
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 color = tex2D(SpriteTextureSampler, input.TexCoord);
    clip(-step(color.a, 0.1));
    if (IsShadow)
    {
        clip(-step(color.a, 0.9));
        return float4(input.RowIndex + 1.0, 0.0, 0.0, 1.0);
    }
    else if (input.IsGlowing)
    {
        return float4(color.rgb * 1.5, color.a);
    }
    else
    {
        float3 litColor = applyLights(input.WorldPos, color.rgb);
        float3 shadowedLitColor = applyShadows(input.WorldPos, input.RowIndex, litColor);
        return float4(shadowedLitColor, color.a);
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