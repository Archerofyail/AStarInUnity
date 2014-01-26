using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Ray Colors Guide:
//Green: The node has been moved to the open list
//Red: The node was moved to the closed list
//White: The node was added to the list of path nodes
//A point in between nodes to check if there were any obstacles
//

public class CachedNode
{
	public int FCost;
	public int GCost;
	public int HCost;
	public AStarNode NodeCached;
	public AStarNode ParentNode;

	public CachedNode(int gCost, int hCost, AStarNode nodeCached, AStarNode parentNode)
	{
		FCost = gCost + hCost;
		GCost = gCost;
		HCost = hCost;
		NodeCached = nodeCached;
		ParentNode = parentNode;
	}

	public void RecalculateFCost()
	{
		FCost = GCost + HCost;
	}
}

public class Mover : MonoBehaviour
{
	public enum CurrentState
	{
		Idling,
		Wandering,
		LookingForTarget,
		FindingPath,
		Moving,
		TargetReached,
		TargetImpossibleToReach
	}

	public CurrentState movementState = CurrentState.Wandering;

	public Vector2 targetNodePosition;
	public Vector2 initialPosition;
	/// <summary>
	///Used to determine whether debug rays should be drawn. Only change in inspector 
	/// </summary>
	public bool DebugRays;
	/// <summary>
	/// Used to determin whether path lines should be drawn in editor. Only should change in inspector
	/// </summary>
	public bool DebugPathDraw;
	/// <summary>
	/// List of nodes that have had their values calculated and are available to be chosen
	/// </summary>
	private List<CachedNode> openList;
	/// <summary>
	/// List of nodes that have been evaluated based on their cost to move
	/// </summary>
	private List<CachedNode> closedList;
	/// <summary>
	/// List of nodes that have been selected to be followed to the target
	/// </summary>
	private List<CachedNode> pathNodes;
	/// <summary>
	/// The number of steps to check for obstacles in between nodes when smoothing the path
	/// </summary>
	public int stepsBetweenNodes = 5;

	public int secondsForRaysToStayUp;
	public int secondsForPathsToStayUp;

	/// <summary>
	/// The speed of the mover
	/// </summary>
	public float speed = 6f;
	public float rotationSpeed = 20f;
	public float nodeIntersectOffset = 0.1f;
	/// <summary>
	/// The manager of the path nodes grid
	/// </summary>
	public AStarGridCreator aStarGrid;
	/// <summary>
	/// The current index of the path nodes list
	/// </summary>
	private int currentNode;
	/// <summary>
	/// If you can't reach the target in any way, this should turn false
	/// </summary>
	private bool canReachTarget = true;
	/// <summary>
	/// The current path node during path calculations
	/// </summary>
	private CachedNode currentPathCalcNode;
	/// <summary>
	/// The current position the mover actually is at
	/// </summary>
	public Vector2 currentMoverGridPosition { get; private set; }
	/// <summary>
	/// The last position currentPathCalcPosition was at
	/// </summary>
	private Vector2 lastPathCalcPos;
	/// <summary>
	/// The next node to move to when moving
	/// </summary>
	private AStarNode nextNode;
	/// <summary>
	/// The current node you're at when figuring out the path after you've reached the target position
	/// </summary>
	private CachedNode PathFindNode;
	/// <summary>
	/// The list of nodes adjacent to currentPathCalcPosition
	/// </summary>
	//private List<CachedNode> currentAdjacentNodes;

	private bool canSeePlayer;



