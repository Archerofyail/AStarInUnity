using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Linq;

[RequireComponent(typeof(Mover))]
public class NodeDisplay : MonoBehaviour
{
	public Sprite OpenListTex;
	public Sprite ClosedListTex;
	public Sprite PathListTex;
	public Sprite currentPosTex;
	public SpriteRenderer NodeSpritePrefab;

	public List<CachedNode> openList;
	public List<CachedNode> closedList;
	public List<CachedNode> pathList;

	private int lastOpenListSize;
	private int lastClosedListSize;
	private int lastPathListSize;

	public float texWidthHeight = 15;

	public Camera mainCam;

	public bool displayNodes = true;
	public bool displayCurrentPos = true;
	private float fps = 0;

	private Mover mover;
	private List<SpriteRenderer> spritePool;
	private List<bool> spriteIsTaken;

	void Start()
	{
		spritePool = new List<SpriteRenderer>(FindObjectOfType<AStarGridCreator>().AStarNodes.Length);
		spriteIsTaken = new List<bool>(spritePool.Capacity);
		for (int i = 0; i < spritePool.Capacity; i++)
		{
			spritePool.Add(Instantiate(NodeSpritePrefab));
			spritePool[i].sprite = OpenListTex;
			spriteIsTaken.Add(false);
		}
		print("texWidthHeight was " + texWidthHeight);
		print("Screen.height is " + Screen.height);
		texWidthHeight *= (Screen.height / 650f);
		print("texWidthHeight now " + texWidthHeight);
		mover = GetComponent<Mover>();
		mainCam = Camera.main;
		openList = mover.openList;
		closedList = mover.closedList;
		pathList = mover.pathNodes;
		StartCoroutine(SetLists());
		StartCoroutine(SetNodeDisplay());
	}

	IEnumerator SetNodeDisplay()
	{
		while (true)
		{
			//print("Ran SetNodeDisplay");
			if (displayNodes)
			{
				var i = 0;
				if (openList != null)
				{

					for (i = 0; i < openList.Count; i++)
					{

						SetSprite(openList[i].NodeCached.transform.position, OpenListTex);
					}

				}
				if (closedList != null)
				{


					for (i = 0; i < closedList.Count; i++)
					{

						var sprite = SetSprite(closedList[i].NodeCached.transform.position, ClosedListTex);
						sprite.sortingOrder = 1;
					}
				}
				if (pathList != null)
				{

					for (i = 0; i < pathList.Count; i++)
					{
						var sprite = SetSprite(pathList[i].NodeCached.transform.position, PathListTex);
						sprite.sortingOrder = 2;
					}

				}

			}
			if (displayCurrentPos)
			{
				var sprite = SetSprite(transform.position, currentPosTex);
				sprite.sortingOrder = 3;
			}
			yield return null;
		}
	}

	IEnumerator SetLists()
	{
		while (openList == null && closedList == null && pathList == null)
		{
			if (openList == null)
			{
				openList = mover.openList;
			}
			if (closedList == null)
			{
				closedList = mover.closedList;
			}
			if (pathList == null)
			{
				pathList = mover.pathNodes;
			}
			yield return null;
		}
	}

	private void Update()
	{
		for (int i = 0; i < spritePool.Count; i++)
		{
			spritePool[i].transform.position = Vector3.left * 700;
		}
		fps = 1 / Time.deltaTime;



	}

	void LateUpdate()
	{
		for (int i = 0; i < spriteIsTaken.Count; i++)
		{
			spriteIsTaken[i] = false;
		}
	}

	SpriteRenderer SetSprite(Vector3 pos, Sprite sp)
	{
		var screenPos = (pos + (Vector3.up * 10));
		var index = spriteIsTaken.IndexOf(false);
		spriteIsTaken[index] = true;
		var sprite = spritePool[index];
		if (sprite.sprite.name != sp.name)
		{
			sprite.sprite = sp;
		}
		sprite.transform.position = screenPos;
		return sprite;
	}

	private void OnGUI()
	{
		GUILayout.BeginArea(new Rect(Screen.width * 0.8f, Screen.height * 0.01f, Screen.width * 0.2f, Screen.height * 0.8f));
		displayNodes = GUILayout.Toggle(displayNodes, "Display Nodes");
		GUILayout.Label("Pathfinding iterations per frame:");
		mover.iterationsPerFrame = int.Parse(GUILayout.TextField(mover.iterationsPerFrame.ToString()));
		GUILayout.Label("FPS: " + fps);
		GUILayout.EndArea();
		//if (displayNodes)
		//{
		//	if (openList != null)
		//	{
		//		foreach (var node in openList)
		//		{
		//			if (!closedList.Contains(node) && !pathList.Contains(node))
		//			{
		//				var screenPos = mainCam.WorldToScreenPoint(node.NodeCached.transform.position);
		//				GUI.DrawTexture(
		//					new Rect(screenPos.x - texWidthHeight / 2, Screen.height - (screenPos.y + texWidthHeight / 2), texWidthHeight,
		//						texWidthHeight), OpenListTex);
		//			}
		//		}
		//	}
		//	if (closedList != null)
		//	{

		//		foreach (var node in closedList)
		//		{
		//			if (!pathList.Contains(node))
		//			{
		//				var screenPos = mainCam.WorldToScreenPoint(node.NodeCached.transform.position);
		//				GUI.DrawTexture(
		//					new Rect(screenPos.x - texWidthHeight / 2, Screen.height - (screenPos.y + texWidthHeight / 2), texWidthHeight,
		//						texWidthHeight), ClosedListTex);
		//			}
		//		}
		//	}
		//	if (pathList != null)
		//	{
		//		foreach (var node in pathList)
		//		{
		//			var screenPos = mainCam.WorldToScreenPoint(node.NodeCached.transform.position);
		//			GUI.DrawTexture(
		//				new Rect(screenPos.x - texWidthHeight / 2, Screen.height - (screenPos.y + texWidthHeight / 2), texWidthHeight,
		//					texWidthHeight), PathListTex);
		//		}
		//	}

		//}
		//if (displayCurrentPos)
		//{
		//	var moverScreenPos = mainCam.WorldToScreenPoint(transform.position);
		//	GUI.DrawTexture(
		//		new Rect(moverScreenPos.x - texWidthHeight/2, Screen.height - (moverScreenPos.y + texWidthHeight/2),
		//			texWidthHeight,
		//			texWidthHeight), currentPosTex);
		//}
	}
}
