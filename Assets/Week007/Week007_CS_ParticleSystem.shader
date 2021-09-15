Shader "Unlit/Week07_CS_ParticleSystem"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Scale("Scale", Range(0, 2)) = 1
		_Color("Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags {
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
		}
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off

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
			float4 _Color;
			float _Scale;

			float4 MatrixTranslate(float4x4 m) {
				return m._m03_m13_m23_m33;
			}

			v2f vert (appdata v, uint instance_id: SV_InstanceID)
			{
				float life = _particleLifespan[instance_id].x / _particleLifespan[instance_id].y;
				float alpha = 1 - (1 - life) * (1 - life);

				float4 posWS = MatrixTranslate(unity_ObjectToWorld) + float4(_particlePosition[instance_id], 0);
//				posWS.z += instance_id;

				float4 posVS = mul(UNITY_MATRIX_V, posWS);
				posVS += v.vertex * _Scale; // billboard

				v2f o;
				o.vertex = mul(UNITY_MATRIX_P, posVS);

				if (life <= 0) {
					o.vertex = float4(0,0,0,1);
				}

				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				
				uint ci = instance_id % 8;
				float3 color = saturate(float3(ci & 1, ci & 2, ci & 4));

				o.color = _Color * float4(color, alpha);
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
