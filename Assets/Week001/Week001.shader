Shader "Unlit/Week001"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_MainTex2 ("Texture2", 2D) = "white" {}
		_MaskTex ("MaskTexture", 2D) = "white" {}

		_Dissolve("Dissolve", range(0,1) ) = 0.0
		_Mode("Mode [0:Normal] [1:Radial] [2:Rhombus]", Int) = 1

		_PivotX("PivotX", range(0,1)) = 0.5
		_PivotY("PivotY", range(0,1)) = 0.5

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

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 maskUv : TEXCOORD1;
			};

			sampler2D _MainTex;
			sampler2D _MainTex2;
			sampler2D _MaskTex;
			float4    _MaskTex_ST;

			float _Dissolve;
			float _PivotX;
			float _PivotY;
			int _Mode;

			float _EdgeWidth;
			float _EdgeSoftness;
			float4 _EdgeColor;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.maskUv = TRANSFORM_TEX(v.uv, _MaskTex);
				return o;
			}

			float invLerp(float from, float to, float value){
				return (value - from) / (to - from);
			}

			float4 frag (v2f i) : SV_Target
			{
				const static float epsilon = 1E-4;

				float hardEdge = _EdgeWidth / 2;
				float softEdge = hardEdge + _EdgeSoftness + epsilon;

				if (_Mode == 1) { // Radial
					float distance = length(i.uv - float2(_PivotX, _PivotY));
					_Dissolve = saturate(_Dissolve / max(distance, epsilon));
				} else if (_Mode == 2) { // Rhombus
					float2 a = i.uv - float2(_PivotX, _PivotY);
					float distance = abs(a.x) + abs(a.y);
					_Dissolve = saturate(_Dissolve / max(distance, epsilon));
				}

//				return float4(_Dissolve, 0,0, 1);

				float dissolve = lerp(-softEdge, 1 + softEdge, _Dissolve);

				float4 tex  = tex2D(_MainTex,  i.uv);
				float4 tex2 = tex2D(_MainTex2, i.uv);

				float  mask = 1 - tex2D(_MaskTex, i.maskUv).r;
//				return float4(mask, mask, mask, 1);

				float4 o = lerp(tex, tex2, step(mask, dissolve));

				if (_EdgeWidth || _EdgeSoftness) { // avoid div by zero
					float e = abs(mask - dissolve);
					//float w = invLerp(hardEdge, softEdge, e);
					float w = smoothstep(hardEdge, softEdge, e);
					o = lerp(_EdgeColor, o,  saturate(w));
				}

				return o;
			}
			ENDCG
		}
	}
}
