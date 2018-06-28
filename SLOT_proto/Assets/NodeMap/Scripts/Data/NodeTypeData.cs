using System.Linq;
using UnityEngine;

namespace JSNodeMap {
	[CreateAssetMenu(menuName = "Node Map/Node Type Data")]
	public class NodeTypeData : ScriptableObject {
		public NodeType[] nodeTypes;
		public NodeType[] markerTypes;

		public Material meshMaterial;
		public Material lineMaterial;

		public string[] NodeTypeNames {
			get {
				return nodeTypes.Select(t => t.name).ToArray();
			}
		}

		public string[] MarkerTypeNames {
			get {
				return markerTypes.Select(t => t.name).ToArray();
			}
		}
	}

	[System.Serializable]
	public class NodeType {
		public string name;
		public Sprite sprite;
		public Mesh mesh;
	}
}
