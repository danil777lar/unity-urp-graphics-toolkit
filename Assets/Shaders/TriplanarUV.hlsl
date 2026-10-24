inline float3 TriplanarWeights(float3 n, float sharpness)
{
    n = abs(normalize(n));
    n = pow(n, sharpness);
    float s = max(n.x + n.y + n.z, 1e-5);
    return n / s;
}

void TriplanarUV_float(float3 positionWS, float3 normalWS, float tiling, float sharpness, float2 offset,
    out float2 uvX,
    out float2 uvY,
    out float2 uvZ,
    out float3 weights,
    out float2 uvBlended
)
{
    float3 p = positionWS * tiling;
    uvX = p.zy + offset;
    uvY = p.xz + offset;
    uvZ = p.xy + offset;
    weights = TriplanarWeights(normalWS, sharpness);
    uvBlended = uvX * weights.x + uvY * weights.y + uvZ * weights.z;
}