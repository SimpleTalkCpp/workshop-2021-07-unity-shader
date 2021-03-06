Shader "Unlit/Week006_WaterInBottle"
{
	Properties
	{
		_BaseColor("BaseColor", Color) = (1,1,1,1)
		_WaterSurfaceColor("Water Surface Color", Color) = (1,1,1,1)
		_WaterPlane("Water Plane", Vector) = (0,1,0,0)
		_Refractive("_Refractive", Range(0, 1)) = 0.2 
		_EdgeRefractive("_EdgeRefractive", Range(0, 1)) = 0.2 
		_Edge("_Edge", Range(0.0001, 0.01)) = 0.0002 

		[Toggle]
		_DebugSurfaceUV("== Debug Surface UV ==", int) = 0
		_UVChecker("UV Checker", 2D) = "white" {}
	}
	SubShader
	{
		Tags {
			"RenderType"="Transparent"
			"Queue"="Transparent+1"
		}

		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
			#include "../../MyCommon/MyCommon.hlsl"

			struct Attributes
			{
				float4 positionOS : POSITION;
				float3 normalOS   : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct Varyings
			{
				float4 positionHCS : SV_POSITION;
				float3 positionWS  : TEXCOORD8;
				float3 normalVS    : NORMAL;
				float2 uv : TEXCOORD0;
			};

			float4 _BaseColor;
			float4 _WaterSurfaceColor;
			float4 _WaterPlane;
			float4 _WaterPivot;
			int _DebugSurfaceUV;

			float _Edge;
			float _Refractive;
			float _EdgeRefractive;

			MY_TEXTURE2D(_UVChecker);

			Varyings vert (Attributes i)
			{
				Varyings o;
				o.positionHCS = TransformObjectToHClip(i.positionOS.xyz);
				o.positionWS  = TransformObjectToWorld(i.positionOS.xyz);
				o.normalVS    = mul((float3x3)UNITY_MATRIX_IT_MV, i.normalOS);
				o.uv = i.uv;
				return o;
			}

			float4 frag (Varyings i, bool isFrontFace : SV_IsFrontFace) : SV_Target
			{
				float2 screenUV = i.positionHCS.xy / _ScaledScreenParams.xy;

				float d = _WaterPlane.w - dot(_WaterPlane.xyz, i.positionWS);

				if (d < 0) {
					discard;
				}

				float2 r = lerp(_EdgeRefractive, _Refractive, saturate(d / _Edge));

				float4 color = _BaseColor;

				float3 N = normalize(i.normalVS);
				if (isFrontFace) {

					if (_DebugSurfaceUV) {
						float3 viewDir = normalize(i.positionWS - _WorldSpaceCameraPos);
						float3 planeNormal = _WaterPlane.xyz;
						float t = dot(_WorldSpaceCameraPos - _WaterPivot.xyz, planeNormal) / dot(viewDir, planeNormal);
						float3 pt = t * viewDir - _WorldSpaceCameraPos;					
						float3 uv = pt + _WaterPivot.xyz;
						float4 uvChecker = MY_SAMPLE_TEXTURE2D(_UVChecker, uv.xz);
						return uvChecker;
					}

					N = _WaterPlane.xyz;
					color = _WaterSurfaceColor;
				}

				float3 c = SampleSceneColor(screenUV + (N * N).xy * r);
				c = lerp(c, color.rgb, color.a);
				return float4(c, 1);
			}
			ENDHLSL
		}
	}
}
