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

    o.o5.x      = 0.5 * (o.position.w + o.position.x);
    o.o5.y      = 0.5 * (o.position.w + o.position.y * _ProjectionParams.x);
    o.o5.zw     = o.position.zw;

    return o;
}
