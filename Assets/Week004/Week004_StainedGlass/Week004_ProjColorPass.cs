using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteInEditMode]
public class Week004_ProjColorPass : MyPostProcessBase
{
	public Camera ProjectorCamera;
	public RenderTexture renderTarget;

	override public void OnPostProcessExecute(MyPostProcessRenderPass pass, ScriptableRenderContext context, ref RenderingData renderingData) {
		if (!ProjectorCamera) return;

		if (renderingData.cameraData.camera != ProjectorCamera) 
			return;

		CommandBuffer cmd = CommandBufferPool.Get("Week004_Projection_CopyProjDepthTex");
		cmd.Clear();
		cmd.ClearRenderTarget(false, true, Color.black);

		context.ExecuteCommandBuffer(cmd);
		cmd.Clear();
		CommandBufferPool.Release(cmd);
	}
}
