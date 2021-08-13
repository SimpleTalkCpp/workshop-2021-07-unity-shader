using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MyPostProcessSimple : MyPostProcessBase
{
	public Material material;
	static Mesh mesh;

	override public void OnVolumeRenderExecute(ScriptableRenderContext context, ref RenderingData renderingData) {
		if (!material) return;

		if (!mesh) {
			mesh = new Mesh();
			mesh.name = "MyPostProcessSimple";
			mesh.vertices = new Vector3[] {
				new Vector3(-1,-1,0),
				new Vector3( 1,-1,0),
				new Vector3(-1, 1,0),
				new Vector3( 1, 1,0),
			};
			mesh.triangles = new int[] {0,1,2, 2,1,3};
		}

		CommandBuffer cmd = CommandBufferPool.Get(GetType().Name);
		cmd.Clear();
		cmd.DrawMesh(mesh, Matrix4x4.identity, material);
		context.ExecuteCommandBuffer(cmd);
		cmd.Clear();
		CommandBufferPool.Release(cmd);
	}
}
