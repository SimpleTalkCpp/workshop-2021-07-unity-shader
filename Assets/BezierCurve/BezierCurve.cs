using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierCurve : MonoBehaviour
{
	public Vector3 p0;
	public Vector3 p1;
	public Vector3 p2;
	public Vector3 p3;

	public int div = 10;
	public float tol = 0.1f;

	public int pointCount = 0;

	Vector3 eval(float t) {
		float it  = 1 - t;
		float it2 = it * it;
		float t2  = t * t;

		Vector3 o = it2 * it * p0
				  + 3f  * it2 * t  * p1
				  + 3f  * it  * t2 * p2
				  + t   * t2  * p3;
		return o;
	}

	public Vector3 offset;
	List<Vector3> points = new List<Vector3>();

	static public float GetDistPointToLine(Vector3 origin, Vector3 direction, Vector3 point){
		Vector3 point2origin = origin - point;
		Vector3 point2closestPointOnLine = point2origin - Vector3.Dot(point2origin,direction) * direction;
		return point2closestPointOnLine.magnitude;
	}

	void genPoints(Vector3 a, Vector3 b, float t, int lv) {
		if (lv > 10) return;
		var e = eval(t);
		var dis = GetDistPointToLine(a, (b-a).normalized, e);
		if (dis < tol) {
			points.Add(b);
			return;
		}

		float step = 1.0f / (1<<lv);
		genPoints(a, e, t - step, lv + 1);
		genPoints(e, b, t + step, lv + 1);
	}

	void OnDrawGizmos() {
		var size = Vector3.one * 0.1f;
		Gizmos.DrawCube(p0, size);
		Gizmos.DrawCube(p1, size);
		Gizmos.DrawCube(p2, size);
		Gizmos.DrawCube(p3, size);

		{
			size = Vector3.one * 0.02f;
			Vector3 last = p0;
			for (int i = 1; i <= div; i++) {
				var p = eval((float)i / div);
				Gizmos.color = Color.white;
				Gizmos.DrawLine(last, p);

				Gizmos.color = Color.red;
				Gizmos.DrawCube(p, size);
				last = p;
			}
		}

		{
			points.Clear();
			genPoints(p0, p3, 0.5f, 2);
			Vector3 last = p0 + offset;
			for (int i = 1; i < points.Count; i++) {
				var p = points[i] + offset;
				Gizmos.color = Color.white;
				Gizmos.DrawLine(last, p);

				Gizmos.color = Color.green;
				Gizmos.DrawCube(p, size);
				last = p;
			}

			pointCount = points.Count;
		}
	}
}
