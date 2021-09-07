using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
[ExecuteInEditMode]
public class Week006_WaterInBottle : MonoBehaviour
{
	public Material waterInBottleMaterial;

	public float waterLevel = 0;

	Vector3 waterPivot;
	Vector4 _WaterPlane = new Vector4(0,1,0,0);

	Vector3 PendulumVelocity = Vector3.zero;
	Vector3 PendulumPos = new Vector3(0, -1, 0);
	Vector3 lastPos;

	public float damping = 0.9f;
	public float gravity = 9.8f;

	private void OnDrawGizmos() {
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(waterPivot, new Vector3(1,0,1));
	}

	private void Start() {
		_WaterPlane = new Vector4(0,1,0,0);
		lastPos = transform.position;
	}

	private void FixedUpdate() {
		var t = Time.fixedDeltaTime;
		if (t <= 0.0001f) return;

		PendulumVelocity *= damping;
		PendulumVelocity += (lastPos - transform.position) / t;
		PendulumVelocity += new Vector3(0, gravity, 0);

		var N = PendulumPos.normalized;

		PendulumVelocity -= N * Vector3.Dot(N, PendulumVelocity);

		lastPos = transform.position;
		PendulumPos += PendulumVelocity * t;

		var w = _WaterPlane.w;
		_WaterPlane = -PendulumPos.normalized;
		_WaterPlane.w = w;
	}

	void Update()
	{
		waterPivot = transform.position + Vector3.up * waterLevel;
		_WaterPlane.w = Vector3.Dot(_WaterPlane, waterPivot);

		if (waterInBottleMaterial) {
			waterInBottleMaterial.SetVector("_WaterPlane", _WaterPlane);
			waterInBottleMaterial.SetVector("_WaterPivot", waterPivot);
		}
	}
}
