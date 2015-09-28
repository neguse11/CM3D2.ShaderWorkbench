#include "__shader_predefined__"
#define USE_WORLD_SPACE_VIEW_DIR
#define USE_SHADE_SH
#include "Unity.inc.hlsl"

ToonyLighted VSMain(appdata_full v)
{
    ToonyLighted o;

    o.position  = mul(UNITY_MATRIX_MVP, v.vertex);
    o.texCoord  = TRANSFORM_TEX(v.texcoord, _MainTex);
    o.viewDir   = WorldSpaceViewDir(v.vertex);
    o.normal    = mul((float3x3) _Object2World, SCALED_NORMAL);
    o.color     = ShadeSH9(float4(o.normal, 1));

    o.color     += Shade4PointLights(
        unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
        unity_LightColor0, unity_LightColor1, unity_LightColor2, unity_LightColor3,
        unity_4LightAtten0,
        o.viewDir,
        o.normal
    );

    return o;
}
