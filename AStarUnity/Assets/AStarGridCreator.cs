using UnityEngine;
using System.Collections.Generic;



public class AStarGridCreator : MonoBehaviour
{
	public int gridWidth = 50;
	public int gridLength = 50;
	public AStarNode[,] AStarNodes;

	void Start()
	{
		AStarNodes = new AStarNode[gridWidth, gridLength];
		var tempNodeArray = (AStarNode[])FindObjectsOfType(typeof(AStarNode));
		//Debug.Log("found all pathnodes " + tempNodeArray);
		foreach (var node in tempNodeArray)
		{
			AStarNodes[(int)node.GridPosition.y, (int)node.GridPosition.x] = node;
			//Debug.Log("Created Node at " + node.GridPosition);
		}

	}

	[ContextMenu("Create Grid")]
	void CreateGrid()
	{
		//Maybe change it so it's dynamic. It starts at zero and goes out in concentric circles, raycasting down, or maybe you set a range of units for the scene (like test a 500x500 area or something), if it hits something within the set range, place a node
		AStarNodes = new AStarNode[gridLength, gridWidth];
		for (var y = 0; y < gridLength; y++)
		{
			for (var x = 0; x < gridWidth; x++)
			{
				var temp =
					(GameObject)
					Instantiate(Resources.Load("AStarNode"),
								new Vector3(transform.position.x + (x * 2), 1.5f, transform.position.z + (y * 2)), Quaternion.identity);

				AStarNodes[y, x] = temp.GetComponent<AStarNode>();
				AStarNodes[y, x].tag = "PathNode";
			}
		}
		for (var y = 0; y < gridLength; y++)
		{
			for (var x = 0; x < gridWidth; x++)
			{
				AStarNodes[y, x].GridPosition.Set(x, y);
				AStarNodes[y, x].name = "AStarNode @ " + x + ", " + y;
			}
		}
	}
}
