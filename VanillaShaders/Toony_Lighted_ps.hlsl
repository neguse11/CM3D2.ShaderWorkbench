#include "__shader_predefined__"
#define USE_MULTICOL
#include "Unity.inc.hlsl"

float4 PSMain(ToonyLighted i) : SV_TARGET
{
    float3 viewDir = normalize(i.viewDir.xyz);
    float rimLit = CM3D2_RimLit(viewDir, i.normal, _RimShift, _RimPower);
    float llit = CM3D2_LitForLookup(_WorldSpaceLightPos0.xyz, i.normal);
    float shadowToon = 1 - _ShadowRateToon.Sample(_ShadowRateToon_Sampler, llit).x;
    float3 toonColor = CM3D2_MultiColXyz(_ToonRamp, _ToonRamp_Sampler, llit).xyz * _LightColor0.xyz;
    float dp0 = log(max(dot(i.normal, normalize(mad(_WorldSpaceLightPos0.xyz, rimLit, viewDir))), 0));
    float4 shadowTex = CM3D2_MultiColXyz(_ShadowTex, _ShadowTex_Sampler, i.texCoord.xy);

    float4 mainTex = _Color.xyzw * CM3D2_MultiColXyz(_MainTex, _MainTex_Sampler, i.texCoord.xy);
    mainTex.xyz = mad(_RimColor.xyz, rimLit, mainTex).xyz;

    float3 r2 = mad(shadowTex.xyz, shadowToon * shadowTex.w, mainTex.xyz * mad(-shadowTex.w, shadowToon, 1.0));
    mainTex.xyz = mad(mainTex.xyz, i.color.xyz, mad(r2, toonColor, exp(dp0*48) * _Shininess));

    return mainTex;
}
