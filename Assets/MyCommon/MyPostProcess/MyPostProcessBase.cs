using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteInEditMode]
public abstract class MyPostProcessBase : MonoBehaviour {
	public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
	public RenderTargetIdentifier cameraColorTarget;

	public virtual void OnEnable() {
//		Debug.Log("MyPostProcessVolumeMonoBehaviour.OnEnable");
		MyPostProcessManager.instance.Register(this);
	}

	public virtual void OnDisable() {
		MyPostProcessManager.instance.Unregister(this);
	}

	public virtual void OnPostProcessConfigure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {
	}

	public virtual void OnPostProcessFrameCleanup(CommandBuffer cmd) {
	}

	public virtual void OnPostProcessExecute(ScriptableRenderContext context, ref RenderingData renderingData) {
	}
}
