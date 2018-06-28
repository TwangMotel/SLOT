using UnityEngine;
using JSNodeMap;

[RequireComponent(typeof(Agent))]
public class DirectMove : MonoBehaviour {
	private Agent agent;
	public bool active;

	[Tooltip("Decrease this value to increase required precision with movement")]
	public float detectionAngle = 45f;
	public float deadZone = 0.1f;

	public float inputDelay = 0.02f;

	private float currentInputTime;
	private float inputWait = 0.01f;
	private float inputCylce;

	public MovementReference moveRef = MovementReference.Camera;

	void Awake() {
		agent = GetComponent<Agent>();
	}

	void Update() {
		if (! active || agent.isMoving) {
			return;
		}

		if (inputCylce > 0f) {
			inputCylce -= Time.deltaTime;
			return;
		}

		inputCylce = inputWait;

		Vector2 move = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

		if (Mathf.Abs(move.x) < deadZone && Mathf.Abs(move.y) < deadZone) { 
			currentInputTime = 0f; 
			return; 
		}

		currentInputTime += Time.deltaTime;
		if (currentInputTime > inputDelay) {
			currentInputTime = 0f;
		} else {
			return;
		}

		float moveAngle = GetAngleV2(Vector2.zero, move);
		Node targetNode = null;
		float closestAngleDiff = 500f;

		float closestAngle = 0f;
		foreach (Node node in Pathfinding.GetNeighbors(agent.currentNode, agent)) {
			Vector3 fromPos, toPos;

			fromPos = agent.currentNode.transform.position;
			toPos = node.transform.position;

			float pathAngle = GetAngleV3(fromPos, toPos);
			float pathAngleDiff = GetAngleDiff(moveAngle, pathAngle);
			if (pathAngleDiff >= detectionAngle) {
				continue;
			}

			if (pathAngleDiff < closestAngleDiff) {
				closestAngleDiff = pathAngleDiff;
				targetNode = node;
				closestAngle = pathAngle;
			}

		}

		if (closestAngleDiff > detectionAngle) {
			currentInputTime = 1f / 20f;
		} else {
			if (targetNode != null) {
				agent.MoveToTarget(targetNode);
			}
		}
	}

	float GetAngleV2(Vector2 point1, Vector2 point2) {
		point2 -= point1;
		point1 = Vector2.zero;
		return Mathf.DeltaAngle(Mathf.Atan2(point1.x, point1.y) * Mathf.Rad2Deg, Mathf.Atan2(point2.x, point2.y) * Mathf.Rad2Deg);
	}

	float GetAngleV3(Vector3 point1, Vector3 point2) {
		point2 -= point1;
		point1 = Vector3.zero;

		if (moveRef == MovementReference.Camera) {
			point2 = Camera.main.WorldToViewportPoint(point2);
			point1 = Camera.main.WorldToViewportPoint(point1);

			return GetAngleV2(new Vector2(point1.x, point1.y), new Vector2(point2.x, point2.y));
		}

		return Mathf.DeltaAngle(Mathf.Atan2(point1.x, point1.z) * Mathf.Rad2Deg, Mathf.Atan2(point2.x, point2.z) * Mathf.Rad2Deg);
	}

	float GetAngleDiff(float angle1, float angle2) {
		float angle = 180f - Mathf.Abs(Mathf.Abs(angle1 - angle2) - 180f);
		return (angle + 360f) % 360f;
	}


	public enum MovementReference {
		Global,
		Camera
	}
}
