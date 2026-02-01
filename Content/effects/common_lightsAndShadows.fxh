
#ifndef DEEPWOODS_COMMON_LIGHTS_AND_SHADOWS
#define DEEPWOODS_COMMON_LIGHTS_AND_SHADOWS

#include "common.fxh"

float3 AmbientLightColor;
/*
float4 Lights[8];
float2 LightPositions[8];
int NumLights;
*/

float4 ShadowMapBounds;
float2 ShadowMapTileSize;
float ShadowStrength;

sampler2D ShadowMapSampler = sampler_state
{
    Texture = <ShadowMap>;
    MinFilter = POINT;
    MagFilter = POINT;
    MipFilter = POINT;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

sampler2D LightMapSampler = sampler_state
{
    Texture = <LightMap>;
    MinFilter = POINT;
    MagFilter = POINT;
    MipFilter = POINT;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

float3 getLightMapLights(float2 pos)
{
    if (pos.x < ShadowMapBounds.x || pos.x > ShadowMapBounds.z
        || pos.y < ShadowMapBounds.y || pos.y > ShadowMapBounds.w)
    {
        return float3(0, 0, 0);
    }
    float2 lightMapUV = (pos - ShadowMapBounds.xy) / ShadowMapTileSize;
    return tex2D(LightMapSampler, float2(lightMapUV.x, 1.0 - lightMapUV.y)).rgb;
}

float3 applyLights(float2 pos, float3 color, float glow)
{
    /*
    float3 light = AmbientLightColor;

    for (int i = 0; i < NumLights; i++)
    {
        float distSqrd = calcDistSqrd(pos, LightPositions[i]);
        float maxDistSqrd = Lights[i].a * Lights[i].a;
        float strength = clamp(1.0 - distSqrd / maxDistSqrd, 0.0, 1.0);
        light += Lights[i].rgb * strength;
    }
    
    return glow * color + (1.0 - glow) * color * light;
    */
    
    return glow * color + (1.0 - glow) * color * clamp(AmbientLightColor + getLightMapLights(pos), 0, 1);
}

float3 applyShadows(float2 pos, float3 color, float glow, float rowIndex, float shadowSkew)
{
    pos.x = pos.x + shadowSkew * (pos.y - rowIndex);

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
    if (rowIndex >= 0 && rowIndex < shadowRowIndex + 0.01)
    {
        return color;
    }

    return color * (1.0 - ShadowStrength * (1.0 - glow));
}

#endif