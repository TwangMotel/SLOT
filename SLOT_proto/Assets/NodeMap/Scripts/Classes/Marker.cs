using UnityEngine;

namespace JSNodeMap {
	[System.Serializable]
	[ExecuteInEditMode]
	public class Marker : MonoBehaviour {
		public SpriteRenderer spriteRenderer;
		public MeshRenderer meshRenderer;
		public MeshFilter meshFilter;

		public int markerType;

		public Path nodePath;

		public void Initialize(Path path) {
			nodePath = path;
			markerType = path.markerType;
			SetRenderer(path);

		}

		void SetRenderer(Path path) {
			if (path.nodeMap.drawMode == DrawMode.Sprite) {
				// 2D
				
				// Clear out 3D renderers
				DestroyImmediate(meshFilter);
				DestroyImmediate(meshRenderer);
				meshFilter = null;
				meshRenderer = null;

				// Set 2D renderer
				if (spriteRenderer == null) {
					spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
				}
				spriteRenderer.sprite = path.nodeMap.nodeData.markerTypes[markerType].sprite;

				spriteRenderer.sortingLayerName = "NodeMap";
				spriteRenderer.sortingOrder = 1;

				Vector3 moveDirection = path.toNode.transform.position - path.fromNode.transform.position;
				float angle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
				// transform.localRotation = Quaternion.Euler(0f, 0f, angle);
				transform.Rotate(90f, 0f, - angle);
			} else {
				// 3D

				// Clear out 2D renderers
				DestroyImmediate(spriteRenderer);
				spriteRenderer = null;

				// Set 3D renderer
				if (meshFilter == null) {
					meshFilter = gameObject.AddComponent<MeshFilter>();
				}
				meshFilter.mesh = path.nodeMap.nodeData.markerTypes[markerType].mesh;

				if (meshRenderer == null) {
					meshRenderer = gameObject.AddComponent<MeshRenderer>();
				}
				meshRenderer.sharedMaterial = path.nodeMap.nodeData.meshMaterial;
				
				// Set orientation
				transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
				transform.LookAt(path.toNode.transform);
			}
		}
	}
}