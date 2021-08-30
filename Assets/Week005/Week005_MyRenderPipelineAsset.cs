using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Week005/MyRenderPipelineAsset")]
public class Week005_MyRenderPipelineAsset : RenderPipelineAsset {

	public enum GBufferDebugMode {
		None       = 0,
		BaseColor  = 1,
		PositionWS = 2,
		NormalWS   = 3,
		LightOnly  = 4,
	};

	public GBufferDebugMode gbufferDebugMode = GBufferDebugMode.None;

	protected override RenderPipeline CreatePipeline() {
		return new Week005_MyRenderPipeline(this);
	}
}
