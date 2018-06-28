using UnityEngine;
using UnityEditor;
using JSNodeMap;

[ExecuteInEditMode]
[CustomEditor(typeof(Marker))]
public class MarkerEditor : Editor {
	void OnSceneGUI() {
		Selection.activeGameObject = Selection.activeGameObject.transform.parent.gameObject;
	}
}
