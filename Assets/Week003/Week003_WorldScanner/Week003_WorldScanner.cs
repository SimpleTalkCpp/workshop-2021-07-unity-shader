using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Week003_WorldScanner : MyPostProcessComponent
{
	public Material material;
	
	RenderTargetHandle tempTexture;

	public override void OnVolumeRenderConfigure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {
		cmd.GetTemporaryRT(tempTexture.id, cameraTextureDescriptor);
	}

	public override void OnVolumeRenderFrameCleanup(CommandBuffer cmd) {
		cmd.ReleaseTemporaryRT(tempTexture.id);
	}

	override public void OnVolumeRenderExecute(ScriptableRenderContext context, ref RenderingData renderingData) {
//		Debug.Log("test");

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
