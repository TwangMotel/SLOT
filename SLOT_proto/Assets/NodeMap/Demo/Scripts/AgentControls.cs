using UnityEngine;
using UnityEngine.UI;
using JSNodeMap;

public class AgentControls : MonoBehaviour {
	public Agent agent;
	public AgentType agentType;
	public Text txtStatus;
	public Text txtLatest;

	void Awake() {
		agent.OnMoveStart += MoveStart;
		agent.OnNodeArrive += NodeArrive;
		agent.OnMoveEnd += MoveEnd;
		agent.OnRetreat += Retreat;
		agent.OnCannotReach += CannotReach;
		agent.OnAgentCollide += AgentCollide;
		agent.OnMarkerTick += MarkerTick;
		agent.OnPause += OnPause;
		agent.OnResume += OnResume;
	}

	public void ToggleActive(bool status) {
		switch (agentType) {
		case AgentType.Click:
			agent.GetComponent<ClickMove>().active = status;
			break;
		case AgentType.Direct:
			agent.GetComponent<DirectMove>().active = status;
			break;
		case AgentType.Patrol:
			agent.GetComponent<Patrol>().active = status;
			break;
		case AgentType.Wander:
			agent.GetComponent<Wander>().active = status;
			break;
		}
	}

	void MoveStart(Node targetNode) {
		UpdateStatus("Moving to " + targetNode.nodeName);
	}

	void NodeArrive(Node arrivedNode, bool isTargetNode) {
		UpdateLatest("Arrived at " + ((isTargetNode) ? " target: " : "") + arrivedNode.nodeName);
	}

	void MoveEnd(Node targetNode) {
		UpdateStatus("Reached " + targetNode.nodeName);
	}

	void Retreat(Node targetNode) {
		UpdateStatus("Retreating to " + targetNode.nodeName);
	}

	void CannotReach(Node targetNode) {
		UpdateStatus("Cannot reach " + targetNode.nodeName);
	}

	void AgentCollide(Agent otherAgent) {
		UpdateLatest("Contact with " + otherAgent.name);
	}

	void MarkerTick(int markerIndex) {
		UpdateLatest("Passed marker " + markerIndex);
	}

	void UpdateStatus(string status) {
		txtStatus.text = status;
	}

	void UpdateLatest(string status) {
		txtLatest.text = status;
	}

	void OnPause() {
		UpdateLatest("Paused");
	}

	void OnResume() {
		UpdateLatest("Resuming");
	}

	public enum AgentType {
		Click,
		Direct,
		Patrol,
		Wander
	}
}
