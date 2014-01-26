using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AStarNode : MonoBehaviour
{
	public Vector2 GridPosition;
	public bool IsWalkable = true;
	public float GCostModifier = 1f;
	public List<AStarNode> AdjacentNodes;
	public LayerMask layersToLinecast;

	private void Start()
	{
		AdjacentNodes = new List<AStarNode>(8);
		GetAdjacentNodes();
		//If you're not less than 3 units above something, you're not traversable
		IsWalkable = Physics.Raycast(transform.position, -Vector3.up, 3f);

	}

	private void GetAdjacentNodes()
	{
		//If you're traversable
		if (IsWalkable)
		{
			var collidersInRadius = Physics.OverlapSphere(transform.position, 1.5f);
			foreach (var gObject in collidersInRadius)
			{
				//Get all path nodes within 1.5 units from you
				if (gObject.tag == "PathNode" && !Physics.Linecast(transform.position, gObject.transform.position, layersToLinecast.value))
				{
					//As long as they're traversable as well
					if (gObject.GetComponent<AStarNode>().IsWalkable)
					{
						AdjacentNodes.Add(gObject.GetComponent<AStarNode>());
					}
				}
			}
			if (AdjacentNodes.Count == 0)
			{
				IsWalkable = false;
			}
		}
	}

	void OnTriggerStay(Collider other)
	{
		//If there's an Obstacle collider on top of you, you're not traversable
		IsWalkable = other.tag != "Obstacle";
	}
}