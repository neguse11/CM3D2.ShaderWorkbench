#include "__shader_predefined__"
#include "Unity.inc.hlsl"

float4 PSMain(ToonyLighted i) : SV_TARGET
{
    float4 r0;

    float llit = CM3D2_LitForLookup(i.viewDir, i.normal);
    float3 t = _ToonRamp.Sample(_ToonRamp_Sampler, llit).xyz;

    r0.xyz = t * _OutlineColor.xyz * _LightColor0.xyz + UNITY_LIGHTMODEL_AMBIENT.xyz;
    r0.xyz *= 2;
    r0.w = _LightColor0.w;

    return r0;
}
