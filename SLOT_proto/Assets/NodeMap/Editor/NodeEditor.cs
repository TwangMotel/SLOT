using UnityEngine;
using UnityEditor;
using JSNodeMap;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
[CustomEditor(typeof(Node))]
[CanEditMultipleObjects]
public class NodeEditor : Editor {
	private Node targetNode;

	private Vector3 lastDrawnPosition;

	private int _nodeTypeIndex;

	private string[] nodeTypeNames;

	// Built-in

	private void OnEnable() {
		targetNode = target as Node;
		nodeTypeNames = (targetNode.nodeMap != null) ? targetNode.nodeMap.nodeData.NodeTypeNames : new string[0];
	}

	void OnSceneGUI() {
		if (Event.current.type == EventType.KeyDown) {
			if (Event.current.control) {
				if (Event.current.keyCode == KeyCode.E) {
					Extend();
				}

				if (Event.current.keyCode == KeyCode.J) {
					CreateNodePath();
				}
			}
		} else if (Event.current.type == EventType.MouseDrag) {
			Node node = target as Node;
			if (node.nodeMap.redrawThreshhold != 0f && Vector3.Distance(node.transform.position, lastDrawnPosition) > node.nodeMap.redrawThreshhold) {
				RedrawMarkers(node);
				lastDrawnPosition = node.transform.position;
			}
		}
	}

	public override void OnInspectorGUI() {
		serializedObject.Update();

		ClearInvalidPaths();

		if (serializedObject.isEditingMultipleObjects) {
			ShowPathButton(targets);
			if (targets.Length == 2) {
				if (GUILayout.Button("Split")) {
					if (targets[0].GetType() == typeof(Node) && (targets[1].GetType() == typeof(Node))) {
						SplitPath(targets[0] as Node, targets[1] as Node);
					}
				}
			}
			ClearPaths();
		} else {
			GameObject sel = Selection.activeGameObject;
			if (sel != null) {
				ShowPathSettings(Selection.activeGameObject.GetComponent<Node>());
			}
		}

		serializedObject.ApplyModifiedProperties();

		RedrawMarkers(targetNode);

		EditorGUILayout.Space();

		EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

		if (GUILayout.Button("Extend")) {
			Extend();
		}

		if (GUILayout.Button("Delete")) {
			Map.RemoveNode(targetNode);
		}
	}

	void OnDestroy() {
		Node delNode = target as Node;
		if (Application.isEditor && delNode == null && targetNode != null && targetNode.nodeMap != null) {
			targetNode.nodeMap.ClearPathsToNode(targetNode);
			targetNode.nodeMap.mapNodes.Remove(targetNode);
		}
	}

	// Private Methods

	void ClearPaths() {
		List<Path> removeList = new List<Path>();
		bool doClear = GUILayout.Button("Clear Paths");
		if (doClear) {
			foreach (GameObject obj in Selection.gameObjects) {
				foreach (Path path in obj.GetComponent<Node>().paths) {
					if (Selection.gameObjects.Contains(path.toNode.gameObject)) {
						removeList.Add(path);
					}
				}
			}

			foreach (GameObject obj in Selection.gameObjects) {
				obj.GetComponent<Node>().paths.RemoveAll(x => removeList.Contains(x));
			}
		}
	}

	void CreateNodePath() {
		Object[] selObjs = Selection.GetFiltered(typeof(Node), SelectionMode.TopLevel);
		List<Node> nodes = new List<Node>();
		foreach (Object obj in selObjs) {
			if (obj.GetType() == typeof(Node)) nodes.Add(obj as Node);
		}

		nodes.RemoveAll(i => i == null);

		Event.current.Use();
		SceneView.RepaintAll();

		for (int i = 0; i < nodes.Count - 1; i++) {
			if (Map.FindValidPath(nodes[i], nodes[i + 1]) != null) {
				// Path already exists
				return;
			}
			nodes[i].CreatePath(nodes[i + 1], nodes[i].nodeMap.drawMarkers);
			RedrawMarkers(nodes[i]);
		}
	}

