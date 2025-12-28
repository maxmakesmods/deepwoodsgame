
#ifndef DEEPWOODS_COMMON_DITHERING
#define DEEPWOODS_COMMON_DITHERING

#include "common.fxh"
#include "common_BlueNoise.fxh"

int BlurHalfSize;

float2 getDitheredUV(float2 uv)
{
    float2 gridTexelSize = 1.0 / (GridSize * CellSize);

    int blurFullSize = BlurHalfSize * 2 + 1;

    float bluenoise = getRandomFromBlueNoise(uv, BlueNoiseDitherOffset, BlueNoiseDitherChannel);
    int pixelIndex = int(bluenoise * (blurFullSize * blurFullSize));

    int pixelX = pixelIndex / blurFullSize;
    int pixelY = pixelIndex % blurFullSize;

    uv = uv + float2(pixelX - BlurHalfSize, pixelY - BlurHalfSize) * gridTexelSize;


    uv = int2(uv / gridTexelSize) * gridTexelSize;

    float bluenoise1 = getRandomFromBlueNoise(uv / CellSize, BlueNoiseSineXOffset, BlueNoiseSineXChannel);
    float bluenoise2 = getRandomFromBlueNoise(uv / CellSize, BlueNoiseSineYOffset, BlueNoiseSineYChannel);

    float wavy_x = uv.x + (sin(uv.y * GridSize.x * 3.1415) / GridSize.x) * 0.25 * bluenoise1;
    float wavy_y = uv.y + (sin(uv.x * GridSize.y * 3.1415) / GridSize.y) * 0.25 * bluenoise2;

    int gridX = int(wavy_x * GridSize.x);
    int gridY = int(wavy_y * GridSize.y);

    return float2(gridX / GridSize.x, gridY / GridSize.y);
}

#endif