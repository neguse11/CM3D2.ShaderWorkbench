Shader "CM3D2/Toony_Lighted_Outline_Trans" {
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
		_OutlineColor ("Outline Color", Color) = (0,0,0,1)
		_OutlineWidth ("Outline width", Range(0.0016,0.003)) = 0.0016
		_MultiColTex ("Multi Color Table (RGBA)", 2D) = "white" {}
		[MaterialToggle]  _UseMulticolTex ("Use MultiColor Tex", Float) = 0
		_SetManualRenderQueue ("_SetManualRenderQueue", Float) = 3000
	}

	SubShader {
		Tags { "QUEUE"="Transparent" "RenderType"="Transparent" }

		// UsePass "CM3D2/Toony_Lighted_Trans/FORWARD"
		Pass {
			Name "FORWARD"
			Tags { "LIGHTMODE"="ForwardBase" "QUEUE"="Transparent" "IGNOREPROJECTOR"="true" "RenderType"="Transparent" }
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
			} // Program "vp"
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
			} // Program "fp"
		}

		// UsePass "CM3D2/Outline/OUTLINE"
		Pass {
			Name "OUTLINE"
			Tags { "LIGHTMODE"="ForwardBase" "SHADOWSUPPORT"="true" "QUEUE"="Geometry" "RenderType"="Opaque" }
			Cull Front
			Program "vp" {
				SubProgram "d3d11 " {
					Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" }
					Bind "vertex" Vertex
					Bind "normal" Normal
					Bind "texcoord" TexCoord0
					ConstBuffer "$Globals" 64
					Float 48 [_OutlineWidth]
					ConstBuffer "UnityLighting" 720
					Vector 0 [_WorldSpaceLightPos0]
					ConstBuffer "UnityPerDraw" 336
					Matrix 0 [glstate_matrix_mvp]
					Matrix 128 [glstate_matrix_invtrans_modelview0]
					Matrix 256 [_World2Object]
					ConstBuffer "UnityPerFrame" 208
					Matrix 0 [glstate_matrix_projection]
					BindCB  "$Globals" 0
					BindCB  "UnityLighting" 1
					BindCB  "UnityPerDraw" 2
					BindCB  "UnityPerFrame" 3
					__CompilerArgument__ { "profile"="vs_4_0" "entrypoint"="VSMain" "source"="Outline_vs.hlsl" }
				}
			}

			Program "fp" {
				SubProgram "d3d11 " {
					Keywords { "DIRECTIONAL" "SHADOWS_OFF" "LIGHTMAP_OFF" "DIRLIGHTMAP_OFF" }
					SetTexture 0 [_ToonRamp] 2D 0
					ConstBuffer "$Globals" 64
					Vector 16 [_OutlineColor]
					Vector 32 [_LightColor0]
					ConstBuffer "UnityPerFrame" 208
					Vector 64 [glstate_lightmodel_ambient]
					BindCB  "$Globals" 0
					BindCB  "UnityPerFrame" 1
					__CompilerArgument__ { "profile"="ps_4_0" "entrypoint"="PSMain" "source"="Outline_ps.hlsl" }
				}
			}
		} // Pass "OUTLINE"
	}

	Fallback "CM3D2/Toony_Lighted"
}
