using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Week003_WorldScannerRenderPass : ScriptableRenderPass
{
	public Material material;
	public RenderTargetIdentifier cameraColor;

	RenderTargetHandle tempTexture;
	string profilerTag;

	public struct SetupParam {
		RenderTargetIdentifier cameraColorTargetIdent;
	}

	public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {
		cmd.GetTemporaryRT(tempTexture.id, cameraTextureDescriptor);
	}
	public override void FrameCleanup(CommandBuffer cmd) {
		cmd.ReleaseTemporaryRT(tempTexture.id);
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
//		Debug.Log("test");

		if (!material) return;

		CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
		cmd.Clear();
		cmd.Blit(cameraColor, tempTexture.Identifier(), material, 0);
		cmd.Blit(tempTexture.Identifier(), cameraColor);
		context.ExecuteCommandBuffer(cmd);
		cmd.Clear();
		CommandBufferPool.Release(cmd);
	}
}

public class Week003_WorldScannerRendererFeature : ScriptableRendererFeature
{
	public Material material;
	public RenderPassEvent WhenToInsert = RenderPassEvent.BeforeRenderingPostProcessing;

	Week003_WorldScannerRenderPass	m_pass;

	public override void Create() {
		m_pass = new Week003_WorldScannerRenderPass();
		m_pass.renderPassEvent = WhenToInsert;
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
		var volMgr = VolumeManager.instance;
		if (volMgr == null) return;
		LayerMask layerMask = -1;
		var volArr = volMgr.GetVolumes(layerMask);

		foreach (var vol in volArr) {
			
		}

		m_pass.cameraColor = renderer.cameraColorTarget;
		m_pass.material = material;
		renderer.EnqueuePass(m_pass);
	}
}
