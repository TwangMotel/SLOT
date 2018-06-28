using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace JSNodeMap {
	[SelectionBase]
	[System.Serializable]
	[ExecuteInEditMode]
	[RequireComponent(typeof(LineRenderer))]
	public class Path : MonoBehaviour {
		public Node fromNode;
		public Node toNode;

		public bool drawInEdtior = true;
		public bool drawMarkers = true;
		public bool drawLine = false;

		public MovementType pathDirection;

		// Factors are used to multiply the cost value of a path. The higher the cost, the less desirable the path
		public float toFactor = 1f;
		public float fromFactor = 1f;

		// Speed factor is multiplied to agent movement speed. Higher numbers make agents move faster, lower numbers, slower.
		public float toSpeed = 1f;
		public float fromSpeed = 1f;

		public bool passableByDefault = true;
		public List<string> passExemptions;

		public List<MovementOverride> movementOverrides;

		public int markerType = 0;
		public float markerSpacing;

		public List<float> markerPosList;

		[HideInInspector]
		public Map nodeMap;

		public LineRenderer line;

		public float Distance {
			get {
				return Vector3.Distance(fromNode.transform.position, toNode.transform.position);
			}
		}

		// Built-in

		void Awake() {
			if (passExemptions == null) {
				passExemptions = new List<string>();
			}
			if (markerPosList == null) {
				markerPosList = new List<float>();
			}
			if (movementOverrides == null) {
				movementOverrides = new List<MovementOverride>();
			}
		}

		// Private Methods

		private void ClearPathMarkers() {
			Marker[] markers = transform.GetComponentsInChildren<Marker>();
			foreach (Marker marker in markers) {
				Object.DestroyImmediate(marker.gameObject);
			}
			markerPosList.Clear();
		}

		private void CalculatePathPoints() {
			ClearPathMarkers();
			
			float pathDist = Vector3.Distance(fromNode.transform.position, toNode.transform.position);
			int numMarkers = Mathf.FloorToInt(pathDist / markerSpacing);
			float actualSpacing = pathDist / numMarkers;
			List<Vector3> linePositions = new List<Vector3>();
			linePositions.Add(fromNode.transform.position);

			// Start drawing markers at 1, since 0 would be the point of the original node
			for (int i = 1; i < numMarkers; i++) {
				float markerLoc = i * actualSpacing / pathDist;
				Vector3 pos = Map.GetPointOnPath(this, markerLoc);
				markerPosList.Add(markerLoc);
				linePositions.Add(pos);

				if (drawMarkers) {
					// Instantiate markers
					GameObject markerObj = new GameObject("Marker");
					markerObj.transform.SetParent(transform);
					markerObj.transform.position = pos;

					markerObj.name = "PathMarker";
					markerObj.isStatic = true;
					// markerObj.hideFlags = HideFlags.HideInInspector;

					Marker newPathMarker = markerObj.AddComponent<Marker>();
					newPathMarker.Initialize(this);
				}
			}

			// Update line values
			linePositions.Add(toNode.transform.position);

			if (line) {
				if (drawLine) {
					line.positionCount = linePositions.Count();
					line.SetPositions(linePositions.ToArray());
				} else {
					line.positionCount = 0;
				}
				
			}
			
		}

		// Public Methods

		public void Initialize(Map nodeMap, Node fromNode, Node toNode) {
			this.nodeMap = nodeMap;
			this.markerSpacing = nodeMap.defaultMarkerSpacing;
			this.fromNode = fromNode;
			this.toNode = toNode;
			this.drawMarkers = nodeMap.drawMarkers;
			this.drawLine = nodeMap.drawLines;
			this.pathDirection = nodeMap.defaultPathDirection;
			this.line = gameObject.GetComponent<LineRenderer>();

			SetLineDefaults();
		}

		public bool IsValid() {
			if (fromNode == null || toNode == null) {
				return false;
			}

			return true;
		}

		public void RedrawMarkers() {
			CalculatePathPoints();
		}

		public void Reverse() {
			Node newTo = fromNode;

			fromNode = toNode;
			toNode = newTo;

			transform.SetParent(fromNode.gameObject.transform);
			transform.localPosition = Vector3.zero;
			RedrawMarkers();

			nodeMap.Refresh();
		}

		public void SetVisibleMarkers(bool status) {
			CalculatePathPoints();

			drawMarkers = status;
		}

		public void SetVisibleLine(bool status) {
			drawLine = status;
			if (! drawLine) {
				line.positionCount = 0;
			} else {
				CalculatePathPoints();
			}
		}

		public void SetLineDefaults() {
			line.alignment = this.nodeMap.lineOrientation;
			line.startWidth = 0.5f;
			line.endWidth = 0.5f;
			line.widthMultiplier = this.nodeMap.lineWidth;
			line.sharedMaterial = this.nodeMap.nodeData.lineMaterial;
			line.positionCount = 0;
			line.sortingLayerName = "NodeMap";
			line.sortingOrder = 0;
		}

		public Node GetOtherEnd(Node node) {
			if (fromNode == node) {
				return toNode;
			}
			return fromNode;
		}

		public void AddOverride(int agentType, MovementType movementType) {
			MovementOverride existingOverrideForAgent = movementOverrides.First(o => o.agentType == agentType);
			if (existingOverrideForAgent == null) {
				MovementOverride newOverride = new MovementOverride() {
					agentType = agentType,
					movement = movementType
				};
				movementOverrides.Add(newOverride);
			} else {
				existingOverrideForAgent.movement = movementType;
			}
		}

		public void RemoveOverride(int agentType) {
			MovementOverride existingOverrideForAgent = movementOverrides.First(o => o.agentType == agentType);
			if (existingOverrideForAgent != null) {
				movementOverrides.Remove(existingOverrideForAgent);
			}
		}
	}

	public enum MovementType {
		Impassable,
		OneWay,
		TwoWay,
		Reverse
	}

	[System.Serializable]
	public class MovementOverride {
		public int agentType = 0;
		public MovementType movement = MovementType.TwoWay;
	}
}
