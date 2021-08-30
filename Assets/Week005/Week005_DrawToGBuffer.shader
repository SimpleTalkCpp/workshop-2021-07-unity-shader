Shader "Week005/Week005_DrawToGBuffer"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_Diffuse ("Diffuse",    Range(0,1)) = 0.7
		_Specular("Specular",   Range(0,1)) = 0.25
		_Shininess("Shininess", Range(0.1, 256)) = 32
	}
	SubShader
	{
		Tags {
			"RenderType" = "Opaque"
			"LightMode" = "MyGBuffer"
			"RenderPipeline" = "UniversalPipeline"
		}

		Cull [_MyGBufferCull]

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "../MyCommon/MyCommon.hlsl"

			struct Attributes
			{
				float4 positionOS   : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct Varyings
			{
				float4 positionHCS  : SV_POSITION;
				float3 positionWS   : TEXCOORD8;
				float3 normalWS     : TEXCOORD9;
				float2 uv : TEXCOORD0;
			};

			MY_TEXTURE2D(_MainTex)
			float4 _Color;
			float _Diffuse;
			float _Specular;
			float _Shininess;

			Varyings vert(Attributes i) {
				Varyings o;
				o.positionHCS = TransformObjectToHClip(i.positionOS.xyz);
				o.positionWS  = TransformObjectToWorld(i.positionOS.xyz);
				o.normalWS    = TransformObjectToWorldDir(i.normal);
				o.uv = i.uv;

			// _ProjectionParams 
			//   - x is 1.0 (or –1.0 if currently rendering with a flipped projection matrix),
			//   - y is the camera’s near plane,
			//   - z is the camera’s far plane and w is 1/FarPlane.

				if (_ProjectionParams.x < 0)
				{
					o.positionHCS.y = -o.positionHCS.y;
//					o.positionHCS.z = 1 + o.positionHCS.z;
				}

				return o;
			}

			struct FragmentOutput {
				float4 baseColor   : SV_Target0;
				float4 positionWS  : SV_Target1;
				float4 normalWS    : SV_Target2;
			};

			FragmentOutput frag(Varyings i)
			{
				FragmentOutput o;
				o.baseColor  = MY_SAMPLE_TEXTURE2D(_MainTex, i.uv) * _Color;
				o.baseColor.a = _Diffuse;
				o.positionWS = float4(i.positionWS, _Specular);
				o.normalWS   = float4(i.normalWS,   _Shininess);
				return o;
			}

			ENDHLSL
		}
	}
}