	void Start()
	{
		aStarGrid = GameObject.Find("AStarNodeManager").GetComponent<AStarGridCreator>();
		openList = new List<CachedNode>(50);
		closedList = new List<CachedNode>(50);
		pathNodes = new List<CachedNode>(50);
		//currentAdjacentNodes = new List<CachedNode>(8);
		currentPathCalcNode = new CachedNode(0, GetHCost(currentMoverGridPosition, targetNodePosition),
														 aStarGrid.AStarNodes[(int)initialPosition.y, (int)initialPosition.x].GetComponent<AStarNode>(),
														aStarGrid.AStarNodes[(int)initialPosition.y, (int)initialPosition.x].GetComponent<AStarNode>());
		print(aStarGrid + " node at 0, 0 is " + aStarGrid.AStarNodes[0, 0]);
		print(aStarGrid);
		currentMoverGridPosition = currentPathCalcNode.NodeCached.GridPosition;
		transform.position = currentPathCalcNode.NodeCached.transform.position;
		nextNode = aStarGrid.AStarNodes[(int)currentMoverGridPosition.y, (int)currentMoverGridPosition.x].GetComponent<AStarNode>();
	}

	void Update()
	{



		switch (movementState)
		{
			case CurrentState.Idling:
			{
				closedList.Clear();
				openList.Clear();
				pathNodes.Clear();

				movementState = CurrentState.LookingForTarget;

				openList.Add(currentPathCalcNode);
				canReachTarget = true;
				//GetAdjacentNodes();
				currentNode = 0;
			}
			break;
			case CurrentState.Wandering:
			{
				closedList.Clear();
				openList.Clear();
				pathNodes.Clear();

				//targetNodePosition = new Vector2(1, 1);
				targetNodePosition = GetRandomWaypoint().GridPosition;
				movementState = CurrentState.LookingForTarget;

				openList.Add(currentPathCalcNode);
				canReachTarget = true;
				//GetAdjacentNodes();
				currentNode = 0;
			}
			break;
			case CurrentState.LookingForTarget:
			{

				if (canReachTarget)
				{
					FindTarget();
					lastPathCalcPos = currentPathCalcNode.NodeCached.GridPosition;
				}
				else
				{
					movementState = CurrentState.TargetImpossibleToReach;
				}

			}
			break;
			case CurrentState.FindingPath:
			{
				FindPath();
			}
			break;
			case CurrentState.Moving:
			{
				MoveToNextNode();
			}
			break;
			case CurrentState.TargetReached:
			{
				pathNodes.Clear();
				openList.Clear();
				closedList.Clear();
				movementState = CurrentState.Wandering;
			}
			break;
			case CurrentState.TargetImpossibleToReach:
			{
				//Find new target if possible, else return to idle state
				targetNodePosition = GetRandomWaypoint().GridPosition;
				currentPathCalcNode = new CachedNode(0, GetHCost(currentMoverGridPosition, targetNodePosition),
													 aStarGrid.AStarNodes[
														 (int)currentMoverGridPosition.y, (int)currentMoverGridPosition.x]
														 .GetComponent<AStarNode>(),
													 aStarGrid.AStarNodes[
														 (int)currentMoverGridPosition.y, (int)currentMoverGridPosition.x]
														 .GetComponent<AStarNode>());
				movementState = CurrentState.Wandering;
			}
			break;
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.tag == "PathNode")
		{
			ChangeCurrentPosition(other.gameObject.GetComponent<AStarNode>().GridPosition);
		}
	}

