Shader "Unlit/Week004_CopyDepth"
{
	SubShader
	{
		Tags {
			"Queue" = "Transparent"
			"RenderType" = "Transparent" 
			"RenderPipeline" = "UniversalPipeline" 
		}
		
		ZTest Always
//		ZWrite Off

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

			float4 frag(Varyings i) : SV_Target
			{
				float2 screenUV = i.positionHCS.xy / _ScaledScreenParams.xy;

				float depth = SampleSceneDepth(screenUV);

//				#if UNITY_REVERSED_Z
//					float depth = SampleSceneDepth(screenUV);
//				#else
//					float depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(screenUV));
//				#endif
				return float4(depth, 0, 0, 1);
			}
			ENDHLSL
		}
	}
}