using System.Collections.Generic;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Mover))]
public class NodeDisplay : MonoBehaviour
{
	public Texture OpenListTex;
	public Texture ClosedListTex;
	public Texture PathListTex;
	public Texture currentPosTex;

	public List<CachedNode> openList;
	public List<CachedNode> closedList;
	public List<CachedNode> pathList;

	public float texWidthHeight = 15;

	public Camera mainCam;

	public bool displayNodes = true;
	public bool displayCurrentPos = true;

	private Mover mover;

	void Start()
	{
		print("texWidthHeight was " + texWidthHeight);
		print("Screen.height is " + Screen.height);
		texWidthHeight *= (Screen.height / 650f);
		print("texWidthHeight now " + texWidthHeight);
		mover = GetComponent<Mover>();
		mainCam = Camera.main;
		openList = mover.openList;
		closedList = mover.closedList;
		pathList = mover.pathNodes;

	}

	private void Update()
	{
		if (openList == null)
		{
			openList = mover.openList;
		}
		if (closedList == null)
		{
			closedList = mover.closedList;
		}
		if (closedList == null)
		{
			pathList = mover.pathNodes;
		}
	}

	private void OnGUI()
	{
		if (displayNodes)
		{
			if (openList != null)
			{
				foreach (var node in openList)
				{
					if (!closedList.Contains(node) && !pathList.Contains(node))
					{
						var screenPos = mainCam.WorldToScreenPoint(node.NodeCached.transform.position);
						GUI.DrawTexture(
							new Rect(screenPos.x - texWidthHeight / 2, Screen.height - (screenPos.y + texWidthHeight / 2), texWidthHeight,
								texWidthHeight), OpenListTex);
					}
				}
			}
			if (closedList != null)
			{

				foreach (var node in closedList)
				{
					if (!pathList.Contains(node))
					{
						var screenPos = mainCam.WorldToScreenPoint(node.NodeCached.transform.position);
						GUI.DrawTexture(
							new Rect(screenPos.x - texWidthHeight / 2, Screen.height - (screenPos.y + texWidthHeight / 2), texWidthHeight,
								texWidthHeight), ClosedListTex);
					}
				}
			}
			if (pathList != null)
			{
				foreach (var node in pathList)
				{
					var screenPos = mainCam.WorldToScreenPoint(node.NodeCached.transform.position);
					GUI.DrawTexture(
						new Rect(screenPos.x - texWidthHeight / 2, Screen.height - (screenPos.y + texWidthHeight / 2), texWidthHeight,
							texWidthHeight), PathListTex);
				}
			}
			
		}
		if (displayCurrentPos)
		{
			var moverScreenPos = mainCam.WorldToScreenPoint(transform.position);
			GUI.DrawTexture(
				new Rect(moverScreenPos.x - texWidthHeight/2, Screen.height - (moverScreenPos.y + texWidthHeight/2),
					texWidthHeight,
					texWidthHeight), currentPosTex);
		}
	}
}
