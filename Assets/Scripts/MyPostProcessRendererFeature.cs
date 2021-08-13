using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MyPostProcessRendererFeature : ScriptableRendererFeature
{
	public override void Create() {
//		Debug.Log("MyPostProcessManager.RendererFeature.Create");
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
//		Debug.Log("MyPostProcessManager.RendererFeature.AddRenderPasses");
		var mgr = MyPostProcessManager.instance;
		if (mgr == null) return;
		mgr.AddRenderPasses(renderer, ref renderingData);
	}
}

public class MyPostProcessRenderPass : ScriptableRenderPass
{
	MyPostProcessComponent m_volume;

	public MyPostProcessRenderPass(MyPostProcessComponent vol) {
		m_volume = vol;
	}

	public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {
		if (m_volume) {
			m_volume.OnVolumeRenderConfigure(cmd, cameraTextureDescriptor);
		}
	}
	public override void FrameCleanup(CommandBuffer cmd) {
		if (m_volume) {
			m_volume.OnVolumeRenderFrameCleanup(cmd);
		}
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
		if (m_volume) {
			m_volume.OnVolumeRenderExecute(context, ref renderingData);
		}
	}
}

public class MyPostProcessManager
{
	static MyPostProcessManager s_instance;
	static public MyPostProcessManager instance {
		get {
			if (s_instance == null) {
				s_instance = new MyPostProcessManager();
			}
			return s_instance;
		}
	}

	public void Register(MyPostProcessComponent vol) {
		m_volumes.Add(vol, new MyPostProcessRenderPass(vol));
	}

	public void Unregister(MyPostProcessComponent vol) {
		if (m_volumes.ContainsKey(vol)) {
			m_volumes.Remove(vol);
		}
	}

	internal void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
		foreach (var t in m_volumes) {
			if (t.Key == null || t.Value == null) continue;
			t.Key.cameraColorTarget = renderer.cameraColorTarget;
			renderer.EnqueuePass(t.Value);
		}
	}

	Dictionary<MyPostProcessComponent, MyPostProcessRenderPass> m_volumes = new Dictionary<MyPostProcessComponent, MyPostProcessRenderPass>();
}

