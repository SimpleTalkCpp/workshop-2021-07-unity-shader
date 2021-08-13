using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenu("Week003_WorldScanner")]
public class Week003_WorldScannerVolume : VolumeComponent, IPostProcessComponent {
	override public void Override(VolumeComponent state, float interpFactor) {
		Debug.Log("Override");
		base.Override(state, interpFactor);
	}
	override protected void OnDisable() {
		base.OnDisable();
	}
	override protected void OnEnable() {
		base.OnEnable();
	}

	public bool IsActive() => true;
	public bool IsTileCompatible() => false;
}
