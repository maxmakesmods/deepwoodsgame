
#ifndef DEEPWOODS_COMMON_FOGLAYER
#define DEEPWOODS_COMMON_FOGLAYER

#include "common.fxh"
#include "common_Dithering.fxh"

sampler2D TerrainFogLayerSampler = sampler_state
{
    Texture = <TerrainFogLayer>;
    MinFilter = POINT;
    MagFilter = POINT;
    MipFilter = POINT;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

float getFogValue(float2 worldUV)
{
    float2 ditheredUV = getDitheredUV(worldUV);
    return tex2D(TerrainFogLayerSampler, ditheredUV).r;
}

#endif