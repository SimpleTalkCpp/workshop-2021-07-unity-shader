using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteInEditMode]
public class Week003_WorldScanner : MyPostProcessSimple
{
	public void Update() {
		if (material) {
			material.SetVector("_ScannerCenter", transform.position);
		}
	}
}
