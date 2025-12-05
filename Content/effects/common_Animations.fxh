
#ifndef DEEPWOODS_COMMON_ANIMATIONS
#define DEEPWOODS_COMMON_ANIMATIONS

#include "common.fxh"

sampler2D DUDVTextureSampler = sampler_state
{
    Texture = <DUDVTexture>;
    MinFilter = POINT;
    MagFilter = POINT;
    MipFilter = POINT;
    AddressU = WRAP;
    AddressV = WRAP;
};

float getAnimationY(float3 animationData, float4 texRect)
{
    int animFrame = 0;
    if (animationData.x > 0)
    {
        animFrame = int(fmod(animationData.z * GlobalTime, max(animationData.x, 1)));
    }
    return texRect.y + animFrame * animationData.y;
}

float2 animateWater(float2 uv, int groundType)
{
    if (groundType == 8 || groundType == 9 || groundType == 23)
    {
        float time = abs(frac(GlobalTime * 0.01) * 2 - 1);
        
        float movespeed = 0;
        float scale = 10;
        if (groundType == 23)
        {
            scale = 1;
            movespeed = 0.1;
        }

        float2 dudv = tex2D(DUDVTextureSampler, uv * scale + time).xy * 0.2;

        float moveX = GlobalTime * movespeed;
        float moveY = GlobalTime * movespeed;

        return float2(uv.x + dudv.x / GridSize.x + moveX / GridSize.x, uv.y + dudv.y / GridSize.y + moveY / GridSize.y);
    }

    return uv;
}

float2 applyShaderAnimation(float2 uv, int ShaderAnim)
{
    if (ShaderAnim == 1)
    {
        float yvalue = uv.y * ObjectTextureSize.y;// / CellSize;
        float2 cellUVSize = 1 / ObjectTextureSize * CellSize;
        float time = abs(frac((yvalue + GlobalTime) * 0.1) * 2 - 1) * 2 - 1;
        
        float xOffset = time * cellUVSize.x * 0.125;
        uv.x = uv.x + xOffset;
    }
    
    return uv;
}

#endif