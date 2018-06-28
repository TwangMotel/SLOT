using UnityEngine;
using UnityEditor;
using JSNodeMap;

[ExecuteInEditMode]
[CustomEditor(typeof(Agent))]
[CanEditMultipleObjects]
public class AgentEditor : Editor {
	private Agent agent;

	int _currentNodeIndex;
	int _agentTypeIndex;

	string[] agentTypeNames;
	string playPauseText;

	private void OnEnable() {
		agent = target as Agent;
		agentTypeNames = (agent.nodeMap != null) ? agent.nodeMap.agentTypes : new string[0];
		playPauseText = (agent.doMove) ? "Pause" : "Play";
	}

	public override void OnInspectorGUI() {
		serializedObject.Update();

		GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Type");
		_agentTypeIndex = EditorGUILayout.Popup(agent.agentType, agentTypeNames);
		agent.agentType = _agentTypeIndex;
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Move Speed");
		agent.movementSpeed = EditorGUILayout.FloatField(agent.movementSpeed);
		GUILayout.EndHorizontal();

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

		serializedObject.ApplyModifiedProperties();

		// Wasn't positive this would work consistently. Uncomment and use if you like!
		//if (GUILayout.Button("Face Camera")) {
		//	Vector3 lookTarget = new Vector3(Camera.main.transform.position.x, agent.transform.position.y, Camera.main.transform.position.z);
		//	agent.FaceDir(lookTarget);
		//}

		if (GUILayout.Button("Face Target")) {
			if (agent.targetNode != null) {
				Vector3 lookTarget = new Vector3(agent.targetNode.transform.position.x, agent.transform.position.y, agent.targetNode.transform.position.z);
				agent.FaceDir(lookTarget);
			}
		}

		if (GUILayout.Button("Retreat")) {
			agent.DoRetreat();
		}

		if (GUILayout.Button(playPauseText)) {
			if (agent.doMove) {
				agent.Pause();
			} else {
				agent.Play();
			}
			playPauseText = (agent.doMove) ? "Pause" : "Play";
		}
	}
}
