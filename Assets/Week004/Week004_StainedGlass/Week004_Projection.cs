using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteInEditMode]
public class Week004_Projection : MonoBehaviour
{
	public Camera ProjectorCamera;
	public Material material;

	public Shader copyDepthShader;

//	public RenderTexture projColorTex;
	public RenderTexture projDepthTenderTarget;

	ProjClearColorPass projClearColorPass;
	ProjCopyDepthPass projCopyDepthPass;
	ProjectionPass projectionPass;

	RenderTargetHandle projColorTex;
	RenderTargetHandle projDepthTex;

	private void OnEnable() {
		projColorTex.Init("_MyProjColorTex");
		projDepthTex.Init("_MyProjDepthTex");

		projClearColorPass  = new ProjClearColorPass(this);
		projCopyDepthPass   = new ProjCopyDepthPass(this);
		projectionPass      = new ProjectionPass(this);

		MyPostProcessManager.instance.OnAddRenderPasses += AddRenderPasses;
	}

	private void OnDisable() {
		MyPostProcessManager.instance.OnAddRenderPasses -= AddRenderPasses;
	}

	void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
		renderer.EnqueuePass(projClearColorPass);
		renderer.EnqueuePass(projCopyDepthPass);
		renderer.EnqueuePass(projectionPass);
	}

	public class ProjClearColorPass : ScriptableRenderPass {
		Week004_Projection _owner;

		public ProjClearColorPass(Week004_Projection owner) {
			_owner = owner;
			renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
			if (renderingData.cameraData.camera != _owner.ProjectorCamera)
				return;

			CommandBuffer cmd = CommandBufferPool.Get(GetType().Name);
			cmd.Clear();
//			cmd.SetRenderTarget(_owner.projColorTex.Identifier());
			cmd.ClearRenderTarget(false, true, Color.black);
			context.ExecuteCommandBuffer(cmd);
			cmd.Clear();
			CommandBufferPool.Release(cmd);
		}
	}

	public class ProjCopyDepthPass : ScriptableRenderPass {
		Week004_Projection _owner;
		Material copyDepthMaterial;

		public ProjCopyDepthPass(Week004_Projection owner) {
			_owner = owner;
			renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
		}

		public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {
			var desc = cameraTextureDescriptor;
			cmd.GetTemporaryRT(_owner.projDepthTex.id, desc.width, desc.height, desc.depthBufferBits, FilterMode.Point, RenderTextureFormat.Depth);
		}

		public override void FrameCleanup(CommandBuffer cmd) {
			cmd.ReleaseTemporaryRT(_owner.projDepthTex.id);
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
			if (renderingData.cameraData.camera != _owner.ProjectorCamera)
				return;

			if (!copyDepthMaterial) {
				copyDepthMaterial = new Material(_owner.copyDepthShader);
			}

			CommandBuffer cmd = CommandBufferPool.Get(GetType().Name);
			cmd.Clear();

			var oldColor = colorAttachment;
			var oldDepth = depthAttachment;

//			Blit(cmd, depthAttachment, _owner.projDepthTex.Identifier(), copyDepthMaterial);
			Blit(cmd, colorAttachment, _owner.projDepthTenderTarget, copyDepthMaterial);

//			cmd.SetRenderTarget(_owner.projDepthTenderTarget);
//			cmd.DrawMesh(MyPostProcessBase.GetFullScreenTriangleMesh(), Matrix4x4.identity, _owner.material);

//			context.ExecuteCommandBuffer(cmd);

//			cmd.Blit(colorAttachment, _owner.projColorTex.Identifier(), );
//			cmd.Blit(depthAttachment, _owner.projDepthTex.Identifier());
//
//			cmd.Clear();
			cmd.SetRenderTarget(oldColor, oldDepth);
			context.ExecuteCommandBuffer(cmd);
			cmd.Clear();
			CommandBufferPool.Release(cmd);
		}
	}

	public class ProjectionPass : ScriptableRenderPass {
		Week004_Projection _owner;
		public ProjectionPass(Week004_Projection owner) {
			_owner = owner;
			renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing + 1;
		}
		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
			if (!_owner.ProjectorCamera) return;
			if (!_owner.material) return;

			var cam = _owner.ProjectorCamera;
			if (renderingData.cameraData.camera == cam) return;

			_owner.material.SetMatrix("_ProjVP", cam.projectionMatrix * cam.transform.worldToLocalMatrix);

			var target = cam.targetTexture;

			CommandBuffer cmd = CommandBufferPool.Get(GetType().Name);
			cmd.Clear();

			_owner.material.SetTexture("_MyProjColorTex", target);
			_owner.material.SetTexture("_MyProjDepthTex", _owner.projDepthTenderTarget);

	//		material.SetTexture("_MyProjDepthTex", depthTex);
	//		cmd.SetGlobalTexture("_MyProjColorTex", projColorTex.Identifier());
//			cmd.SetGlobalTexture("_MyProjColorTex", _owner.projColorTex.Identifier());
//			cmd.SetGlobalTexture(_owner.projDepthTex.id, _owner.projDepthTex.Identifier());

			cmd.DrawMesh(MyPostProcessBase.GetFullScreenTriangleMesh(), Matrix4x4.identity, _owner.material);
			context.ExecuteCommandBuffer(cmd);
			cmd.Clear();
			CommandBufferPool.Release(cmd);

		}
	}
}
