using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MyPostProcessBlit : MyPostProcessBase
{
	public Material material;
	RenderTargetHandle tempTexture;

	override public void OnVolumeRenderConfigure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {
		cmd.GetTemporaryRT(tempTexture.id, cameraTextureDescriptor);
	}

	override public void OnVolumeRenderFrameCleanup(CommandBuffer cmd) {
		cmd.ReleaseTemporaryRT(tempTexture.id);
	}

	override public void OnVolumeRenderExecute(ScriptableRenderContext context, ref RenderingData renderingData) {
		if (!material) return;

		CommandBuffer cmd = CommandBufferPool.Get(GetType().Name);
		cmd.Clear();
		cmd.Blit(cameraColorTarget, tempTexture.Identifier(), material, 0);
		cmd.Blit(tempTexture.Identifier(), cameraColorTarget);
		context.ExecuteCommandBuffer(cmd);
		cmd.Clear();
		CommandBufferPool.Release(cmd);
	}
}
