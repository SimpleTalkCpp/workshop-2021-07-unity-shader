Shader "Unlit/Week002_DissolveTriangles"
{
	Properties
	{
//		_MyTime("MyTime", range(0,1)) = 0
		_MainTex ("Texture", 2D) = "white" {}
		_Duration("Duration", float) = 2
		_Delay("Delay", float) = 0
		_Offset("Offset", float) = 1
		_Scale("Scale", float) = 1
		_Spread("Spread", float) = 0.15
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
		Cull Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv  : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
				float2 uv3 : TEXCOORD2;
			};

			struct v2f
			{				
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
				float2 uv : TEXCOORD0;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Duration;
			float _Delay;
			float _Offset;
			float _Scale;
			float _Spread;
			float _MyTime;

			float4 MatrixTranslate(float4x4 m) {
				return m._m03_m13_m23_m33;
//				return float4(m._m03, m._m13, m._m23, m._m33);
			}

			v2f vert (appdata v)
			{
				v2f o;

				float3 center = float3(v.uv2.x, v.uv2.y, v.uv3.x);
				float  groupId = v.uv3.y;

				float intensity = saturate((groupId -1 + _MyTime - _Delay) / _Duration);

				float scale = saturate(intensity * _Scale);
				v.vertex.xyz = lerp(v.vertex.xyz, center, scale);

				float4 worldPos       = mul(unity_ObjectToWorld, v.vertex);
				float4 objectWorldPos = MatrixTranslate(unity_ObjectToWorld);

				float  y  = intensity * _Offset;
				float2 xz = worldPos.xz - objectWorldPos.xz;

				worldPos.y  += y * y;
				worldPos.xz += xz * (intensity) * _Spread;

				o.color = float4(groupId, 0, 0, 1);

				o.vertex = mul(UNITY_MATRIX_VP, worldPos);

				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			float4 frag (v2f i) : SV_Target
			{
				float4 col = tex2D(_MainTex, i.uv);
				return col;
			}
			ENDCG
		}
	}
}
