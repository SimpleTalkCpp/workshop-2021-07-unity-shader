Shader "Unlit/Week006_NormalMap"
{
	Properties
	{
		_BaseColor("BaseColor", Color) = (1,1,1,1)
		_Ambient ("Ambient",    Color) = (0.1, 0.1, 0.1, 1)
		_Diffuse ("Diffuse",    Range(0,1)) = 0.7
		_Specular("Specular",   Range(0,1)) = 0.25
		_Shininess("Shininess", Range(0.1, 256)) = 32
		_NormalMap("Normal Map", 2D) = "white" {}
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
				float3 tangent : TANGENT;
				float2 uv : TEXCOORD0;
			};

			struct Varyings
			{
				float4 positionHCS : SV_POSITION;
				float3 positionWS  : TEXCOORD8;
				float3 normalWS    : NORMAL;
				float3 tangentWS   : TANGENT;
				float2 uv : TEXCOORD0;
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

			MY_TEXTURE2D(_NormalMap)

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
				o.normalWS    = TransformObjectToWorldDir(i.normal);
				o.tangentWS   = TransformObjectToWorldDir(i.tangent);
				o.uv = i.uv;
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

					o.rgb += (diffuse + specular) * intensity * s.baseColor.rgb * lightColor;
				}

				return o;
			}

			float3 UnpackNormalDXT5nm (float4 packednormal)
			{
				float3 normal;
				normal.xy = packednormal.wy * 2 - 1;
				normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy)));
				return normal;
			}

			float4 frag (Varyings i) : SV_Target
			{
				SurfaceInfo s;
				s.baseColor = _BaseColor;
				s.ambient   = _Ambient.rgb;
				s.diffuse   = _Diffuse;
				s.specular  = _Specular;
				s.shininess = _Shininess;

				float3 normalMap = UnpackNormalDXT5nm(MY_SAMPLE_TEXTURE2D(_NormalMap, i.uv));

				float3 N = normalize(i.normalWS);
				float3 T = normalize(i.tangentWS); // Tangent
				float3 B = cross(N, T); // Bi-Normal, Bi-Tangent
				float3x3 TBN = float3x3(T, B, N);
				s.normal = mul(TBN, normalMap); // tangent space -> world space

				return computeLighting(i, s);
			}
			ENDHLSL
		}
	}
}
