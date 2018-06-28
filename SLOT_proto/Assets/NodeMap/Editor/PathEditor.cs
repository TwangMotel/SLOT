using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using JSNodeMap;
using System.Linq;

[ExecuteInEditMode]
[CustomEditor(typeof(Path))]
public class PathEditor : Editor {
	private Path targetPath;

	int _pathTypeIndex;

	string[] agentTypeNames;
	string[] markerTypeNames;
	private SerializedProperty movementOverridesProperty;

	private List<bool> showOverride;

	private bool showMarkersSection = true;
	private bool showLineSection = true;

	private void OnEnable() {
		targetPath = target as Path;
		movementOverridesProperty = serializedObject.FindProperty("movementOverrides");
		agentTypeNames = (targetPath.fromNode.nodeMap != null) ? targetPath.fromNode.nodeMap.agentTypes : new string[0];
		markerTypeNames = (targetPath.fromNode.nodeMap != null) ? targetPath.fromNode.nodeMap.nodeData.MarkerTypeNames : new string[0];

		if (showOverride == null) {
			showOverride = new List<bool>();	
		}

		targetPath.line.hideFlags = HideFlags.HideInInspector;
	}

	public override void OnInspectorGUI() {
		serializedObject.Update();

		// Header
		GUILayout.BeginHorizontal();
		GUILayout.Label("From: " + targetPath.fromNode.nodeName);
		GUILayout.Label("To: " + targetPath.toNode.nodeName);
		GUILayout.EndHorizontal();

		// Cost
		GUILayout.BeginHorizontal();
		GUILayout.Label("From Cost");
		targetPath.fromFactor = EditorGUILayout.FloatField(targetPath.fromFactor);
		GUILayout.Label("To Cost");
		targetPath.toFactor = EditorGUILayout.FloatField(targetPath.toFactor);
		GUILayout.EndHorizontal();

		// Speed
		GUILayout.BeginHorizontal();
		GUILayout.Label("From Speed");
		targetPath.fromSpeed = EditorGUILayout.FloatField(targetPath.fromSpeed);
		GUILayout.Label("To Speed");
		targetPath.toSpeed = EditorGUILayout.FloatField(targetPath.toSpeed);
		GUILayout.EndHorizontal();

		// Select button
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Select")) {
			Selection.SetActiveObjectWithContext(targetPath.fromNode, Selection.activeContext);
		}
		if (GUILayout.Button("Select")) {
			Selection.SetActiveObjectWithContext(targetPath.toNode, Selection.activeContext);
		}
		GUILayout.EndHorizontal();

		EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

		EditorGUILayout.Space();

		GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Movement");
		targetPath.pathDirection = (MovementType)EditorGUILayout.EnumPopup(targetPath.pathDirection);
		GUILayout.EndHorizontal();

		_pathTypeIndex = targetPath.markerType;
		if (_pathTypeIndex < 0) _pathTypeIndex = 0;

		showMarkersSection = EditorGUILayout.Foldout(showMarkersSection, "Markers");
		if (showMarkersSection) {
			EditorGUI.indentLevel++;

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Draw Markers");
			bool drawMarkers = EditorGUILayout.Toggle(targetPath.drawMarkers);
			targetPath.SetVisibleMarkers(drawMarkers);
			EditorGUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Marker Style");
			_pathTypeIndex = EditorGUILayout.Popup(_pathTypeIndex, markerTypeNames);
			if (_pathTypeIndex != targetPath.markerType) {
				targetPath.markerType = _pathTypeIndex;
				targetPath.RedrawMarkers();
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Marker Spacing");
			targetPath.markerSpacing = EditorGUILayout.FloatField(targetPath.markerSpacing);
			GUILayout.EndHorizontal();

			EditorGUI.indentLevel--;
		}

		showLineSection = EditorGUILayout.Foldout(showLineSection, "Line");
		if (showLineSection) {
			EditorGUI.indentLevel++;

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Draw Line");
			bool drawLine = EditorGUILayout.Toggle(targetPath.drawLine);
			targetPath.SetVisibleLine(drawLine);
			EditorGUILayout.EndHorizontal();

			EditorGUI.indentLevel--;
		}

		EditorGUILayout.Space();

		// Override lists
		EditorGUILayout.PropertyField(movementOverridesProperty);
		if (movementOverridesProperty.isExpanded) {
			EditorGUI.indentLevel += 1;

			for (int i = 0; i < movementOverridesProperty.arraySize; i++) {
				SerializedProperty overrideElem = movementOverridesProperty.GetArrayElementAtIndex(i);
				SerializedProperty elemAgentType = overrideElem.FindPropertyRelative("agentType");
				SerializedProperty elemMoveType = overrideElem.FindPropertyRelative("movement");

				GUILayout.BeginHorizontal();

				elemAgentType.intValue = EditorGUILayout.Popup(elemAgentType.intValue, agentTypeNames);
				elemMoveType.enumValueIndex = System.Convert.ToInt32(EditorGUILayout.EnumPopup((MovementType)elemMoveType.enumValueIndex));

				if (GUILayout.Button("X")) {
					movementOverridesProperty.DeleteArrayElementAtIndex(i);
				}
				GUILayout.EndHorizontal();
			}

			EditorGUI.indentLevel -= 1;
		}
		

		if (targetPath.movementOverrides.Select(o => o.agentType).Distinct().Count() < targetPath.movementOverrides.Count) {
			EditorGUILayout.HelpBox("You should only have one override per Agent type", MessageType.Error);
		}

		EditorGUILayout.Space();

		if (GUILayout.Button("Add Override")) {
			targetPath.movementOverrides.Add(new MovementOverride());
			serializedObject.ApplyModifiedProperties();
		}

		EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

		GUILayout.Label("Actions");

		if (GUILayout.Button("Reverse")) {
			targetPath.Reverse();
		}

		if (GUILayout.Button("Split")) {
			SplitPath(targetPath.fromNode, targetPath.toNode);
		}

		if (GUILayout.Button("Delete")) {
			Map.RemovePath(targetPath);
		}

		if (target) {
			serializedObject.ApplyModifiedProperties();
		}
	}

	void SplitPath(Node nodeA, Node nodeB) {
		Path path = Map.FindValidPath(nodeA, nodeB);
		if (path == null) return;

		Node newNode = (target as Path).nodeMap.CreateNode();
		Undo.RegisterCreatedObjectUndo(newNode.gameObject, "Path Created");
		Node midNode = newNode.GetComponent<Node>();

		newNode.transform.position = Map.GetPointOnPath(path, 0.5f);

		Map.RemovePath(path);

		nodeA.CreatePath(midNode, nodeA.nodeMap.drawMarkers);
		midNode.CreatePath(nodeB, nodeA.nodeMap.drawMarkers);

		RedrawMarkers(midNode);
	}

	void RedrawMarkers(Node node) {
		// Redraw this node's outgoing paths
		foreach (Path path in node.paths) {
			path.RedrawMarkers();
		}

		// Redraw paths pointing to this node
		Path[] redrawPaths = node.nodeMap.GetPathsToNode(node);
		foreach (Path path in redrawPaths) {
			path.RedrawMarkers();
		}
	}
}
