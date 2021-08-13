using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteInEditMode]
public abstract class MyPostProcessComponent : MonoBehaviour {
	public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
	public RenderTargetIdentifier cameraColorTarget;

	public virtual void OnEnable() {
		Debug.Log("MyPostProcessVolumeMonoBehaviour.OnEnable");
		MyPostProcessManager.instance.Register(this);
	}

	public virtual void OnDisable() {
		MyPostProcessManager.instance.Unregister(this);
	}

//	RenderTargetHandle tempTexture;

	public virtual void OnVolumeRenderConfigure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {
//		cmd.GetTemporaryRT(tempTexture.id, cameraTextureDescriptor);
	}

	public virtual void OnVolumeRenderFrameCleanup(CommandBuffer cmd) {
//		cmd.ReleaseTemporaryRT(tempTexture.id);
	}

	public virtual void OnVolumeRenderExecute(ScriptableRenderContext context, ref RenderingData renderingData) {
		/*
		CommandBuffer cmd = CommandBufferPool.Get(GetType().Name);
		cmd.Clear();
		cmd.Blit(cameraColor, tempTexture.Identifier(), material, 0);
		cmd.Blit(tempTexture.Identifier(), cameraColor);
		context.ExecuteCommandBuffer(cmd);
		cmd.Clear();
		CommandBufferPool.Release(cmd);
		*/
	}

}
