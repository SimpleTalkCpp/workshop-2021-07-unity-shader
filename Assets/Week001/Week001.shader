Shader "Unlit/Week001"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_MainTex2 ("Texture2", 2D) = "white" {}
		_MaskTex ("MaskTexture", 2D) = "white" {}

		_Dissolve("Dissolve", range(0,1) ) = 0.0
		_EdgeWidth("Edge Width", range(0,1)) = 0.0
		_EdgeSoftness("Edge Softness", range(0,1)) = 0.0
		_EdgeColor("Edge Color", Color) = (1,0,0,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

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
			};

			sampler2D _MainTex;
			sampler2D _MainTex2;
			sampler2D _MaskTex;

			float _Dissolve;
			float _EdgeWidth;
			float _EdgeSoftness;
			float4 _EdgeColor;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			float invLerp(float from, float to, float value){
				return (value - from) / (to - from);
			}

			float remap(float origFrom, float origTo, float targetFrom, float targetTo, float value){
				float rel = invLerp(origFrom, origTo, value);
				return lerp(targetFrom, targetTo, rel);
			}

			float4 frag (v2f i) : SV_Target
			{
				const static float epsilon = 1E-4;

				float hardEdge = _EdgeWidth / 2;
				float softEdge = hardEdge + _EdgeSoftness + epsilon;

				float dissolve = lerp(-softEdge, 1 + softEdge, _Dissolve);

				float4 tex  = tex2D(_MainTex,  i.uv);
				float4 tex2 = tex2D(_MainTex2, i.uv);
				float  mask = tex2D(_MaskTex, i.uv).r;

				if (0) { // debug
					tex  = float4(0,1,1,1);
					tex2 = float4(0,1,0,1);
				}

				float4 o = lerp(tex, tex2, step(mask, dissolve));

				if (_EdgeWidth || _EdgeSoftness) { // avoid div by zero
					float e = abs(mask - dissolve);
					float w = 1 - saturate(invLerp(hardEdge, softEdge, e));
					o = lerp(o, _EdgeColor,  w);
				}

				return o;
			}
			ENDCG
		}
	}
}
