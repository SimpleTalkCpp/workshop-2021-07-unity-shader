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

	public RenderTexture projDepthTenderTarget;

	ProjClearColorPass projClearColorPass;
	ProjCopyDepthPass projCopyDepthPass;
	ProjectionPass projectionPass;

	private void OnEnable() {
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

			// Clear all Opaques color, only wants transparent objects
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
			ConfigureTarget(_owner.projDepthTenderTarget);
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
			if (renderingData.cameraData.camera != _owner.ProjectorCamera)
				return;

			if (!copyDepthMaterial) {
				copyDepthMaterial = new Material(_owner.copyDepthShader);
			}

			CommandBuffer cmd = CommandBufferPool.Get(GetType().Name);
			cmd.Clear();
			cmd.DrawMesh(MyPostProcessBase.GetFullScreenTriangleMesh(), Matrix4x4.identity, copyDepthMaterial);
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

			var projMat = GL.GetGPUProjectionMatrix(cam.projectionMatrix, false);
			var viewMat = cam.transform.worldToLocalMatrix;
			_owner.material.SetMatrix("_ProjVP", projMat * viewMat);

			var target = cam.targetTexture;

			CommandBuffer cmd = CommandBufferPool.Get(GetType().Name);
			cmd.Clear();
			_owner.material.SetTexture("_MyProjColorTex", target);
			_owner.material.SetTexture("_MyProjDepthTex", _owner.projDepthTenderTarget);

			cmd.DrawMesh(MyPostProcessBase.GetFullScreenTriangleMesh(), Matrix4x4.identity, _owner.material);
			context.ExecuteCommandBuffer(cmd);
			cmd.Clear();
			CommandBufferPool.Release(cmd);

		}
	}
}