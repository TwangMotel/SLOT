using UnityEngine;
using UnityEditor;
using JSNodeMap;

[ExecuteInEditMode]
[CustomEditor(typeof(Map))]
public class MapEditor : Editor {
	private Map map;
	private readonly string[] mapModes = new string[] { "2D", "3D" };
	private int mapModeIndex;

	private SerializedProperty agentTypesProperty;


	private bool showSettingsSection = true;
	private bool showDefaultSection = true;
	private string jsonFileName = "SavedMap";
	private TextAsset jsonImport;
	private string playPauseText;

	[MenuItem("GameObject/Node Map/Create New Map")]
	static void CreateNodeMap() {
		GameObject newMap = new GameObject("NodeMap");
		newMap.transform.rotation = Quaternion.identity;
		newMap.AddComponent<Map>();
		newMap.isStatic = true;

		GameObject nodeContainer = new GameObject("Nodes");
		nodeContainer.transform.SetParent(newMap.transform);

		GameObject agentContainer = new GameObject("Agents");
		agentContainer.transform.SetParent(newMap.transform);

		Selection.SetActiveObjectWithContext(newMap, Selection.activeContext);
	}

	private void OnEnable() {
		map = target as Map;
		agentTypesProperty = serializedObject.FindProperty("agentTypes");
		playPauseText = (map.allPaused) ? "Play All" : "Pause All";
	}

	public override void OnInspectorGUI() {
		serializedObject.Update();

		// Settings
		showSettingsSection = EditorGUILayout.Foldout(showSettingsSection, "Settings");
		if (showSettingsSection) {
			EditorGUI.indentLevel++;

			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Draw Mode");
			DrawMode newValue = (DrawMode)EditorGUILayout.Popup((int)map.drawMode, mapModes);
			if (newValue != map.drawMode) {
				Undo.RecordObject(map, "Changed draw mode");
				map.drawMode = newValue;
				map.RedrawMap(true);
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Node Scale");
			float nodeScale = EditorGUILayout.FloatField(map.nodeScale);
			if (System.Math.Abs(nodeScale - map.nodeScale) > Mathf.Epsilon) {
				map.nodeScale = nodeScale;
				map.ResizeNodes();
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Redraw Threshold");
			map.redrawThreshhold = EditorGUILayout.FloatField(map.redrawThreshhold);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Node Data");
			map.nodeData = EditorGUILayout.ObjectField(map.nodeData, typeof(NodeTypeData), false) as NodeTypeData;
			GUILayout.EndHorizontal();

			if (map.nodeData == null) {
				EditorGUILayout.HelpBox("Please assign a NodeTypeData object", MessageType.Error);
			}

			EditorGUI.indentLevel--;
		}

		// Agent Types
		EditorGUILayout.PropertyField(agentTypesProperty, true);

		if (map.agentTypes.Length == 0) {
			EditorGUILayout.HelpBox("You must have at least one Agent type", MessageType.Error);
		}

		// Defaults
		showDefaultSection = EditorGUILayout.Foldout(showDefaultSection, "Defaults");
		if (showDefaultSection) {
			EditorGUI.indentLevel++;

			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Default Marker Spacing");
			map.defaultMarkerSpacing = EditorGUILayout.FloatField(map.defaultMarkerSpacing);
			GUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Draw Nodes");
			map.drawNodes = EditorGUILayout.Toggle(map.drawNodes);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Draw Path Markers");
			map.drawMarkers = EditorGUILayout.Toggle(map.drawMarkers);
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Draw Path Lines");
			map.drawLines = EditorGUILayout.Toggle(map.drawLines);
			EditorGUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Path Line Width");
			map.lineWidth = EditorGUILayout.FloatField(map.lineWidth);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Path Direction");
			map.defaultPathDirection = (MovementType)EditorGUILayout.EnumPopup(map.defaultPathDirection);
			GUILayout.EndHorizontal();

			EditorGUI.indentLevel--;
		}

		EditorGUI.indentLevel--;

		EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

		GUI.enabled &= (map.nodeData != null && map.agentTypes.Length != 0);

		if (GUILayout.Button("New Node")) {
			if (map.nodeData != null) {
				Node newNode = map.CreateNode();
				Selection.activeGameObject = newNode.gameObject;

				serializedObject.Update();

				Selection.SetActiveObjectWithContext(newNode, Selection.activeContext);
			}
		}

		GUI.enabled &= (map.nodeData != null && map.agentTypes.Length != 0 && map.mapNodes.Count > 0);

		if (GUILayout.Button("New Agent")) {
			Agent newAgent = map.CreateAgent();

			serializedObject.Update();
		}

		GUI.enabled = true;

		EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

		if (GUILayout.Button("Redraw Map")) {
			map.RedrawMap(false);
		}

		if (GUILayout.Button("Refresh")) {
			map.Refresh();
		}

		if (GUILayout.Button(playPauseText)) {
			if (map.allPaused) {
				map.PlayAll();
			} else {
				map.PauseAll();
			}
			map.allPaused = ! map.allPaused;
			playPauseText = (map.allPaused) ? "Play All" : "Pause All";
		}

		serializedObject.ApplyModifiedProperties();
	}
}
