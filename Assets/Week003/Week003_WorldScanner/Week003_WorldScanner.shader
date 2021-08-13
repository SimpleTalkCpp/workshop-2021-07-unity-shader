Shader "Unlit/Week003_WorldScanner"
{
	Properties {
		_MainTex("Texture", 2D) = "white" {}
		[Enum(Week003_WorldScanner_UvMode)]
		_UvMode("UvMode", Int) = 0

		_Color ("Color", Color) = (1,1,1,1)
		_Radius("Radius", Range(0, 300)) = 10
		_EdgeWidth("Edge Width", Range(0,100)) = 5
		_EdgeSoftness("Edge Softness", Range(0,1)) = 0.01
		_ScannerCenter ("ScannerCenter", Vector) = (0,0,0,0)

	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
		ZTest Always
		ZWrite Off

		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

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
			float4 _ScannerCenter;
			float _Radius;
			float _EdgeWidth;
			float _EdgeSoftness;
			int _UvMode;

			MY_TEXTURE2D(_MainTex)

			float4 frag(Varyings i) : SV_Target
			{
				float2 screenUV = i.positionHCS.xy / _ScaledScreenParams.xy;

				#if UNITY_REVERSED_Z
					float depth = SampleSceneDepth(screenUV);
				#else
					float depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(screenUV));
				#endif

//				return float4(depth * 100, 1, 0, 1);

				float3 worldPos = ComputeWorldSpacePosition(screenUV, depth, UNITY_MATRIX_I_VP);

				float3 d = worldPos - _ScannerCenter.xyz;
				float dis = length(d);

				float outer = _Radius + _EdgeWidth;
				
				float2 uv = float2(my_invLerp(_Radius, outer, dis), 0);

				if (_UvMode == 2) {
					uv.y = atan2(d.z, d.x) / (2 * PI);
				}

				float4 tex = MY_SAMPLE_TEXTURE2D(_MainTex, uv);

				float alpha = smoothstep(0, _EdgeSoftness, saturate(1 - abs(uv.x * 2 - 1)));

				float4 o = _Color * tex;
				o.a *= alpha;
				return o;
			}
			ENDHLSL
		}
	}
}