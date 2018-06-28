using System.Collections.Generic;
using UnityEngine;
using JSNodeMap;

public class Patrol : MonoBehaviour {
	private Agent agent;
	public bool active;

	public List<Node> waypoints = new List<Node>();
	public int currentPointIndex;

	void Awake() {
		agent = GetComponent<Agent>();

		agent.OnCannotReach += NextWaypoint;
		agent.OnMoveEnd += NextWaypoint;
	}

	void Start() {
		NextWaypoint(waypoints[currentPointIndex]);
	}

	private void NextWaypoint(Node targetNode) {
		if (active) {
			if (currentPointIndex >= waypoints.Count) {
				currentPointIndex = 0;
			}

			agent.MoveToTarget(waypoints[currentPointIndex]);

			currentPointIndex++;
		}
	}
}
