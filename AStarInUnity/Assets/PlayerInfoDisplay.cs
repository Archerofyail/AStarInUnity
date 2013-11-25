using UnityEngine;
using System.Collections;

public class PlayerInfoDisplay : MonoBehaviour
{
	public float secondsSurvived;

	void Update()
	{
		secondsSurvived += Time.deltaTime;
	}

	void OnGUI()
	{
		GUI.Label(new Rect(50,50,125,50), "Time Survived: " + secondsSurvived + " seconds");
	}
}
