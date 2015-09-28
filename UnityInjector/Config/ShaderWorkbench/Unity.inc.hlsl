//--------------------------------------------------------------------------------------
// Globals
//--------------------------------------------------------------------------------------
/*
#ifdef CBUF_GLOBALS
cbuffer Globals : register(CBUF_GLOBALS)
{
    float4      _OutlineWidth           : packoffset(c3);
    float4      _MainTex_ST             : packoffset(c9);
    float4      _MainTexHair_ST         : packoffset(c13);
};
#endif

#ifdef CBUF_UNITY_PER_CAMERA
cbuffer UnityPerCamera : register(CBUF_UNITY_PER_CAMERA)
{
    // Time values from Unity
    float4  _Time                   : packoffset(c0);
    float4  _SinTime                : packoffset(c1);
    float4  _CosTime                : packoffset(c2);
    float4  unity_DeltaTime         : packoffset(c3); // dt, 1/dt, smoothdt, 1/smoothdt

    float3  _WorldSpaceCameraPos    : packoffset(c4);

    // x = 1 or -1 (-1 if projection is flipped)
    // y = near plane
    // z = far plane
    // w = 1/far plane
    float4  _ProjectionParams       : packoffset(c5);
    
    // x = width
    // y = height
    // z = 1 + 1.0/width
    // w = 1 + 1.0/height
    float4  _ScreenParams           : packoffset(c6);

    float4  _ZBufferParams          : packoffset(c7);
};
#endif

#ifdef CBUF_UNITY_LIGHTING
cbuffer UnityLighting : register(CBUF_UNITY_LIGHTING)
{
    float4  _WorldSpaceLightPos0    : packoffset(c0);
    float4  _LightPositionRange     : packoffset(c1); // xyz = pos, w = 1/range

    float4  unity_4LightPosX0       : packoffset(c2);
    float4  unity_4LightPosY0       : packoffset(c3);
    float4  unity_4LightPosZ0       : packoffset(c4);
    float4  unity_4LightAtten0      : packoffset(c5);

    float4  unity_LightColor[8]     : packoffset(c6);
    float4  unity_LightPosition[8]  : packoffset(c14);
    // x = -1
    // y = 1
    // z = quadratic attenuation
    // w = range^2
    float4  unity_LightAtten[8]     : packoffset(c22);
    float4  unity_SpotDirection[8]  : packoffset(c30);

    // SH lighting environment
    float4  unity_SHAr          : packoffset(c38);
    float4  unity_SHAg          : packoffset(c39);
    float4  unity_SHAb          : packoffset(c40);

    float4  unity_SHBr          : packoffset(c41);
    float4  unity_SHBg          : packoffset(c42);
    float4  unity_SHBb          : packoffset(c43);
    float4  unity_SHC           : packoffset(c44);
};
#endif

#ifdef CBUF_UNITY_PER_DRAW
cbuffer UnityPerDraw : register(CBUF_UNITY_PER_DRAW)
{
//  matrix  glstate_matrix_mvp                  : packoffset(c0);
//  matrix  glstate_matrix_modelview0           : packoffset(c4);
//  matrix  glstate_matrix_invtrans_modelview0  : packoffset(c8);
//  matrix  _Object2World                       : packoffset(c12);
//  matrix  _World2Object                       : packoffset(c16);
//  float4  unity_Scale                         : packoffset(c20);

    float4x4    glstate_matrix_mvp                  : packoffset(c0);
    float4x4    glstate_matrix_modelview0           : packoffset(c4);
    float4x4    glstate_matrix_invtrans_modelview0  : packoffset(c8);
    float4x4    _Object2World                       : packoffset(c12);
    float4x4    _World2Object                       : packoffset(c16);
    float4      unity_Scale                         : packoffset(c20);
};

#endif

#ifdef CBUF_UNITY_PER_FRAME
cbuffer UnityPerFrame : register(CBUF_UNITY_PER_FRAME)
{
    float4x4 glstate_matrix_projection          : packoffset(c0);
    float4   glstate_lightmodel_ambient         : packoffset(c4);

    float4x4 unity_MatrixV                      : packoffset(c5);
    float4x4 unity_MatrixVP                     : packoffset(c9);
};
#endif
*/

#define UNITY_MATRIX_MVP glstate_matrix_mvp
#define UNITY_MATRIX_MV glstate_matrix_modelview0
#define UNITY_MATRIX_IT_MV glstate_matrix_invtrans_modelview0

#define UNITY_MATRIX_P glstate_matrix_projection
#define UNITY_LIGHTMODEL_AMBIENT glstate_lightmodel_ambient
#define UNITY_MATRIX_V unity_MatrixV
#define UNITY_MATRIX_VP unity_MatrixVP


//--------------------------------------------------------------------------------------
// Input / Output structures
//--------------------------------------------------------------------------------------
struct appdata_full {
    float4  vertex      : POSITION;
    float4  tangent     : TANGENT;
    float3  normal      : NORMAL;
    float4  texcoord    : TEXCOORD0;
    float4  texcoord1   : TEXCOORD1;
    float4  color       : COLOR;
};


struct ToonyLighted
{
    float4 position : SV_POSITION;
    float2 texCoord : TEXCOORD0;
    float3 viewDir  : TEXCOORD1;
    float3 normal   : TEXCOORD2;
    float3 color    : TEXCOORD3;
    float4 o5       : TEXCOORD4;
};


