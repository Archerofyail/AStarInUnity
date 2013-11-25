using UnityEngine;
using System.Collections;
[RequireComponent(typeof(Mover))]
public class PlayerChaser : MonoBehaviour
{
	public enum PlayerChaseState
	{
		Wandering,
		ChasingPlayer,
		MovingToLastSeenPoint
	}
	//Use the speed and rotation speed from the Mover object
	public Mover mover;
	public float ViewDistance;
	public float StopDistance;
	public PlayerChaseState ChaseState;
	public CapsuleFirstPerson Player;
	private Vector3 playersLastSeenPos;
	public bool hasCaughtPlayer { get; private set; }
	
	void Update()
	{
		switch (ChaseState)
		{
			case PlayerChaseState.Wandering:
			{
				//If the player is within viewing distance
				if ((Player.transform.position - transform.position).magnitude < ViewDistance)
				{
					//and there's nothing between you
					if (!Physics.Linecast(transform.position, Player.transform.position, 8))
					{
						//Start chasing him
						ChaseState = PlayerChaseState.ChasingPlayer;
						mover.enabled = false;
					}
				}
			}
			break;
			case PlayerChaseState.ChasingPlayer:
			{


				//If you haven't reached the player
				if ((Player.transform.position - transform.position).magnitude > StopDistance)
				{
					//Keep following
					var temp =
						Quaternion.LookRotation(Vector3.RotateTowards(transform.forward,
																	  Player.transform.position - transform.position,
																	  mover.rotationSpeed * Time.deltaTime, mover.rotationSpeed * Time.deltaTime));
					transform.rotation = new Quaternion(0, temp.y, 0, temp.w);
					transform.Translate(0, 0, mover.speed * Time.deltaTime);
				}
				else
				{
					hasCaughtPlayer = true;
				}
				//If you can still see the player, record his position
				if (!Physics.Linecast(transform.position, Player.transform.position, 8) && (Player.transform.position - transform.position).magnitude < ViewDistance)
				{
					playersLastSeenPos = Player.transform.position;
				}
				//If the player's out of your range or is behind a wall
				if ((Player.transform.position - transform.position).magnitude > ViewDistance ||
						Physics.Linecast(transform.position, Player.transform.position, 8))
				{
					//start moving to his last seen position

					ChaseState = PlayerChaseState.MovingToLastSeenPoint;
				}
			}
			break;
			case PlayerChaseState.MovingToLastSeenPoint:
			{
				//If you've made it to the last place you saw the player
				if (transform.position.x <= playersLastSeenPos.x + mover.nodeIntersectOffset &&
					transform.position.x >= playersLastSeenPos.x - mover.nodeIntersectOffset &&
					transform.position.z <= playersLastSeenPos.z + mover.nodeIntersectOffset &&
					transform.position.z >= playersLastSeenPos.z - mover.nodeIntersectOffset)
				{
					//And the player is in range
					if ((Player.transform.position - transform.position).magnitude < ViewDistance)
					{
						//And there's no obstacles between you and the player
						if (!Physics.Linecast(transform.position, Player.transform.position, 8))
						{
							//start chasing him
							ChaseState = PlayerChaseState.ChasingPlayer;
						}
						else
						{
							//otherwise, go back to wandering
							ChaseState = PlayerChaseState.Wandering;
							mover.enabled = true;
						}
					}
				}
				//If you haven't made it to the last place you saw him, keep moving
				else
				{
					var temp =
						Quaternion.LookRotation(Vector3.RotateTowards(transform.forward,
																	  playersLastSeenPos - transform.position,
																	  mover.rotationSpeed * Time.deltaTime, mover.rotationSpeed * Time.deltaTime));
					transform.rotation = new Quaternion(0, temp.y, 0, temp.w);
					transform.Translate(0, 0, mover.speed * Time.deltaTime);
				}

			}
			break;
		}



	}


	void OnCollisionEnter(Collision other)
	{
		if (other.gameObject.tag == "Player")
		{
			hasCaughtPlayer = true;
		}
	}
}
