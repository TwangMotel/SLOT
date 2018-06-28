using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace JSNodeMap {
	public static class Pathfinding {
		// Private methods

		private static float HCost(Node curNode, Node goalNode) {
			return Vector3.Distance(curNode.transform.position, goalNode.transform.position);
		}

		private static float GCost(Node curNode, Node neighbor) {
			Path path = Map.FindValidPath(curNode, neighbor);
			float factor = (path.fromNode == curNode) ? path.fromFactor : path.toFactor;

			return path.Distance * factor;
		}

		// Public Methods

		public static List<Node> GetNeighbors(Node node, Agent agent = null) {
			List<Node> neighbors = new List<Node>();

			// Check all paths FROM this node
			foreach (Path path in node.paths) {
				if (AgentCanMoveAcross(path, node, agent)) {
					neighbors.Add(path.toNode);
				}
			}
		
			// Now do the same for paths TO this node
			foreach(Path path in node.nodeMap.mapNodes.SelectMany(n => n.paths).Where(p => p.toNode == node)) {
				if (AgentCanMoveAcross(path, node, agent)) {
					neighbors.Add(path.fromNode);
				}
			}

			return neighbors;
		}

		public static bool AgentCanMoveAcross(Path path, Node node, Agent agent) {
			MovementOverride moveOR = (agent == null) ? null : path.movementOverrides.FirstOrDefault(o => o.agentType == agent.agentType);
			MovementType checkMove;

			if (moveOR == null) {
				// No overrides. Use normal path direction
				checkMove = path.pathDirection;
			} else {
				// Use override value for this agent
				checkMove = moveOR.movement;
			}

			// Everyone can move on a two-way
			if (checkMove == MovementType.TwoWay) {
				return true;
			}
			// One-way allows moving from FROM to TO
			if (checkMove == MovementType.OneWay && path.fromNode == node) {
				return true;
			}
			// Reverse allows moving from TO to FROM
			if (checkMove == MovementType.Reverse && path.toNode == node) {
				return true;
			}
			// If we're here, it means it's impassable
			return false;
		}

		public static List<Node> FindRoute(Node startNode, Node goalNode, Agent agent = null) {
			PriorityQueue<float, Node> openNodes = new PriorityQueue<float, Node>();
			HashSet<Node> checkedNodes = new HashSet<Node>();

			Dictionary<Node, Node> pathTo = new Dictionary<Node, Node>();
			Dictionary<Node, float> gCost = new Dictionary<Node, float>();

			pathTo[startNode] = null;
			gCost[startNode] = 0f;

			openNodes.Push(0f + HCost(startNode, goalNode), startNode);

			while (openNodes.Count > 0) {
				Node leafNode = openNodes.Top.Value;
				if (leafNode == goalNode) {
					// Success!
					List<Node> route = new List<Node>();
					Node pointer = goalNode;

					while (pointer != null) {
						route.Add(pointer);
						pointer = pathTo[pointer];
					}

					route.Reverse();	// Invert route so we can follow it from start to finish

					return route;
				}
				openNodes.Pop();

				checkedNodes.Add(leafNode);

				List<Node> neighbors = GetNeighbors(leafNode, agent);
				for (int i = 0; i < neighbors.Count; i++) {
					Node neighbor = neighbors[i];
					if (! checkedNodes.Contains(neighbor) && ! openNodes.Contains(neighbor)) {
						gCost[neighbor] = gCost[leafNode] + GCost(leafNode, neighbor);
						pathTo[neighbor] = leafNode;
						openNodes.Push(gCost[neighbor] + HCost(neighbor, goalNode), neighbor);
					}
				}
			}

			// No route found
			return null;
		}
	}
}