//--------------------------------------------------------------------------------------
#define MATRIX_BASIS_X(mat) mat._m00_m10_m20_m30
#define MATRIX_BASIS_Y(mat) mat._m01_m11_m21_m31
#define MATRIX_BASIS_Z(mat) mat._m02_m12_m22_m32
#define MATRIX_BASIS_W(mat) mat._m03_m13_m23_m33

// Transforms 2D UV by scale/bias property
#define TRANSFORM_TEX(tex,name) (tex.xy * name##_ST.xy + name##_ST.zw)

#define SCALED_NORMAL (v.normal * unity_Scale.w)

#if defined(USE_WORLD_SPACE_VIEW_DIR) || (defined(CBUF_UNITY_PER_CAMERA) && defined(CBUF_UNITY_PER_DRAW))
// Computes world space view direction
inline float3 WorldSpaceViewDir( in float4 v )
{
    return _WorldSpaceCameraPos.xyz - mul(_Object2World, v).xyz;
}
#endif
    
#if defined(USE_SHADE_SH) || defined(CBUF_UNITY_LIGHTING)
// normal should be normalized, w=1.0
float3 ShadeSH9 (float4 normal)
{
    float3 x1, x2, x3;

    // Linear + constant polynomial terms
    x1.r = dot(unity_SHAr,normal);
    x1.g = dot(unity_SHAg,normal);
    x1.b = dot(unity_SHAb,normal);

    // 4 of the quadratic polynomials
    float4 vB = normal.xyzz * normal.yzzx;
    x2.r = dot(unity_SHBr,vB);
    x2.g = dot(unity_SHBg,vB);
    x2.b = dot(unity_SHBb,vB);

    // Final quadratic polynomial
    float vC = normal.x*normal.x - normal.y*normal.y;
    x3 = unity_SHC.rgb * vC;
    return x1 + x2 + x3;
}
#endif


float3 Shade4PointLights (
    float4 lightPosX, float4 lightPosY, float4 lightPosZ,
    float3 lightColor0, float3 lightColor1, float3 lightColor2, float3 lightColor3,
    float4 lightAttenSq,
    float3 pos, float3 normal)
{
    // to light vectors
    float4 toLightX = lightPosX - pos.x;
    float4 toLightY = lightPosY - pos.y;
    float4 toLightZ = lightPosZ - pos.z;
    // squared lengths
    float4 lengthSq = 0;
    lengthSq += toLightX * toLightX;
    lengthSq += toLightY * toLightY;
    lengthSq += toLightZ * toLightZ;
    // NdotL
    float4 ndotl = 0;
    ndotl += toLightX * normal.x;
    ndotl += toLightY * normal.y;
    ndotl += toLightZ * normal.z;
    // correct NdotL
    float4 corr = rsqrt(lengthSq);
    ndotl = max (float4(0,0,0,0), ndotl * corr);
    // attenuation
    float4 atten = 1.0 / (1.0 + lengthSq * lightAttenSq);
    float4 diff = ndotl * atten;
    // final color
    float3 col = 0;
    col += lightColor0 * diff.x;
    col += lightColor1 * diff.y;
    col += lightColor2 * diff.z;
    col += lightColor3 * diff.w;
    return col;
}

//--------------------------------------------------------------------------------------
float4 CM3D2_RampSample(Texture2D tex, SamplerState samplerState, float x) {
    float2 t;
    t.x = x * 0.995;
    t.y = 0.5;
    return tex.Sample(samplerState, t);
}

float CM3D2_LitForLookup(float3 normal, float3 light) {
    float x;
    float4 r2, r3;
    r2.yzw = normalize(normal);
    x = dot(light, r2.yzw);
    x = mad(x, 0.495, 0.5);
    return x;
}

float CM3D2_expLit(float3 normal, float3 vec3, float2 param) {
    float x;
    x = saturate(dot(normal, vec3));
    x = -x + 1.0;
    x = x + param.y;
    x = log(x);
    x = x * param.x;
    x = exp(x);
    x = min(x, 1.0);
    return x;
}

float CM3D2_RimLit(float3 viewDir, float3 normal, float rimShift, float rimPower) {
    float x;
    x = saturate(dot(viewDir, normal));
    x = -x + 1;
    x = x + rimShift;
    x = log(x);
    x = x * rimPower;
    x = exp(x);
    x = min(x, 1.0);
    return x;
}

#ifdef USE_MULTICOL
float4 CM3D2_MultiColXyz(Texture2D tex, SamplerState samplerState, float2 uv) {
    float4 c = tex.Sample(samplerState, uv).xyzw;
    if(_UseMulticolTex) {
        c.xyz = CM3D2_RampSample(_MultiColTex, _MultiColTex_Sampler, c.x).xyz;
    }
    return c;
}

float4 CM3D2_MultiColXyzw(Texture2D tex, SamplerState samplerState, float2 uv) {
    float4 c = tex.Sample(samplerState, uv).xyzw;
    if(_UseMulticolTex) {
        c = CM3D2_RampSample(_MultiColTex, _MultiColTex_Sampler, c.x).xyzw;
    }
    return c;
}
#endif
