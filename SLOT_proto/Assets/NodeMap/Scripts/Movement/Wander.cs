using UnityEngine;
using JSNodeMap;

[RequireComponent(typeof(Agent))]
public class Wander : MonoBehaviour {
	private Agent agent;

	public bool active;

	public Node randomNode;

	void Awake() {
		agent = GetComponent<Agent>();

		agent.OnCannotReach += FindNewNode;
		agent.OnMoveEnd += FindNewNode;
	}

	void Start() {
		FindNewNode(null);
	}

	private Node GetRandomNode() {
		return agent.nodeMap.mapNodes[Random.Range(0, agent.nodeMap.mapNodes.Count)];
	}

	private void FindNewNode(Node targetNode) {
		if (active) {
			randomNode = GetRandomNode();
			agent.MoveToTarget(randomNode);
		}
	}
}
