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
	MyPostProcessBase m_myPostProcessBase;

	public MyPostProcessRenderPass(MyPostProcessBase vol) {
		m_myPostProcessBase = vol;
	}

	public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {
		if (m_myPostProcessBase) {
			m_myPostProcessBase.OnPostProcessConfigure(this, cmd, cameraTextureDescriptor);
		}
	}
	public override void FrameCleanup(CommandBuffer cmd) {
		if (m_myPostProcessBase) {
			m_myPostProcessBase.OnPostProcessFrameCleanup(this, cmd);
		}
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
		if (m_myPostProcessBase) {
			m_myPostProcessBase.OnPostProcessExecute(this, context, ref renderingData);
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

	public void Register(MyPostProcessBase vol) {
		m_passes.Add(vol, new MyPostProcessRenderPass(vol));
	}

	public void Unregister(MyPostProcessBase vol) {
		if (m_passes.ContainsKey(vol)) {
			m_passes.Remove(vol);
		}
	}

	internal void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
		foreach (var t in m_passes) {
			if (t.Key == null || t.Value == null) continue;
			t.Key.cameraColorTarget = renderer.cameraColorTarget;
			t.Value.renderPassEvent = t.Key.renderPassEvent;
			renderer.EnqueuePass(t.Value);
		}

		if (OnAddRenderPasses != null)
			OnAddRenderPasses(renderer, ref renderingData);
	}

	public delegate void OnAddRenderPassesDelegate(ScriptableRenderer renderer, ref RenderingData renderingData);
	public event OnAddRenderPassesDelegate OnAddRenderPasses;

	Dictionary<MyPostProcessBase, MyPostProcessRenderPass> m_passes = new Dictionary<MyPostProcessBase, MyPostProcessRenderPass>();
}