	/// <summary>
	/// Searches for the target using an A* search algorithm (WIP)
	/// </summary>
	private void FindTarget()
	{
		//Find the cheapest FCost node in the open list
		var tempOpenNode = new CachedNode(10000, 15000, null, null);
		// Look for the lowest F cost square on the open list. We refer to this as the current square.
		foreach (var node in openList)
		{
			//If node FCost is less than temp node FCost
			if (node.FCost < tempOpenNode.FCost)
			{
				tempOpenNode = node;
			}
		}


		{
			currentPathCalcNode = tempOpenNode;

			closedList.Add(currentPathCalcNode);
			// Switch it to the closed list.
			openList.Remove(currentPathCalcNode);
			//Stop when you add the target square to the closed list, in which case the path has been found (see note below)
			if (currentPathCalcNode.NodeCached.GridPosition == targetNodePosition)
			{
				movementState = CurrentState.FindingPath;
				//SmoothPath();
				PathFindNode = closedList[closedList.Count - 1];
				if (DebugRays)
				{
					Debug.DrawRay(currentPathCalcNode.NodeCached.transform.position, currentPathCalcNode.NodeCached.transform.up,
								  Color.blue, 50f);
				}
				return;
			}

			if (DebugRays)
			{
				Debug.DrawRay(currentPathCalcNode.NodeCached.transform.position, currentPathCalcNode.NodeCached.transform.up,
							  Color.red, secondsForRaysToStayUp);
			}
		}
		var listOfNodesToAdd = new List<CachedNode>(8);
		//For each of the 8 squares adjacent to this current square …
		foreach (var adjacentNode in currentPathCalcNode.NodeCached.AdjacentNodes)
		{
			if (adjacentNode.IsWalkable)
			{//If it is not walkable or if it is on the closed list, ignore it. Otherwise do the following
				if (closedList.All(cachedNode => adjacentNode.GridPosition != cachedNode.NodeCached.GridPosition))
				{
					//If it is on the open list already, check to see if this path to that square is better, using G cost as the measure. A lower G cost means 
					//that this is a better path. If so, change the parent of the square to the current square, and recalculate the G and F scores of the square
					if (openList.Any(cachedNode => adjacentNode.GridPosition == cachedNode.NodeCached.GridPosition))
					{
						var tempAdjNode = openList.Find(openNode => openNode.NodeCached.GridPosition == adjacentNode.GridPosition);

						if (tempAdjNode.GCost < currentPathCalcNode.GCost)
						{
							//Debug.Log("Recalculated " + tempAdjNode.NodeCached.gameObject.name + " GCost and FCost and parented it to " +
							//currentPathCalcNode.NodeCached.gameObject.name);
							tempAdjNode.ParentNode = currentPathCalcNode.NodeCached;
							tempAdjNode.GCost = currentPathCalcNode.GCost +
												GetGCost(tempAdjNode.NodeCached.transform.position, currentPathCalcNode.NodeCached.transform.position);
							tempAdjNode.RecalculateFCost();
						}
					}
					//If it isn’t on the open list, add it to the open list. Make the current square the parent of this square. 
					//Record the F, G, and H costs of the square. 
					else if (openList.All(cachedNode => adjacentNode.GridPosition != cachedNode.NodeCached.GridPosition))
					{

						var tempAdjNode =
							new CachedNode(
							//GetGCost(adjacentNode.transform.position, currentPathCalcNode.NodeCached.transform.position) + currentPathCalcNode.GCost,
								0, GetHCost(adjacentNode.GridPosition, targetNodePosition), adjacentNode, currentPathCalcNode.NodeCached);
						listOfNodesToAdd.Add(tempAdjNode);
						//Debug.Log("Added " + tempAdjNode.NodeCached.gameObject.name + " to the open list");
						//openList[openList.Count - 1].NodeCached.renderer.enabled = true;
						if (DebugRays)
						{
							Debug.DrawRay(tempAdjNode.NodeCached.transform.position, transform.up, Color.green, secondsForRaysToStayUp);
						}
					}
				}
			}
			openList.AddRange(listOfNodesToAdd);
			listOfNodesToAdd.Clear();
			//Debug.Log("The length of the open list is " + openList.Count);
		}
		if (openList.Count == 0)
		{
			canReachTarget = false;
		}
		else
		{
			canReachTarget = true;
		}
	}

