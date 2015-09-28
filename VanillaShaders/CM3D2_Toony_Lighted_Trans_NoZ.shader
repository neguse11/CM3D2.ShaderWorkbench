Shader "CM3D2/Toony_Lighted_Trans_NoZ" {
	Properties {
		_Color ("Main Color", Color) = (0.5,0.5,0.5,1)
		_ShadowColor ("Shadow Color", Color) = (0.5,0.5,0.5,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_ToonRamp ("Toon Ramp (RGB)", 2D) = "gray" {}
		_ShadowTex ("Shadow Texture(RGBA)", 2D) = "white" {}
		_ShadowRateToon ("Shadow Rate Toon (RGB)", 2D) = "white" {}
		_RimColor ("Rim Color", Color) = (0,0,1,0)
		_RimPower ("Rim Power", Range(0.1,30)) = 3
		_RimShift ("Rim Shift", Range(0,1)) = 0
		_Shininess ("Shininess", Range(0,1)) = 0
		_MultiColTex ("Multi Color Table (RGBA)", 2D) = "white" {}
		[MaterialToggle]  _UseMulticolTex ("Use MultiColor Tex", Float) = 0
		_SetManualRenderQueue ("_SetManualRenderQueue", Float) = 3000
	}
	SubShader { 
		LOD 200
		Tags { "QUEUE"="Transparent" "RenderType"="Transparent" }
		Pass {
			Name "FORWARD"
			Tags { "LIGHTMODE"="ForwardBase" "QUEUE"="Transparent" "RenderType"="Transparent" }
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			AlphaTest Greater 0
			ColorMask RGB
			Program "vp" {
				SubProgram "d3d11 " {
					Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" "VERTEXLIGHT_ON" }
					Bind "vertex" Vertex
					Bind "color" Color
					Bind "normal" Normal
					Bind "texcoord" TexCoord0
					ConstBuffer "$Globals" 160
					Vector 144 [_MainTex_ST]
					ConstBuffer "UnityPerCamera" 128
					Vector 64 [_WorldSpaceCameraPos] 3
					ConstBuffer "UnityLighting" 720
					Vector 32 [unity_4LightPosX0]
					Vector 48 [unity_4LightPosY0]
					Vector 64 [unity_4LightPosZ0]
					Vector 80 [unity_4LightAtten0]
					Vector 96 [unity_LightColor0]
					Vector 112 [unity_LightColor1]
					Vector 128 [unity_LightColor2]
					Vector 144 [unity_LightColor3]
					Vector 160 [unity_LightColor4]
					Vector 176 [unity_LightColor5]
					Vector 192 [unity_LightColor6]
					Vector 208 [unity_LightColor7]
					Vector 608 [unity_SHAr]
					Vector 624 [unity_SHAg]
					Vector 640 [unity_SHAb]
					Vector 656 [unity_SHBr]
					Vector 672 [unity_SHBg]
					Vector 688 [unity_SHBb]
					Vector 704 [unity_SHC]
					ConstBuffer "UnityPerDraw" 336
					Matrix 0 [glstate_matrix_mvp]
					Matrix 192 [_Object2World]
					Vector 320 [unity_Scale]
					BindCB  "$Globals" 0
					BindCB  "UnityPerCamera" 1
					BindCB  "UnityLighting" 2
					BindCB  "UnityPerDraw" 3

					__CompilerArgument__ { "profile"="vs_4_0" "entrypoint"="VSMain" "source"="Toony_Lighted_Trans_vs.hlsl" }
				}
			}
			Program "fp" {
				SubProgram "d3d11 " {
					Keywords { "DIRECTIONAL" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" }
					SetTexture 0 [_MainTex] 2D 4
					SetTexture 1 [_MultiColTex] 2D 3
					SetTexture 2 [_ToonRamp] 2D 0
					SetTexture 3 [_ShadowRateToon] 2D 2
					SetTexture 4 [_ShadowTex] 2D 1
					ConstBuffer "$Globals" 160
					Vector 16 [_LightColor0]
					Float 48 [_Shininess]
					Float 80 [_UseMulticolTex]
					Vector 96 [_Color]
					Vector 112 [_RimColor]
					Float 128 [_RimPower]
					Float 132 [_RimShift]
					ConstBuffer "UnityLighting" 720
					Vector 0 [_WorldSpaceLightPos0]
					BindCB  "$Globals" 0
					BindCB  "UnityLighting" 1

					__CompilerArgument__ { "profile"="ps_4_0" "entrypoint"="PSMain" "source"="Toony_Lighted_ps.hlsl" }
				}
			}
		}
	}
	Fallback "Diffuse"
}
