using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Week005_MyRenderPipeline : RenderPipeline {
	public class LightSetup {
		public Light[] lights;

		List<Vector4>	_LightColor = new List<Vector4>();
		List<Vector4>	_LightPos   = new List<Vector4>();
		List<Vector4>	_LightDir   = new List<Vector4>();
		List<Vector4>	_LightParam = new List<Vector4>();

		const int kMaxLightCount = 8;

		public void Update() {
			lights = Object.FindObjectsOfType<Light>();

			_LightPos.Clear();
			_LightDir.Clear();
			_LightColor.Clear();
			_LightParam.Clear();
			foreach (var li in lights) {
				Vector4 col = li.color;
				col.w = li.intensity;

				Vector4 pos = li.transform.position;
				pos.w = 1;

				Vector4 dir = li.transform.forward;
				dir.w = li.type == LightType.Directional ? 1 : 0;

				Vector4 param = Vector4.zero;
				if (li.type == LightType.Spot) {
					param.x = 1;
					param.y = Mathf.Cos(li.spotAngle      * Mathf.Deg2Rad * 0.5f);
					param.z = Mathf.Cos(li.innerSpotAngle * Mathf.Deg2Rad * 0.5f);
				}
				param.w = li.range;

				_LightColor.Add(col);
				_LightPos.Add(pos);
				_LightDir.Add(dir);
				_LightParam.Add(param);
			}

			for (int i = _LightPos.Count; i < kMaxLightCount; i++) {
				_LightColor.Add(Vector4.zero);
				_LightDir.Add(Vector4.zero);
				_LightPos.Add(Vector4.zero);
				_LightParam.Add(Vector4.zero);
			}

			Shader.SetGlobalInt("g_MyLightCount", lights.Length);
			Shader.SetGlobalVectorArray("g_MyLightColor", _LightColor);
			Shader.SetGlobalVectorArray("g_MyLightPos",   _LightPos);
			Shader.SetGlobalVectorArray("g_MyLightDir",   _LightDir);
			Shader.SetGlobalVectorArray("g_MyLightParam", _LightParam);
		}
	}

	public class CameraRenderer {
		ScriptableRenderContext m_context;
		Camera m_camera;
		CullingResults m_cullingResults;
		Week005_MyRenderPipelineAsset m_pipelineAsset;

		CommandBuffer m_cmd = new CommandBuffer();

		public struct ShaderTags {
// https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@11.0/manual/urp-shaders/urp-shaderlab-pass-tags.html#urp-pass-tags-lightmode
			public static readonly ShaderTagId UniversalForward		= new ShaderTagId("UniversalForward");
			public static readonly ShaderTagId UniversalGBuffer		= new ShaderTagId("UniversalGBuffer");
			public static readonly ShaderTagId UniversalForwardOnly	= new ShaderTagId("UniversalForwardOnly");
			public static readonly ShaderTagId Universal2D			= new ShaderTagId("Universal2D");
			public static readonly ShaderTagId ShadowCaster			= new ShaderTagId("ShadowCaster");
			public static readonly ShaderTagId DepthOnly			= new ShaderTagId("DepthOnly");
			public static readonly ShaderTagId Meta					= new ShaderTagId("Meta");
			public static readonly ShaderTagId SRPDefaultUnlit		= new ShaderTagId("SRPDefaultUnlit");
		//----
			public static readonly ShaderTagId MyGBuffer			= new ShaderTagId("MyGBuffer");
		}

		public void Render (ScriptableRenderContext context, Camera camera, Week005_MyRenderPipelineAsset pipelineAsset) {
			m_context = context;
			m_camera = camera;
			m_pipelineAsset = pipelineAsset;
			m_cmd.name = "Week005_MyRenderPipeline";

			m_context.SetupCameraProperties(m_camera);

			bool clearDepth = m_camera.clearFlags >= CameraClearFlags.Depth;
			bool clearColor = m_camera.clearFlags >= CameraClearFlags.Color;
			m_cmd.ClearRenderTarget(clearDepth, clearColor, clearColor ? m_camera.backgroundColor.linear : Color.clear);

//			if (camera.cameraType == CameraType.SceneView) {
//				ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
//			}

			if (camera.TryGetCullingParameters(out var cullParam)) {
				m_cullingResults = context.Cull(ref cullParam);
//				Render_Unlit_Opaque();

			// Deferred render
				Render_GBuffer_Setup();
				Render_GBuffer_Opaque();
				Render_GBuffer_Ligthing();
				Render_GBuffer_Release();
			//-------
				context.DrawSkybox(camera);
				DrawUnsupportedShaders();
			// Transparent - always do Forward render
				Render_Transparent();
			}

			DrawGizmos();

			context.ExecuteCommandBuffer(m_cmd);
			m_cmd.Clear();
			context.Submit();
		}

		public void DrawRenderers(ref DrawingSettings drawingSettings, ref FilteringSettings filteringSettings) {
			m_context.ExecuteCommandBuffer(m_cmd);
			m_cmd.Clear();
			// ensure execute all commands before DrawRenderers()
			m_context.DrawRenderers(m_cullingResults, ref drawingSettings, ref filteringSettings);
		}

		void Render_Unlit_Opaque() {
			var sortingSettings = new SortingSettings(m_camera) {
				criteria = SortingCriteria.CommonOpaque
			};
			var filteringSettings = new FilteringSettings(RenderQueueRange.opaque) {
				layerMask = m_camera.cullingMask
			};

			var drawingSettings = new DrawingSettings(ShaderTags.SRPDefaultUnlit, sortingSettings);
			DrawRenderers(ref drawingSettings, ref filteringSettings);
		}

		public struct GBufferIds {
			public static readonly int baseColor   = Shader.PropertyToID("MyGBuffer_baseColor");
			public static readonly int positionWS  = Shader.PropertyToID("MyGBuffer_positionWS");
			public static readonly int normalWS    = Shader.PropertyToID("MyGBuffer_normalWS");
		}

		static readonly RenderTargetBinding s_GBufferTargetBinding = new RenderTargetBinding() {
			colorRenderTargets = new RenderTargetIdentifier[]  { GBufferIds.baseColor,            GBufferIds.positionWS,           GBufferIds.normalWS },
			colorLoadActions   = new RenderBufferLoadAction[]  { RenderBufferLoadAction.DontCare, RenderBufferLoadAction.DontCare, RenderBufferLoadAction.DontCare },
			colorStoreActions  = new RenderBufferStoreAction[] { RenderBufferStoreAction.Store,   RenderBufferStoreAction.Store,   RenderBufferStoreAction.Store },
			depthRenderTarget  = BuiltinRenderTextureType.CameraTarget, // don't change depth buffer
			depthLoadAction    = RenderBufferLoadAction.DontCare,
			depthStoreAction   = RenderBufferStoreAction.Store
		};

		void Render_GBuffer_Setup() {
			
			m_cmd.name = "Render_GBuffer";

			var desc = new RenderTextureDescriptor(m_camera.pixelWidth, m_camera.pixelHeight, RenderTextureFormat.Default, 0);
			desc.colorFormat = RenderTextureFormat.Default;  m_cmd.GetTemporaryRT(GBufferIds.baseColor,  desc, FilterMode.Point);
			desc.colorFormat = RenderTextureFormat.ARGBHalf; m_cmd.GetTemporaryRT(GBufferIds.positionWS, desc, FilterMode.Point);
			desc.colorFormat = RenderTextureFormat.ARGBHalf; m_cmd.GetTemporaryRT(GBufferIds.normalWS,   desc, FilterMode.Point);

			// P.S. Unity HDRP - Deferred Shading rendering path
			//    RT0, ARGB32 format: Diffuse color (RGB), occlusion (A).
			//    RT1, ARGB32 format: Specular color (RGB), roughness (A).
			//    RT2, ARGB2101010 format: World space normal (RGB), unused (A).
			//    RT3, ARGB2101010 (non-HDR) or ARGBHalf (HDR) format: Emission + lighting + lightmaps + reflection probes buffer.
			//    Depth+Stencil buffer
			// from: https://docs.unity3d.com/Manual/RenderTech-DeferredShading.html

			m_cmd.SetRenderTarget(s_GBufferTargetBinding);
			m_cmd.ClearRenderTarget(true, true, Color.clear);
		}

		public static readonly int s_ProjectionParams = Shader.PropertyToID("_ProjectionParams");
		public static readonly int s_MyGBufferCull    = Shader.PropertyToID("_MyGBufferCull");
		
		void Render_GBuffer_Opaque() {
			var _ProjectionParams = new Vector4(1, m_camera.nearClipPlane, 1 / m_camera.farClipPlane);
			int _MyGBufferCull = 2;

			if (m_camera.cameraType == CameraType.Game) {
				// Work around for RenderTaget Upside down on Game Camera
				//     the Render Textures are likely to come out at different vertical orientations in Direct3D-like platforms
				//     and when you use **anti-aliasing**. (SceneView has no anti-aliasing)
				//     To standardize the coordinates, you need to manually “flip” the screen Texture upside down in your Vertex Shader
				//     so that it matches the OpenGL-like coordinate standard.
				// from: https://docs.huihoo.com/unity/5.5/Documentation/Manual/SL-PlatformDifferences.html
				_MyGBufferCull = 1;

				// _ProjectionParams 
				//   - x is 1.0 (or –1.0 if currently rendering with a flipped projection matrix),
				_ProjectionParams.x = -1;
			}

			m_cmd.SetGlobalInt(s_MyGBufferCull, _MyGBufferCull);
			m_cmd.SetGlobalVector(s_ProjectionParams, _ProjectionParams);

			var sortingSettings = new SortingSettings(m_camera) {
				criteria = SortingCriteria.CommonOpaque
			};
			var filteringSettings = new FilteringSettings(RenderQueueRange.opaque) {
				layerMask = m_camera.cullingMask
			};
			var drawingSettings = new DrawingSettings(ShaderTags.MyGBuffer, sortingSettings);
			DrawRenderers(ref drawingSettings, ref filteringSettings);
		}

		Shader   m_GBufferLigthingShader = Shader.Find("Week005/Week005_Deferred_Phong");
		Material m_GBufferLigthingMaterial;

		public static readonly int s_scaledScreenParams  = Shader.PropertyToID("_ScaledScreenParams");
		public static readonly int s_MyGBuffer_DebugMode = Shader.PropertyToID("_MyGBuffer_DebugMode");

		void Render_GBuffer_Ligthing() {
			m_cmd.name = "Render_GBuffer_Ligthing";
			m_cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);

			if (!m_GBufferLigthingMaterial) {
				m_GBufferLigthingMaterial = new Material(m_GBufferLigthingShader);
			}

			m_cmd.SetGlobalVector(s_scaledScreenParams, new Vector4(m_camera.pixelWidth, m_camera.pixelHeight));
			m_cmd.SetGlobalInt(s_MyGBuffer_DebugMode, (int)m_pipelineAsset.gbufferDebugMode);

			m_cmd.SetGlobalTexture(GBufferIds.baseColor,  GBufferIds.baseColor);
			m_cmd.SetGlobalTexture(GBufferIds.positionWS, GBufferIds.positionWS);
			m_cmd.SetGlobalTexture(GBufferIds.positionWS, GBufferIds.positionWS);

			m_cmd.DrawMesh(MyPostProcessBase.GetFullScreenTriangleMesh(), Matrix4x4.identity, m_GBufferLigthingMaterial);

			m_context.ExecuteCommandBuffer(m_cmd);
			m_cmd.Clear();
		}

		void Render_GBuffer_Release() {
			m_cmd.ReleaseTemporaryRT(GBufferIds.baseColor);
			m_cmd.ReleaseTemporaryRT(GBufferIds.positionWS);
			m_cmd.ReleaseTemporaryRT(GBufferIds.normalWS);
		}

		void Render_Transparent() {
			var sortingSettings = new SortingSettings(m_camera) {
				criteria = SortingCriteria.CommonTransparent
			};
			var filteringSettings = new FilteringSettings(RenderQueueRange.transparent) {
				layerMask = m_camera.cullingMask
			};
			var drawingSettings = new DrawingSettings(ShaderTags.SRPDefaultUnlit, sortingSettings);
			DrawRenderers(ref drawingSettings, ref filteringSettings);
		}

		static Material s_errorMaterial;
		static ShaderTagId[] s_legacyShaderTagIds = {
			new ShaderTagId("Always"),
			new ShaderTagId("ForwardBase"),
			new ShaderTagId("PrepassBase"),
			new ShaderTagId("Vertex"),
			new ShaderTagId("VertexLMRGBM"),
			new ShaderTagId("VertexLM")
		};

		void DrawUnsupportedShaders () {
			if (!s_errorMaterial) {
				s_errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
			}
			var drawingSettings = new DrawingSettings(
				s_legacyShaderTagIds[0], new SortingSettings(m_camera)
			) {
				overrideMaterial = s_errorMaterial
			};

			for (int i = 1; i < s_legacyShaderTagIds.Length; i++) {
				drawingSettings.SetShaderPassName(i, s_legacyShaderTagIds[i]);
			}

			var filteringSettings = new FilteringSettings(RenderQueueRange.all);
			DrawRenderers(ref drawingSettings, ref filteringSettings);
		}

	#if UNITY_EDITOR
		void DrawGizmos () {
			if (UnityEditor.Handles.ShouldRenderGizmos()) {
				m_context.DrawGizmos(m_camera, GizmoSubset.PreImageEffects);
				m_context.DrawGizmos(m_camera, GizmoSubset.PostImageEffects);
			}
		}
	#endif
	}

	CameraRenderer renderer = new CameraRenderer();
	LightSetup lightSetup = new LightSetup();
	Week005_MyRenderPipelineAsset m_pipelineAsset;

	public Week005_MyRenderPipeline(Week005_MyRenderPipelineAsset pipelineAsset) {
		m_pipelineAsset = pipelineAsset;
	}

	protected override void Render(ScriptableRenderContext context, Camera[] cameras) {
		lightSetup.Update();

//		BeginFrameRendering(context, cameras);
		foreach (Camera camera in cameras) {
//			BeginCameraRendering(context, camera);
			renderer.Render(context, camera, m_pipelineAsset);
//			EndCameraRendering(context, camera);
		}
//		EndFrameRendering(context, cameras);
	}
}
