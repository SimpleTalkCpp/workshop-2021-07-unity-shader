using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MyPostProcessSimple : MyPostProcessBase
{
	public Material material;

	override public void OnPostProcessExecute(MyPostProcessRenderPass pass, ScriptableRenderContext context, ref RenderingData renderingData) {
		if (!material) return;
		CommandBuffer cmd = CommandBufferPool.Get(GetType().Name);
		cmd.Clear();
		cmd.DrawMesh(GetFullScreenTriangleMesh(), Matrix4x4.identity, material);
		context.ExecuteCommandBuffer(cmd);
		cmd.Clear();
		CommandBufferPool.Release(cmd);
	}
}
