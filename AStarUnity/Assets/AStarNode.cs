using System.Collections.Generic;
using UnityEngine;

public class AStarNode : MonoBehaviour
{
	public Vector2 GridPosition;
	public bool IsWalkable = true;
	public float GCostModifier = 1f;
	public List<AStarNode> AdjacentNodes;

	private void Start()
	{
		AdjacentNodes = new List<AStarNode>(8);
		GetAdjacentNodes();
		IsWalkable = Physics.Raycast(transform.position, -transform.up, 3f);

	}

	void Update()
	{

	}

	void OnDrawGizmos()
	{
		//Gizmos.DrawSphere(transform.position, 0.5f);
	}

	private void GetAdjacentNodes()
	{

		if (IsWalkable)
		{
			var collidersInRadius = Physics.OverlapSphere(transform.position, 1.5f);
			foreach (var gObject in collidersInRadius)
			{
				if (gObject.tag == "PathNode" && !Physics.Linecast(transform.position, gObject.transform.position, 0))
				{
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
		IsWalkable = other.tag != "Obstacle";
		//isWalkable = other.tag != "Pather";
	}
}