#include "__shader_predefined__"
#include "Unity.inc.hlsl"

ToonyLighted VSMain(appdata_full v)
{
    ToonyLighted o;

    float2  r0;
    r0.x    = dot(v.normal.xyz, UNITY_MATRIX_IT_MV._m00_m01_m02);
    r0.y    = dot(v.normal.xyz, UNITY_MATRIX_IT_MV._m10_m11_m12);
    r0.xy   = r0.x * MATRIX_BASIS_X(UNITY_MATRIX_P).xy + r0.y * MATRIX_BASIS_Y(UNITY_MATRIX_P).xy;

    float4  r1;
    r1      = mul(UNITY_MATRIX_MVP, v.vertex);
    r1.xy  += r0.xy * clamp(r1.z, 0.0, 0.7) * _OutlineWidth;

    o.position  = r1;
    o.texCoord  = v.texcoord;
    o.viewDir   = mul(_World2Object, _WorldSpaceLightPos0).xyz;
    o.normal    = v.normal.xyz;
    o.color     = float3(0,0,0);

    return o;
}