	void Extend() {
		Map map = null;
		GameObject[] selectedObjs = new GameObject[Selection.gameObjects.Length];

		int pos = -1;
		Event.current.Use();

		foreach (GameObject obj in Selection.gameObjects) {
			Node fromNode = obj.GetComponent<Node>();
			if (fromNode == null) {
				continue;
			}
			pos++;

			if (map == null) {
				map = fromNode.nodeMap;
			}

			Node newNode = map.CreateNode();
			selectedObjs[pos] = newNode.gameObject;

			newNode.transform.position = obj.transform.position;
			newNode.transform.SetParent(fromNode.transform.parent);

			Path path = fromNode.CreatePath(newNode, fromNode.nodeMap.drawMarkers);
		}

		if (map != null) {
			SerializedObject serializedObj = new SerializedObject(map);
			serializedObj.Update();
		}

		Selection.objects = selectedObjs;
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

	void ShowPathButton(Object[] curTargets) {
		if (Selection.gameObjects.Length < 2) return;

		if (GUILayout.Button("Create Path")) {
			GUILayout.Space(5);
			CreateNodePath();
		}
	}

	//Shows the information in the inspector for each attachment
	void ShowPathSettings(Node curNode) {
		GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Name");
		string newName = EditorGUILayout.TextField(serializedObject.FindProperty("nodeName").stringValue);
		serializedObject.FindProperty("nodeName").stringValue = newName;
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Type");

		_nodeTypeIndex = EditorGUILayout.Popup(targetNode.nodeType, nodeTypeNames);
		if (_nodeTypeIndex != targetNode.nodeType) {
			targetNode.nodeType = _nodeTypeIndex;
			targetNode.Redraw();
		}
		GUILayout.EndHorizontal();

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Draw Node");
		bool drawNode = EditorGUILayout.Toggle(curNode.drawNode);
		curNode.SetVisible(drawNode);
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();

		GUILayout.Label("Paths");

		if (curNode == null) { return; }

		Path[] paths = curNode.GetAllPaths();

		EditorGUI.indentLevel++;

		foreach (Path path in paths) {
			EditorGUILayout.BeginHorizontal();
			if (path.toNode == curNode) {
				EditorGUILayout.LabelField("From " + path.fromNode.nodeName);
			} else if (path.fromNode == curNode) {
				EditorGUILayout.LabelField("To " + path.toNode.nodeName);
			} else {
				EditorGUILayout.LabelField("Invalid Path");
			}

			bool edit = GUILayout.Button("Select");
			if (edit) {
				Selection.SetActiveObjectWithContext(path, Selection.activeContext);
			}

			bool remove = GUILayout.Button("Delete");
			if (remove) {
				Undo.RecordObject(path.toNode, "Deleted path");
				Undo.RecordObject(path.fromNode, "Deleted path");

				Map.RemovePath(path);
			}
			EditorGUILayout.EndHorizontal();

		}

		EditorGUI.indentLevel--;
	}

	void SplitPath(Node nodeA, Node nodeB) {
		Path path = Map.FindValidPath(nodeA, nodeB);
		if (path == null) return;

		Node newNode = (target as Node).nodeMap.CreateNode();
		Undo.RegisterCreatedObjectUndo(newNode.gameObject, "Path Created");
		Node midNode = newNode.GetComponent<Node>();

		newNode.transform.position = Map.GetPointOnPath(path, 0.5f);

		Map.RemovePath(path);

		nodeA.CreatePath(midNode, nodeA.nodeMap.drawMarkers);
		midNode.CreatePath(nodeB, nodeA.nodeMap.drawMarkers);

		RedrawMarkers(midNode);
	}

	// Public Methods

	public void ClearInvalidPaths() {
		foreach (Node n in FindObjectsOfType<Node>()) {
			List<Path> listToClean = new List<Path>();
			listToClean.AddRange(n.paths);
			foreach (Path a in listToClean) {
				if (a.IsValid() == false) {
					n.paths.Remove(a);
				}
			}
		}
	}
}
