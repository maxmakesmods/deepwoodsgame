
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

float2 applyVertexShaderAnimation(float2 worldPos, float4 randvalues, int ShaderAnim)
{
    if (ShaderAnim == 2) // Movy
    {
        float totalTime = frac(GlobalTime / 8);
        float time;
        float rand;
        
        if (totalTime < 0.25)
        {
            time = 1.0 - abs((totalTime * 4) * 2 - 1);
            rand = randvalues.x;
        }
        else if (totalTime < 0.5)
        {
            time = 1.0 - abs(((totalTime - 0.25) * 4) * 2 - 1);
            rand = randvalues.y;
        }
        else if (totalTime < 0.75)
        {
            time = 1.0 - abs(((totalTime - 0.5) * 4) * 2 - 1);
            rand = randvalues.z;
        }
        else
        {
            time = 1.0 - abs(((totalTime - 0.75) * 4) * 2 - 1);
            rand = randvalues.w;
        }
        
        worldPos.x = worldPos.x + time * cos(rand * 2 * 3.1415) * 0.02;
        worldPos.y = worldPos.y + time * sin(rand * 2 * 3.1415) * 0.02;
    }
    return worldPos;
}

float2 applyPixelShaderAnimation(float2 uv, float4 uvBounds, int ShaderAnim)
{
    if (ShaderAnim == 1) // Wavy
    {
        float2 cellUVSize = 1 / ObjectTextureSize * CellSize;
        float time = abs(frac(GlobalTime * 0.1) * 2 - 1) * 2 - 1;
        
        float yvalue = frac((GlobalTime * 0.1) + uv.y * ObjectTextureSize.y / CellSize);
        
        yvalue = cos(yvalue * 2 - 1);
        
        
        float xOffset = yvalue * cellUVSize.x * 0.125;
        uv.x = uv.x + xOffset;
    }

    return uv;
}

#endif