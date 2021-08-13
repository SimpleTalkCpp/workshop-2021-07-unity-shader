using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MyPostProcessSimple : MyPostProcessBase
{
	public Material material;
	static Mesh fullScreenTriangle;

	override public void OnPostProcessExecute(ScriptableRenderContext context, ref RenderingData renderingData) {
		if (!material) return;

		if (!fullScreenTriangle) {
			fullScreenTriangle = new Mesh() {
				name = "MyPostProcessSimple",
				vertices = new Vector3[] {
					new Vector3(-1, -1, 0),
					new Vector3( 3, -1, 0),
					new Vector3(-1,  3, 0),
				},
				triangles = new int[] {0,1,2}
			};
			fullScreenTriangle.UploadMeshData(true);
		}

		CommandBuffer cmd = CommandBufferPool.Get(GetType().Name);
		cmd.Clear();
		cmd.DrawMesh(fullScreenTriangle, Matrix4x4.identity, material);
		context.ExecuteCommandBuffer(cmd);
		cmd.Clear();
		CommandBufferPool.Release(cmd);
	}
}
