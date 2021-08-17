Shader "Unlit/Week003_Water"
{
	Properties {
		_MainTex("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		_Intersect("Intersect",  range(0,2)) = 1
	}

	SubShader
	{
		Tags {
			"Queue" = "Transparent"
			"RenderType" = "Transparent" 
			"RenderPipeline" = "UniversalPipeline" 
		}
//		ZTest Always
		ZWrite Off
		Cull Off

		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
			#include "../../MyCommon/MyCommon.hlsl"

			struct Attributes
			{
				float4 positionOS   : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Varyings
			{
				float4 positionHCS  : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			float4 _Color;
			float _Intersect;

			Varyings vert(Attributes i)
			{
				Varyings o;
				o.positionHCS = TransformObjectToHClip(i.positionOS.xyz);
				o.uv = i.uv;
				return o;
			}

			MY_TEXTURE2D(_MainTex)

			float4 frag(Varyings i) : SV_Target
			{
				float2 screenUV = i.positionHCS.xy / _ScaledScreenParams.xy;

				#if UNITY_REVERSED_Z
					float depth = SampleSceneDepth(screenUV);
				#else
					float depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(screenUV));
				#endif

				float3 positionVS0 = ComputeViewSpacePosition(screenUV, depth, UNITY_MATRIX_I_P);
				float3 positionVS1 = ComputeViewSpacePosition(screenUV, i.positionHCS.z, UNITY_MATRIX_I_P);

				float d = saturate(smoothstep(_Intersect, 0, positionVS0.z - positionVS1.z));

				float4 tex = MY_SAMPLE_TEXTURE2D(_MainTex, i.uv);
				
				float4 o = _Color * tex;
				o.a *= 1 - d;
				return o;
			}
			ENDHLSL
		}
	}
}