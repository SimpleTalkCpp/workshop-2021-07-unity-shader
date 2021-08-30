Shader "Week005/Week005_Deferred_Phong"
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

		ZTest Always
		ZWrite Off
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "../MyCommon/MyCommon.hlsl"

			struct Attributes
			{
				float4 positionOS   : POSITION;
			};

			struct Varyings
			{
				float4 positionHCS : SV_POSITION;
			};

			struct SurfaceInfo {
				float3 baseColor;
				float3 positionWS;

				float3 ambient;
				float diffuse;
				float specular;
				float shininess;
				float3 normal;
			};

			static const int kMaxLightCount = 8;

			int		g_MyLightCount;
			float4	g_MyLightColor[kMaxLightCount];
			float4	g_MyLightPos[kMaxLightCount];
			float4	g_MyLightDir[kMaxLightCount];
			float4	g_MyLightParam[kMaxLightCount];

			float4 computeLighting(SurfaceInfo s) {
				float3 viewDir = normalize(_WorldSpaceCameraPos - s.positionWS);

				int lightCount = min(kMaxLightCount, g_MyLightCount);
				float4 o = float4(s.baseColor * s.ambient, 1);

				for (int j = 0; j < lightCount; j++) {
					float3 lightColor     = g_MyLightColor[j].rgb;
					float  lightIntensity = g_MyLightColor[j].a;

					float3 lightPos       = g_MyLightPos[j].xyz;

					float3 lightDir       = g_MyLightDir[j].xyz;
					float  isDirectional  = g_MyLightDir[j].w;

					float3 lightPosDir    = s.positionWS - lightPos;

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

			//float  MyGBuffer_depth;
			TEXTURE2D(MyGBuffer_baseColor);
			TEXTURE2D(MyGBuffer_positionWS);
			TEXTURE2D(MyGBuffer_normalWS);

			SAMPLER(sampler_MyGBuffer_baseColor);
			SAMPLER(sampler_MyGBuffer_positionWS);
			SAMPLER(sampler_MyGBuffer_normalWS);

			static const int GBufferDebugMode_None       = 0;
			static const int GBufferDebugMode_BaseColor  = 1;
			static const int GBufferDebugMode_PositionWS = 2;
			static const int GBufferDebugMode_NormalWS   = 3;
			static const int GBufferDebugMode_LightOnly  = 4;

			int _MyGBuffer_DebugMode;

			Varyings vert (Attributes i) {
				Varyings o;
//				o.positionHCS = TransformObjectToHClip(i.positionOS.xyz);
				o.positionHCS = i.positionOS;
				return o;
			}

			float4 frag (Varyings i) : SV_Target {
				float2 screenUV = i.positionHCS.xy / _ScaledScreenParams.xy;
				
			// _ProjectionParams 
			//   - x is 1.0 (or –1.0 if currently rendering with a flipped projection matrix),
			//   - y is the camera’s near plane,
			//   - z is the camera’s far plane and w is 1/FarPlane.

				if (_ProjectionParams.x > 0) {
					//screenUV.y = 1 - screenUV.y;
				}

				float4 baseColor  = SAMPLE_TEXTURE2D(MyGBuffer_baseColor,  sampler_MyGBuffer_baseColor,  screenUV);
				float4 positionWS = SAMPLE_TEXTURE2D(MyGBuffer_positionWS, sampler_MyGBuffer_positionWS, screenUV);
				float4 normalWS   = SAMPLE_TEXTURE2D(MyGBuffer_normalWS,   sampler_MyGBuffer_normalWS,   screenUV);

				switch (_MyGBuffer_DebugMode) {
					case GBufferDebugMode_BaseColor:	return float4(baseColor.rgb,  1);
					case GBufferDebugMode_PositionWS:	return float4(positionWS.xyz, 1);
					case GBufferDebugMode_NormalWS:		return float4(normalWS.xyz,   1);
					case GBufferDebugMode_LightOnly: {
						baseColor.rgb = float3(1,1,1);
					} break;
				}

				SurfaceInfo s;
				s.baseColor  = baseColor.rgb;
				s.positionWS = positionWS.xyz;
				s.normal     = normalWS.xyz;

				s.ambient   = 0;
				s.diffuse   = baseColor.a;
				s.specular  = positionWS.a;
				s.shininess = normalWS.a;

				return computeLighting(s);
			}
			ENDHLSL
		}
	}
}
