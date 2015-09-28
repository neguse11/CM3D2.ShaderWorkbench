Shader "CM3D2/Outline" {
	Properties {
		_OutlineColor ("Outline Color", Color) = (0,0,0,1)
		_OutlineWidth ("Outline width", Range(0.0016,0.003)) = 0.0016
		_ToonRamp ("Toon Ramp (RGB)", 2D) = "gray" {}
	}

	SubShader {
		Tags { "QUEUE"="Geometry" "RenderType"="Opaque" }

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
		}
	}

	Fallback "Toon/Basic"
}
