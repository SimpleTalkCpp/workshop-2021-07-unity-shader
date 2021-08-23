Shader "Unlit/Week004_Projection"
{
	Properties {
		_MyProjColorTex("ProjectColorTexture", 2D) = "white" {}
		_MyProjDepthTex("ProjectDepthTexture", 2D) = "white" {}

		_Color ("Color", Color) = (1,1,1,1)
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
		ZTest Always
		ZWrite Off

//		Blend SrcAlpha OneMinusSrcAlpha
		Blend SrcAlpha One

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
			};

			struct Varyings
			{
				float4 positionHCS  : SV_POSITION;
			};

			Varyings vert(Attributes i)
			{
				Varyings o;
//				o.positionHCS = TransformObjectToHClip(i.positionOS.xyz);
				o.positionHCS = i.positionOS;
				return o;
			}

			float4 _Color;
			float4x4 _ProjVP;

			MY_TEXTURE2D(_MyProjColorTex)
			MY_TEXTURE2D(_MyProjDepthTex)

			float4 frag(Varyings i) : SV_Target
			{
				float2 screenUV = i.positionHCS.xy / _ScaledScreenParams.xy;

				#if UNITY_REVERSED_Z
					float depth = SampleSceneDepth(screenUV);
				#else
					float depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(screenUV));
				#endif

				float3 worldPos = ComputeWorldSpacePosition(screenUV, depth, UNITY_MATRIX_I_VP);

				float4 projPos = mul(_ProjVP, float4(worldPos,1));

				projPos.xy /= projPos.w;

				float2 absProjPos = abs(projPos.xy);
				if (absProjPos.x > 1 || absProjPos.y > 1)
					return float4(0,0,0,0);

				projPos = projPos * 0.5 + 0.5;
				projPos = 1 - projPos;

				float4 projColorTex = MY_SAMPLE_TEXTURE2D(_MyProjColorTex, projPos.xy);
				float  projDepthTex = MY_SAMPLE_TEXTURE2D(_MyProjDepthTex, projPos.xy).r;

				#if ! UNITY_REVERSED_Z
					projDepthTex = lerp(UNITY_NEAR_CLIP_VALUE, 1, projDepthTex);
				#endif
	
//				return float4(projDepthTex.x * 1000, 0, 0, 1);

				float d = 1 - (projDepthTex * 0.5 + 0.5);

				if (projPos.z < d)
					return float4(1,0,0,1);

				return projColorTex * _Color;
			}
			ENDHLSL
		}
	}
}