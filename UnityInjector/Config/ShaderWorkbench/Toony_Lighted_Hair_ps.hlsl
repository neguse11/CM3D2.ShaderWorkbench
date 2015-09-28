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
    float4 hiColor_ = CM3D2_MultiColXyzw(_HiTex, _HiTex_Sampler, i.texCoord.xy);
    float3 hiColor = hiColor_.xyz * hiColor_.w;
    float dp0 = log(max(dot(i.normal, normalize(mad(_WorldSpaceLightPos0.xyz, rimLit, viewDir))), 0));
    float4 shadowTex = CM3D2_MultiColXyzw(_ShadowTex, _ShadowTex_Sampler, i.texCoord.xy);

    float4 mainTex = float4(_Color.xyz, 1) * CM3D2_MultiColXyzw(_MainTex, _MainTex_Sampler, i.texCoord.xy);

    float3 rxyz = mad(hiColor * exp(dp0 * _HiPow), _HiRate, mad(shadowTex.xyz, shadowToon * shadowTex.w, mainTex.xyz * mad(-shadowTex.w, shadowToon, 1.0)));
    float3 r1 = mad(-rxyz + _ShadowColor.xyz, -_ShadowMapTexture.Sample(_ShadowMapTexture_Sampler, i.o5.xy / i.o5.ww) + 1.0, rxyz);

    mainTex.xyz = mad(mainTex.xyz, i.color.xyz, mad(r1.xyz, toonColor, exp(dp0*48) * _Shininess));
    mainTex.w = 1.0;

    return mainTex;
}
