using UnityEngine;
using JSNodeMap;
using System.Collections.Generic;

[RequireComponent(typeof(Agent))]
public class ClickMove : MonoBehaviour {
	private Agent agent;
	public bool active;
	public float maxDistance = 1000f;

    

	void Awake () {
        agent = GetComponent<Agent>();
       
	}
	
	void Update () {
		if (Input.GetMouseButtonDown(0) && ! agent.isMoving && active) {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit, maxDistance)) {
				Node hitNode = hit.transform.GetComponent<Node>();
                List<Node> neighbors = Pathfinding.GetNeighbors(hitNode);
				if (hitNode != null && neighbors.Contains(agent.currentNode)) {
					agent.MoveToTarget(hitNode);
				}
			}
		}
	}

    
    
}
