using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Week002_Trail : MonoBehaviour
{
	public int TargetFrameRate = 30;

	public Transform Source;

	public float Width = 1;
	public float Duration = 2;
	public float ErrorTolerance = 0.2f;

	[System.Serializable]
	public struct Node {
		public Vector3 pos0;
		public Vector3 pos1;
		public float time;
		public bool breakdown;
	}
	
	List<Node> Nodes = new List<Node>();
	int currentNode = 0;

	List<Vector3> meshVertices = new List<Vector3>();
	List<Vector2> meshUV0 = new List<Vector2>();
	List<ushort>  meshIndices  = new List<ushort>();

	MeshFilter meshFilter;
	MeshRenderer meshRenderer;
	Mesh mesh;

	void Start() {
		Application.targetFrameRate = TargetFrameRate;

		if (!meshFilter) {
			meshFilter = gameObject.GetComponent<MeshFilter>();
			if (!meshFilter) {
				meshFilter = gameObject.AddComponent<MeshFilter>();
				meshFilter.name = "MeshFilter";
			}
		}

		if (!meshRenderer) {
			meshRenderer = gameObject.GetComponent<MeshRenderer>();
			if (!meshRenderer) {
				meshRenderer = gameObject.AddComponent<MeshRenderer>();
				meshRenderer.name = "MeshRenderer";
			}
		}

		if (!mesh) {
			mesh = new Mesh();
			mesh.name = "Trail Mesh";
		}
	}

#if true
	void OnDrawGizmos_Node(Node node) {
		Gizmos.color = node.breakdown ? Color.blue : Color.red;
		Gizmos.DrawLine(node.pos0, node.pos1);
	}

	void OnDrawGizmos() {
		int n = Nodes.Count;
		for (int i = currentNode; i < n; i++) {
			OnDrawGizmos_Node(Nodes[i]);
		}
	}
#endif

	void UpdateNode() {
		// remove timed out nodes
		float time = Time.time;
		int n = Nodes.Count;
		for (int i = currentNode; i < n; i++) {
			if (Nodes[i].time + Duration < time) {
				currentNode = i + 1;
			}
		}

		if (currentNode >= Nodes.Count) {
			Nodes.Clear();
			currentNode = 0;
			
		} else {
			int usedCount = Nodes.Count - currentNode;
			if (Nodes.Count > 32 && currentNode > usedCount * 4) {
				Nodes.RemoveRange(0, currentNode);
				currentNode = 0;
			}
		}

		// add new node
		if (Source) {
			var node = new Node();
			node.time = time;
			node.pos0 = Source.position;
			node.pos1 = Source.position + Source.right * Width;

			AddNode(node, 0);
		}
	}

	void AddNode(Node node, int iter) {
		if (iter > 16) return;

		if (Nodes.Count == 0) {
			Nodes.Add(node);
			return;
		}

		var last = Nodes[Nodes.Count - 1];
		var mid0 = (last.pos0 + node.pos0) / 2;
		var mid1 = (last.pos1 + node.pos1) / 2;

		var v = mid1 - mid0;
		var diff = Mathf.Abs(v.sqrMagnitude - Width);

		var e2 = ErrorTolerance * ErrorTolerance;
		if (diff < e2) {
			Nodes.Add(node);
			return;
		}

		var mid = new Node();
		mid.breakdown = true;
		mid.time = (last.time + node.time) / 2;
		mid.pos0 = mid0;
		mid.pos1 = mid0 + v.normalized * Width;

		AddNode(mid, iter + 1);
		AddNode(node, iter + 1);
	}

	void UpdateMesh() {
		meshFilter.sharedMesh = mesh;

		int n = Nodes.Count - currentNode;
		if (n <= 0) return;

		meshVertices.Clear();
		meshUV0.Clear();
		meshIndices.Clear();
		mesh.SetIndices(meshIndices, MeshTopology.Triangles, 0);

		for (int i = 0; i < n; i++) {
			var node = Nodes[currentNode + i];
			meshVertices.Add(node.pos0);
			meshVertices.Add(node.pos1);

//			var u = Mathf.Clamp01((Time.time - node.time) / Duration);
			var u = node.time / Duration;
			meshUV0.Add(new Vector2(u, 0));
			meshUV0.Add(new Vector2(u, 1));

			if (i > 0) {
				int vi = (i - 1) * 2;
				meshIndices.Add((ushort)(vi));
				meshIndices.Add((ushort)(vi + 2));
				meshIndices.Add((ushort)(vi + 1));
				meshIndices.Add((ushort)(vi + 1));
				meshIndices.Add((ushort)(vi + 2));
				meshIndices.Add((ushort)(vi + 3));
			}
		}

		mesh.SetVertices(meshVertices);
		mesh.SetUVs(0, meshUV0);
		mesh.SetIndices(meshIndices, MeshTopology.Triangles, 0);
	}

	void LateUpdate() {
		UpdateNode();
		UpdateMesh();
	}
}
