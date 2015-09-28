#include "__shader_predefined__"
#define USE_MULTICOL
#include "Unity.inc.hlsl"

#define ALTER_SHADER 1

// http://gamedev.stackexchange.com/a/32688
float rand_1_05(in float2 uv)
{
    float2 noise = (frac(sin(dot(uv ,float2(12.9898,78.233)*2.0)) * 43758.5453));
    return abs(noise.x + noise.y) * 0.5;
}

float2 rand_2_10(in float2 uv) {
    float noiseX = (frac(sin(dot(uv, float2(12.9898,78.233) * 2.0)) * 43758.5453));
    float noiseY = sqrt(1 - noiseX * noiseX);
    return float2(noiseX, noiseY);
}

float2 rand_2_0004(in float2 uv)
{
    float noiseX = (frac(sin(dot(uv, float2(12.9898,78.233)      )) * 43758.5453));
    float noiseY = (frac(sin(dot(uv, float2(12.9898,78.233) * 2.0)) * 43758.5453));
    return float2(noiseX, noiseY) * 0.004;
}

float3 MakeNoisyNormal(float3 normal, float viewDir, float2 screen) {
    if(! ALTER_SHADER) {
        return normal;
    }

    float rn = rand_2_10(screen.xy).x;
    rn *= 0.15;
    normal = lerp(normal, viewDir, rn);
    normal = normalize(normal);

    return normal;
}

float3 PostLit(float2 screen, float3 litDir, float3 normal, float3 litColor) {
    float dn = dot(litDir, normal);

    float dr = pow((dn + 1) * 0.5, 0.5);
    float dg = pow((dn + 1) * 0.5, 2.0);
    float db = pow((dn + 1) * 0.5, 3.0);

    float3 dd = saturate(float3(dr, dg, db) * litColor);

    return dd;
}

float3 Specular(float3 litDir, float3 normal, float3 viewDir) {
    float dn = saturate(dot(viewDir, normal));
    return pow(dn, float3(20, 90, 140));// * float3(0.3, 0.4, 0.5)*0.2;
}

float4 PSMain(ToonyLighted i) : SV_TARGET
{
    float2 screen = i.position.xy;
    float3 normal = i.normal;
    float3 normal_ = normal;
    float3 lightDir = _WorldSpaceLightPos0.xyz;
    float3 viewDir = normalize(i.viewDir.xyz);

    normal = MakeNoisyNormal(normal, viewDir, screen);

    float rimLit = CM3D2_RimLit(viewDir, normal_, _RimShift, _RimPower);
    float llit = CM3D2_LitForLookup(_WorldSpaceLightPos0.xyz, normal);
    float shadowToon = 1 - _ShadowRateToon.Sample(_ShadowRateToon_Sampler, llit).x;
    float3 toonColor = CM3D2_MultiColXyz(_ToonRamp, _ToonRamp_Sampler, llit).xyz * _LightColor0.xyz;
    float dp0 = log(max(dot(normal, normalize(mad(lightDir, rimLit, viewDir))), 0));
    float4 shadowTex = CM3D2_MultiColXyz(_ShadowTex, _ShadowTex_Sampler, i.texCoord.xy);

    float4 mainTex = _Color.xyzw * CM3D2_MultiColXyz(_MainTex, _MainTex_Sampler, i.texCoord.xy);
    mainTex.xyz = mad(_RimColor.xyz, rimLit, mainTex);

    float srx = (1 - shadowToon * shadowTex.w);
    float3 sr = float3(srx, srx, srx);
    if(ALTER_SHADER) {
        sr = PostLit(screen, lightDir, normal, _LightColor0.xyz);
    }

    float3 tc = toonColor * lerp(shadowTex.xyz, mainTex.xyz, sr.xyz);
    mainTex.xyz = mainTex.xyz * i.color.xyz + tc + exp(dp0*48) * _Shininess;

    if(ALTER_SHADER) {
        mainTex.xyz += Specular(lightDir, normal, viewDir) * (_Shininess + float3(0.05, 0.05, 0.05));
        mainTex.xyz += pow(saturate(-dp0), 2) * (_Shininess + float3(0.05, 0.05, 0.05));
    }

    return mainTex;
}
