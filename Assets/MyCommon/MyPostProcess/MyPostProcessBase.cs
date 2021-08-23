using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteInEditMode]
public abstract class MyPostProcessBase : MonoBehaviour {
	public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
	public RenderTargetIdentifier cameraColorTarget;
	static Mesh fullScreenTriangle;

	public virtual void OnEnable() {
//		Debug.Log("MyPostProcessVolumeMonoBehaviour.OnEnable");
		MyPostProcessManager.instance.Register(this);
	}

	public virtual void OnDisable() {
		MyPostProcessManager.instance.Unregister(this);
	}

	public virtual void OnPostProcessConfigure(MyPostProcessRenderPass pass, CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {
	}

	public virtual void OnPostProcessFrameCleanup(MyPostProcessRenderPass pass, CommandBuffer cmd) {
	}

	public virtual void OnPostProcessExecute(MyPostProcessRenderPass pass, ScriptableRenderContext context, ref RenderingData renderingData) {
	}

	static public Mesh GetFullScreenTriangleMesh() {
		if (!fullScreenTriangle) {
			fullScreenTriangle = new Mesh() {
				name = "Week004_ProjectionUpdater",
				vertices = new Vector3[] {
					new Vector3(-1, -1, 0),
					new Vector3( 3, -1, 0),
					new Vector3(-1,  3, 0),
				},
				triangles = new int[] {0,1,2}
			};
			fullScreenTriangle.UploadMeshData(true);
		}

		return fullScreenTriangle;
	}
}
