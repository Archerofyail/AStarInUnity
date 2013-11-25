using UnityEngine;
using System.Linq;

public class ChaserManager : MonoBehaviour
{

	public PlayerChaser[] Chasers;
	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
		if (Chasers.Any(chase => chase.ChaseState == PlayerChaser.PlayerChaseState.ChasingPlayer) &&
			Chasers.Any(chase => chase.ChaseState != PlayerChaser.PlayerChaseState.ChasingPlayer))
		{
			foreach (var chaser in Chasers)
			{
				if (chaser.ChaseState == PlayerChaser.PlayerChaseState.Wandering)
				{
					chaser.mover.targetNodePosition =
							Chasers.First(foundChaser => foundChaser.ChaseState == PlayerChaser.PlayerChaseState.ChasingPlayer)
								   .mover.currentMoverGridPosition;
					chaser.mover.movementState = Mover.CurrentState.Idling;
				}
			}
		}

	}

	void OnGUI()
	{
		if (Chasers.Any(chaser => chaser.hasCaughtPlayer))
		{
			Time.timeScale = 0;
			GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 100, 150, 75),
					  "You got caught! You survived for " +
					  GameObject.Find("Player").GetComponent<PlayerInfoDisplay>().secondsSurvived +
					  " seconds. Do you want to try again?");
			if (GUI.Button(new Rect(Screen.width / 2 -55, Screen.height / 2, 100, 25),
						   "Yes"))
			{
				Application.LoadLevel(0);

			}
			if (GUI.Button(new Rect(Screen.width / 2 + 55, Screen.height / 2, 100, 25),
						   "No"))
			{
				Application.LoadLevel(0);

			}
		}
	}
}
