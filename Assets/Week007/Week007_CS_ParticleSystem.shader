Shader "Unlit/Week07_CS_ParticleSystem"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags {
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
		}
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
			};

			StructuredBuffer<float3> _particlePosition;
			StructuredBuffer<float2> _particleLifespan;

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float4 MatrixTranslate(float4x4 m) {
				return m._m03_m13_m23_m33;
			}

			v2f vert (appdata v, uint instance_id: SV_InstanceID)
			{
				float4 posWS = MatrixTranslate(unity_ObjectToWorld) + float4(_particlePosition[instance_id], 0);
//				posWS.z += instance_id;

				float4 posVS = mul(UNITY_MATRIX_V, posWS);
				posVS += v.vertex; // billboard

				v2f o;
				o.vertex = mul(UNITY_MATRIX_P, posVS);

				float life = _particleLifespan[instance_id].x / _particleLifespan[instance_id].y;

				if (life <= 0) {
					o.vertex = float4(0,0,0,1);
				}

				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				
				uint ci = instance_id % 8;
				float3 color = saturate(float3(ci & 1, ci & 2, ci & 4));
				o.color = float4(color, life > 0 ? 1 : 0);
				return o;
			}

			float4 frag (v2f i) : SV_Target
			{
				float4 tex = tex2D(_MainTex, i.uv);
				return tex * i.color;
			}
			ENDCG
		}
	}
}
