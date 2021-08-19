Shader "Unlit/Week004_Phong"
{
	Properties
	{
		_BaseColor("BaseColor", Color) = (1,1,1,1)
		_Ambient ("Ambient",    Color) = (0.1, 0.1, 0.1, 1)
		_Diffuse ("Diffuse",    Range(0,1)) = 0.7
		_Specular("Specular",   Range(0,1)) = 0.25
		_Shininess("Shininess", Range(0.1, 256)) = 32
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "../../MyCommon/MyCommon.hlsl"

			struct Attributes
			{
				float4 positionOS   : POSITION;
				float3 normal : NORMAL;
			};

			struct Varyings
			{
				float4 positionHCS : SV_POSITION;
				float3 positionWS  : TEXCOORD8;
				float3 viewDir     : TEXCOORD9;
				float3 normal      : NORMAL;
			};

			struct SurfaceInfo {
				float4 baseColor;
				float3 ambient;
				float diffuse;
				float specular;
				float shininess;
				float3 normal;
			};

			float4 _BaseColor;
			float4 _Ambient;
			float _Diffuse;
			float _Specular;
			float _Shininess;

			static const int kMaxLightCount = 8;

			int		g_MyLightCount;
			float4	g_MyLightColor[kMaxLightCount];
			float4	g_MyLightPos[kMaxLightCount];
			float4	g_MyLightDir[kMaxLightCount];
			float4	g_MyLightParam[kMaxLightCount];

			Varyings vert (Attributes i)
			{
				Varyings o;
				o.positionHCS = TransformObjectToHClip(i.positionOS.xyz);
				o.positionWS  = TransformObjectToWorld(i.positionOS.xyz);
				o.normal = i.normal;
				return o;
			}

			float4 computeLighting(Varyings i, SurfaceInfo s) {
				float3 viewDir = normalize(_WorldSpaceCameraPos - i.positionWS);

				int lightCount = min(kMaxLightCount, g_MyLightCount);
				float4 o = s.baseColor * float4(s.ambient, 1);

				for (int j = 0; j < lightCount; j++) {
					float3 lightColor     = g_MyLightColor[j].rgb;
					float  lightIntensity = g_MyLightColor[j].a;

					float3 lightPos       = g_MyLightPos[j].xyz;

					float3 lightDir       = g_MyLightDir[j].xyz;
					float  isDirectional  = g_MyLightDir[j].w;

					float3 lightPosDir = i.positionWS - lightPos;

					float3 L = lerp(lightPosDir, lightDir, isDirectional);
					float  lightSqDis = dot(L,L) * (1 - isDirectional);

					L = normalize(L);

					float  isSpotlight         = g_MyLightParam[j].x;
					float  lightSpotAngle      = g_MyLightParam[j].y;
					float  lightInnerSpotAngle = g_MyLightParam[j].z;
					float  lightRange          = g_MyLightParam[j].w;

					float diffuse  = s.diffuse * max(dot(-L, s.normal), 0.0);

					float3 reflectDir = reflect(L, s.normal);
					float specular = s.specular * pow(max(dot(viewDir, reflectDir), 0.0), s.shininess);

					float attenuation = 1 - saturate(lightSqDis / (lightRange * lightRange));
					float intensity = lightIntensity * attenuation;

					if (isSpotlight > 0) {
						intensity *= smoothstep(lightSpotAngle, lightInnerSpotAngle, dot(lightDir, L));
					}

					o.rgb += (diffuse + specular) * intensity * s.baseColor * lightColor;
				}

				return o;
			}

			float4 frag (Varyings i) : SV_Target
			{
				SurfaceInfo s;
				s.baseColor = _BaseColor;
				s.ambient   = _Ambient;
				s.diffuse   = _Diffuse;
				s.specular  = _Specular;
				s.shininess = _Shininess;
				s.normal = normalize(i.normal);
				return computeLighting(i, s);
			}
			ENDHLSL
		}
	}
}
