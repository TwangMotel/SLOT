using UnityEngine;
using JSNodeMap;
using System.Collections.Generic;
using Yarn.Unity;

[RequireComponent(typeof(Agent))]
public class ClickMove : MonoBehaviour {
	private Agent agent;
	public bool active;
	public float maxDistance = 1000f;
    GameObject dialogue;
    DialogueRunner dialogueRunner;


    void Awake () {
        agent = GetComponent<Agent>();
        dialogue = GameObject.Find("Dialogue");
        dialogueRunner = dialogue.GetComponent<DialogueRunner>();
        
       
	}
	
	void Update () {
		if (Input.GetMouseButtonDown(0) && ! agent.isMoving && active && dialogueRunner.isDialogueRunning == false) {
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit, maxDistance)) {
				Node hitNode = hit.transform.GetComponent<Node>();
                List<Node> neighbors = Pathfinding.GetNeighbors(hitNode,agent);
				if (hitNode != null && neighbors.Contains(agent.currentNode)) {
					agent.MoveToTarget(hitNode);
				}
			}
		}
	}

    
    
}
