using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteInEditMode]
public class Week004_CopyDepth : MyPostProcessBase
{
	public Camera ProjectorCamera;
	public Material copyDepthMaterial;
	public RenderTexture renderTarget;

	override public void OnPostProcessExecute(MyPostProcessRenderPass pass, ScriptableRenderContext context, ref RenderingData renderingData) {
		if (!ProjectorCamera) return;
		if (!copyDepthMaterial) return;

		if (renderingData.cameraData.camera != ProjectorCamera) 
			return;

		CommandBuffer cmd = CommandBufferPool.Get("Week004_Projection_CopyProjDepthTex");
		cmd.Clear();
//		cmd.ClearRenderTarget(false, true, Color.black);

		cmd.Blit(pass.depthAttachment, new RenderTargetIdentifier(renderTarget), copyDepthMaterial);
//		pass.Blit(cmd, new RenderTargetIdentifier("_CameraDepthAttachment"), projDepthTex.Identifier());

//		cmd.Blit(pas	s.colorAttachment, projColorTex.Identifier());
//		cmd.Blit(pass.depthAttachment, projDepthTex.Identifier());
//		cmd.Blit(new RenderTargetIdentifier("_CameraDepthAttachment"), new RenderTargetIdentifier(depthTex));
//		renderingData.cameraData.targetTexture

		context.ExecuteCommandBuffer(cmd);
		cmd.Clear();
		CommandBufferPool.Release(cmd);
	}
}
