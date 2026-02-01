
#ifndef DEEPWOODS_COMMON
#define DEEPWOODS_COMMON

float2 GridSize;
float CellSize;
float GlobalTime;
float2 ObjectTextureSize;
float2 GroundTilesTextureSize;

float calcDistSqrd(float2 p1, float2 p2)
{
    return (p2.x - p1.x) * (p2.x - p1.x) + (p2.y - p1.y) * (p2.y - p1.y);
}

#endif