	/// <summary>
	/// Works back from the target position, using each CachedNode's parent node to find the currentMoverPosition and makes a suitable path
	/// </summary>
	void FindPath()
	{
		if (PathFindNode.NodeCached)
		{
			if (PathFindNode.NodeCached.GridPosition != currentMoverGridPosition)
			{
				pathNodes.Add(PathFindNode);
				PathFindNode =
					closedList.Find(closedNode => closedNode.NodeCached.GridPosition == PathFindNode.ParentNode.GridPosition);
				if (DebugRays && PathFindNode.NodeCached)
				{
					Debug.DrawRay(PathFindNode.NodeCached.transform.position, Vector3.up, Color.white, secondsForRaysToStayUp);
				}
			}
			else
			{
				pathNodes.Reverse();
				SmoothPath();
				movementState = CurrentState.Moving;
			}
		}
	}

	/// <summary>
	/// Smooths the path so there's not so much jagged turns that look unnatural
	/// </summary>
	private void SmoothPath()
	{
		if (pathNodes.Count > 1)
		{
			var checkPoint = pathNodes[0];
			var currentPoint = pathNodes[1];
			var itemsToRemove = new List<CachedNode>();
			for (int i = 1; i < pathNodes.Count - 2; i++)
			{
				//If the area from the current check point to the next node is clear, remove the middle point
				if (Walkable(checkPoint.NodeCached.transform.position, pathNodes[i + 1].NodeCached.transform.position,
							 ((i + 1) - pathNodes.IndexOf(checkPoint))))
				{
					itemsToRemove.Add(pathNodes[i]);
					//then increment the current middle point by one
					currentPoint = pathNodes[i + 1];

				}
				//Otherwise change the checkpoint to that middle point
				else
				{
					checkPoint = currentPoint;
					currentPoint = pathNodes[i];
				}
			}
			//Remove all nodes in the path that we added before
			foreach (var node in itemsToRemove)
			{
				pathNodes.Remove(node);
			}
			if (DebugPathDraw)
			{
				int i = 0;
				foreach (var pathNode in pathNodes)
				{
					Debug.DrawLine(pathNode.NodeCached.transform.position,
								   pathNodes[i < pathNodes.Count - 2 ? ++i : pathNodes.Count - 1].NodeCached.transform.position,
								   new Color(255, 215, 0), secondsForPathsToStayUp);
				}
			}
			//foreach (var node in pathNodes)
			//{
			//	Debug.DrawRay(node.NodeCached.transform.position, transform.up, Color.black, 50f);
			//}
			/*checkPoint = starting point of path
		currentPoint = next point in path
		while (currentPoint->next != NULL)
		if Walkable(checkPoint, currentPoint->next)
		// Make a straight path between those points:
		temp = currentPoint
		currentPoint = currentPoint->next
		delete temp from the path
		else
		checkPoint = currentPoint
		currentPoint = currentPoint->next */
		}
	}

	/// <summary>
	/// Checks between to nodes to make sure there isn't any obstacles blocking the path Note:Obstacles must be tagged with an Obstacle tag
	/// </summary>
	/// <param name="firstNode">the starting node to check</param>
	/// <param name="secondNode">the node to stop checking at</param>
	/// <param name="numOfNodesInBetween">the number of other nodes on the path in between the firstNode and secondNode</param>
	/// <returns>returns true if there aren't any obstacles between the two nodes, otherwise returns false</returns>
	bool Walkable(Vector3 firstNode, Vector3 secondNode, int numOfNodesInBetween)
	{

		//For each node, check stepsBetweenNodes number of times if it's intersecting an obstacle
		for (var i = 1; i <= stepsBetweenNodes * numOfNodesInBetween; i++)
		{
			var position = new Vector3(firstNode.x + (secondNode.x - firstNode.x) / (stepsBetweenNodes * numOfNodesInBetween) * i, 1,
									   firstNode.z + (secondNode.z - firstNode.z) / (stepsBetweenNodes * numOfNodesInBetween) * i);
			if (DebugRays)
			{
				//if (stepsBetweenNodes % 5 == 0)
				//{
				//	Debug.DrawRay(position, Vector3.up, Color.clear, 10f);
				//}
				//else
				//{
				//	Debug.DrawLine(position, position + Vector3.up * 10, new Color(255, 215, 0), secondsForRaysToStayUp);
				//}
			}

			var tempObjects = Physics.OverlapSphere(position, 0.7f);
			//If you are intersecting an obstacle, return false
			if (tempObjects.Any(o => o.tag == "Obstacle"))
			{
				return false;
			}
		}
		//otherwise, return true
		return true;
	}

