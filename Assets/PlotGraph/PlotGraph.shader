Shader "Unlit/PlotGraph"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_LineWidth("LineWidth", range(1, 50)) = 4
		_Param("Param", Vector) = (0,0,0,0)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		Cull off
		LOD 100

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
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _LineWidth;
			float4 _Param;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			float func(float x) {
				float t = abs(frac(x) * 2 - 1);
				return t;

				float v = t*t*(3.0 - (2.0*t));
				return v;
				// return abs(x - _Param.x);
				//return step(_Param.x, x);
			}

			float4 frag (v2f i) : SV_Target
			{
				float4 tex = tex2D(_MainTex, i.uv);

				float2 graphCoord = (i.uv - 0.5) * 10;

				float w = _LineWidth * ddy(i.uv.y);
				float value = func(graphCoord.x);
				value = w / abs(graphCoord.y - value);

				return lerp(tex, float4(1,0,0,1), saturate(value));
			}

			ENDCG
		}
	}
}
