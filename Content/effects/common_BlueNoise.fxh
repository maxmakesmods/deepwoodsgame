
#ifndef DEEPWOODS_COMMON_BLUENOISE
#define DEEPWOODS_COMMON_BLUENOISE

#include "common.fxh"

sampler2D BlueNoiseTextureSampler = sampler_state
{
    Texture = <BlueNoiseTexture>;
    MinFilter = POINT;
    MagFilter = POINT;
    MipFilter = POINT;
    AddressU = WRAP;
    AddressV = WRAP;
};

float2 BlueNoiseTextureSize;
int BlueNoiseDitherChannel;
float2 BlueNoiseDitherOffset;
int BlueNoiseVariantChannel;
float2 BlueNoiseVariantOffset;
int BlueNoiseSineXChannel;
float2 BlueNoiseSineXOffset;
int BlueNoiseSineYChannel;
float2 BlueNoiseSineYOffset;

float getRandomFromBlueNoise(float2 uv, float2 texSize, float2 offset, int channel)
{
    float2 bluenoiseUV = int2(uv * texSize) / BlueNoiseTextureSize;
    float bluenoise = tex2D(BlueNoiseTextureSampler, bluenoiseUV + offset)[channel];
    return frac(bluenoise);
}

float getRandomFromBlueNoise(float2 uv, float2 offset, int channel)
{
    return getRandomFromBlueNoise(uv, GridSize * CellSize, offset, channel);
}

#endif