	/// <summary>
	/// Moves the mover towards the next node every update
	/// </summary>
	void MoveToNextNode()
	{

		if (transform.position.x <= nextNode.gameObject.transform.position.x + nodeIntersectOffset &&
			transform.position.x >= nextNode.gameObject.transform.position.x - nodeIntersectOffset &&
			transform.position.z <= nextNode.gameObject.transform.position.z + nodeIntersectOffset &&
			transform.position.z >= nextNode.gameObject.transform.position.z - nodeIntersectOffset)
		//If you've reached the position of the next node, go to the one after that
		{
			if (currentNode < pathNodes.Count - 1)
			{
				nextNode = pathNodes[++currentNode].NodeCached;
			}
			else
			{
				movementState = CurrentState.TargetReached;
				currentMoverGridPosition = nextNode.GridPosition;
			}

		}
		//If you haven't reached the next node, keep moving
		else
		{
			//In RotateTowards, current is the axis you want to point
			//(recommend using transform.forward/up/right)
			//Then you subtract your current position from your target's
			var temp =
				Quaternion.LookRotation(Vector3.RotateTowards(transform.forward,
															  nextNode.gameObject.transform.position - transform.position,
															  rotationSpeed * Time.deltaTime, rotationSpeed * Time.deltaTime));
			transform.rotation = new Quaternion(0, temp.y, 0, temp.w);
			transform.Translate(0, 0, speed * Time.deltaTime);
			//Debug.DrawRay(nextNode.gameObject.transform.position, nextNode.transform.up, Color.red, 1f);
		}

	}

	/// <summary>
	/// Changes the currentMoverGridPosition
	/// </summary>
	/// <param name="gridPosition"></param>
	void ChangeCurrentPosition(Vector2 gridPosition)
	{
		currentMoverGridPosition = gridPosition;
	}

	/// <summary>
	/// Gets a random node in the grid that is walkable
	/// </summary>
	/// <returns>A walkable node in the pathnode grid</returns>
	AStarNode GetRandomWaypoint()
	{
		AStarNode tempNode =
			aStarGrid.AStarNodes[Random.Range(0, aStarGrid.gridLength - 1), Random.Range(0, aStarGrid.gridWidth - 1)];
		while (!tempNode.IsWalkable)
		{
			tempNode =
			aStarGrid.AStarNodes[Random.Range(0, aStarGrid.gridLength - 1), Random.Range(0, aStarGrid.gridWidth - 1)];
		}
		return tempNode;
	}

	/// <summary>
	/// Returns a Heuristic score based on the difference between the two points
	/// </summary>
	/// <param name="currentPos">The current node you're calculating at</param>
	/// <param name="targetPos">The node to determine the heuristic from</param>
	/// <returns>the calculated cost</returns>
	int GetHCost(Vector2 currentPos, Vector2 targetPos)
	{
		int tempCost =
			//(int)
			//(Mathf.Abs(targetPos.x - currentPos.x) + Mathf.Abs(targetPos.y - currentPos.y)) * 10;
		(int)
			Mathf.Abs((targetPos - currentPos).sqrMagnitude);
		return tempCost;
	}
	/// <summary>
	/// Returns a travel based on the distance between the two nodes
	/// </summary>
	/// <param name="currentPos">The current node's position you're calculating at</param>
	/// <param name="targetPos">The node's position you're calculating travel cost from</param>
	/// <returns></returns>
	int GetGCost(Vector3 currentPos, Vector3 targetPos)
	{
		return (targetPos - currentPos).magnitude > 2 ? 14 : 10;
	}
}