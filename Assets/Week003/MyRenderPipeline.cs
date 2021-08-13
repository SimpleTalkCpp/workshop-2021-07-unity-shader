using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MyRenderPipeline : RenderPipeline
{
	UniversalRenderPipeline basePipeline;

	public MyRenderPipeline(UniversalRenderPipelineAsset asset) {
		basePipeline = new UniversalRenderPipeline(asset);
	}

	override protected void Render (ScriptableRenderContext context, Camera[] cameras) {
//		basePipeline.Render();
	}
}

[CreateAssetMenu(menuName = "Rendering/MyRenderPipelineAsset", fileName = "MyRenderPipelineAsset")]
public class MyRenderPipelineAsset : UniversalRenderPipelineAsset { // RenderPipelineAsset {
	protected override RenderPipeline CreatePipeline () {
		return new MyRenderPipeline(this);
	}
}