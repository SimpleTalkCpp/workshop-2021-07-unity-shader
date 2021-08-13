Shader "Unlit/Week003_WorldScanner"
{
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

			struct Attributes
			{
				float4 positionOS   : POSITION;
			};

			struct Varyings
			{
				float4 positionHCS  : SV_POSITION;
			};

			Varyings vert(Attributes IN)
			{
				Varyings OUT;
				OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
				return OUT;
			}

			float4 _Color;

			float4 frag(Varyings IN) : SV_Target
			{
//				return float4(1,0,0,1);

				float2 screenUV = IN.positionHCS.xy / _ScaledScreenParams.xy;

				#if UNITY_REVERSED_Z
					real depth = SampleSceneDepth(screenUV);
				#else
					real depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(UV));
				#endif

				float3 worldPos = ComputeWorldSpacePosition(screenUV, depth, UNITY_MATRIX_I_VP);

				float4 o = _Color;
				o.a *= length(worldPos.xz) * 0.001;
				return o;

				uint scale = 10;
				uint3 worldIntPos = uint3(abs(worldPos.xyz * scale));
				bool white = ((worldIntPos.x) & 1) ^ (worldIntPos.y & 1) ^ (worldIntPos.z & 1);
				float4 color = white ? half4(1,1,1,1) : half4(0,0,0,1);

				#if UNITY_REVERSED_Z
					if(depth < 0.0001)
						return half4(0,0,0,1);
				#else
					if(depth > 0.9999)
						return half4(0,0,0,1);
				#endif

				return color;
			}
			ENDHLSL
		}
	}
}