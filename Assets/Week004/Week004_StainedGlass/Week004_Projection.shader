Shader "Unlit/Week004_Projection"
{
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_Intensity("Intensity", Range(0, 5)) = 1
		_DepthBias("Depth Bias", Range(0.001, 0.05)) = 0.05
		_FadeStart("FadeStart", Range(0, 10)) = 0
		_FadeWidth("FadeWidth", Range(0, 10)) = 1
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
			float4 _ProjPos;
			float4x4 _ProjVP;

			float _Intensity;
			float _DepthBias;
			float _FadeStart;
			float _FadeWidth;

			MY_TEXTURE2D(_MyProjColorTex)

			TEXTURE2D_FLOAT(_MyProjDepthTex);
			SAMPLER(sampler_MyProjDepthTex);

			float4 frag(Varyings i) : SV_Target
			{
				float2 screenUV = i.positionHCS.xy / _ScaledScreenParams.xy;
				float depth = SampleSceneDepth(screenUV);

				float3 worldPos = ComputeWorldSpacePosition(screenUV, depth, UNITY_MATRIX_I_VP);

				float4 projPos = mul(_ProjVP, float4(worldPos,1));
				projPos.xyz /= projPos.w;

				float2 absProjPos = abs(projPos.xy);
				if (absProjPos.x > 1 || absProjPos.y > 1)
					return float4(0,0,0,0);

				float2 projUv = 0.5 - projPos.xy * 0.5;

				float4 projColorTex = MY_SAMPLE_TEXTURE2D(_MyProjColorTex, projUv);
				float  projDepthTex = SAMPLE_TEXTURE2D_X(_MyProjDepthTex, sampler_MyProjDepthTex, projUv).r;

//				return float4(-projPos.z   * 10, 0, 0, 1);
//				return float4(projDepthTex * 10, 0, 0, 1);
	
				float diff = projDepthTex + projPos.z;
//				return float4(diff * 10, 0, 0, 1);

				if (diff > -_DepthBias)
					return float4(1,0,0,0);

				float projDis = length(_ProjPos - worldPos);

				float fade = 1 - saturate(smoothstep(_FadeStart, _FadeStart * _FadeWidth, projDis));
				projColorTex.a *= fade * fade;

				_Color.rgb *= _Intensity;

				return projColorTex * _Color;
			}
			ENDHLSL
		}
	}
}