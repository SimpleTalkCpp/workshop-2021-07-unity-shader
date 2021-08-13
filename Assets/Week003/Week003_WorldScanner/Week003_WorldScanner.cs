using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public enum Week003_WorldScanner_UvMode {
	OneDimension = 1,
	TwoDimension = 2,
};


[ExecuteInEditMode]
public class Week003_WorldScanner : MyPostProcessSimple
{
	public void Update() {
		if (material) {
			material.SetVector("_ScannerCenter", transform.position);
		}
	}
}
