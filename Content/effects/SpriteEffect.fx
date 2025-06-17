#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix World;
matrix WorldViewProjection;

float3 AmbientLightColor;
float4 Lights[8];
float2 LightPositions[8];
int NumLights;

int IsShadow;
float ShadowSkew;
float ShadowStrength;

sampler2D SpriteTextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
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
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float2 WorldPos : TEXCOORD1;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

    if (IsShadow)
    {
        float x = input.Position.x + ShadowSkew * input.Position.y;
        float y = input.Position.y * 1.25;
        output.Position = mul(float4(x, y, input.Position.z, input.Position.w), WorldViewProjection);
    }
    else
    {
        output.Position = mul(input.Position, WorldViewProjection);
    }

    output.TexCoord = input.TexCoord;
    output.WorldPos = mul(input.Position, World).xy;

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

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 color = tex2D(SpriteTextureSampler, input.TexCoord);
    if (IsShadow)
    {
        return float4(0.0, 0.0, 0.0, 0.5 * color.a * ShadowStrength);
    }
    else
    {
        return float4(applyLights(input.WorldPos, color.rgb), color.a);
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