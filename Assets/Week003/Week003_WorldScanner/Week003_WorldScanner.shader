Shader "Unlit/Week003_WorldScanner"
{
	Properties {
		_MainTex("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		_Radius("Radius", range(0, 200)) = 10
		_EdgeWidth("Edge Width", range(0,50)) = 5
		_EdgeSoftness("Edge Softness", range(0,1)) = 0.01
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

			MY_TEXTURE2D(_MainTex)

			float4 frag(Varyings i) : SV_Target
			{
//				return float4(1,0,0,1);

				float2 screenUV = i.positionHCS.xy / _ScaledScreenParams.xy;

				#if UNITY_REVERSED_Z
					real depth = SampleSceneDepth(screenUV);
				#else
					real depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(screenUV));
				#endif

//				return float4(depth * 100, 1, 0, 1);

				float3 worldPos = ComputeWorldSpacePosition(screenUV, depth, UNITY_MATRIX_I_VP);

				float3 d = worldPos - _ScannerCenter.xyz;
				float dis = length(d);

				float outer = _Radius + _EdgeWidth;
				float2 uv = float2(my_invLerp(_Radius, outer, dis), 0.5);
				float4 tex = MY_SAMPLE_TEXTURE2D(_MainTex, uv);

				float alpha = smoothstep(0, _EdgeSoftness, saturate(1 - abs(uv.x * 2 - 1)));

				float4 o = _Color * tex;
				o.a = alpha;

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