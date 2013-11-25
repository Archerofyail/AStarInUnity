using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class CapsuleFirstPerson : MonoBehaviour
{

	public float defaultSpeed = 6f;
	public float boostSpeed = 10f;
	public float rotationSpeed = 300.0f;
	public int boostSpeedTimer = 120;
	public int speedBoostRechargeTimer;
	public int speedBoostRechargeTimerMax = 180;
	public int rotateTimerMax = 30;
	private float hMouseInput;
	private float vMouseInput;
	private float vInput;
	private float sInput;
	private float moveHorizontal;
	private float moveMouseVertical;
	private float moveVertical;
	private float moveStrafe;
	private float currentSpeed = 3f;
	private bool canBoost = false;
	private int rotateTimer;
	private int firstJumpTimer;

	Quaternion targetRotation;
	Vector3 angles;
	bool grounded = true;
	bool hasDoubleJumped = false;
	bool startedRotating = false;
	public Camera characterCam;

	public Vector2 currentGridPosition;
	void Update()
	{
		hMouseInput = Input.GetAxis("Mouse X"); // Get the value of mouse on the X axis (between 1 and -1)
		vMouseInput = Input.GetAxis("Mouse Y"); // Get the value of mouse on the Y axis ( between 1 and -1)
		vInput = Input.GetAxis("Vertical"); // Get the value of the vertical axis (between 1 and -1)
		sInput = Input.GetAxis("Horizontal");
		// Reduces the actual movement based on how long each update is.
		moveHorizontal = hMouseInput * Time.deltaTime;
		moveVertical = vInput * Time.deltaTime;
		moveStrafe = sInput * Time.deltaTime;
		moveMouseVertical = vMouseInput * Time.deltaTime;

		var tempObjects = Physics.OverlapSphere(transform.position, 0.5f, 9);
		if (tempObjects.Length > 0)
		{
			//currentGridPosition = tempObjects[0].GetComponent<AStarNode>().GridPosition;
		}

		if (Input.GetKey(KeyCode.LeftShift) && canBoost)
		{
			currentSpeed = boostSpeed;
			boostSpeedTimer--;
			if (boostSpeedTimer <= 0)
			{
				canBoost = false;
				boostSpeedTimer = 120;
			}
		}
		else
		{
			currentSpeed = defaultSpeed;
		}

		if (!canBoost)
		{
			speedBoostRechargeTimer++;
			if (speedBoostRechargeTimer >= speedBoostRechargeTimerMax)
			{
				canBoost = true;
				speedBoostRechargeTimer = 0;
			}
		}

		if (grounded) // if the capsule is on the ground
		{
			// when the space key is pressed the capsule should jump
			if (Input.GetKey(KeyCode.Space))
			{
				firstJumpTimer++;
				
				grounded = false; // set grounded to false
				// apply a relative force to the capsule up and in the direction the capsule is currently moving.
				rigidbody.AddRelativeForce(sInput * currentSpeed * 50, 250, vInput * currentSpeed * 50);
			}
			// move the capsule on the z axis at the rate determined by moveVertical * s peed
			transform.Translate(0, 0, moveVertical * currentSpeed);
			// if the s Input is not 0 then s trafe, otherwise rotate.
			if (sInput != 0)
			{
				transform.Translate(moveStrafe * currentSpeed, 0, 0);
			}
		}
		else
		{
			transform.Translate(0, 0, moveVertical * currentSpeed);
			if (sInput != 0)
			{
				transform.Translate((sInput * currentSpeed) * Time.deltaTime, 0, (vInput * currentSpeed) * Time.deltaTime);
			}

			if ( firstJumpTimer <= 15)
			{
				firstJumpTimer++;
			}
		}
		if (!grounded && !hasDoubleJumped && firstJumpTimer >= 15)
		{
			if (Input.GetKey(KeyCode.Space))
			{
				rigidbody.AddRelativeForce(sInput * currentSpeed * 25, 250, vInput * currentSpeed * 25);
				hasDoubleJumped = true;
			}
		}

		if (Input.GetKeyDown(KeyCode.E))
		{
			startedRotating = true;
		}

		if (startedRotating)
		{
			rotateTimer++;
			if (rotateTimer <= rotateTimerMax)
			{
				transform.Rotate(Vector3.up, 180 / rotateTimerMax);
			}
			if (rotateTimer >= rotateTimerMax)
			{
				startedRotating = false;
				rotateTimer = 0;
			}
		}

		var vDown = transform.TransformDirection(Vector3.down);
		if (Physics.Raycast(transform.position, vDown, 1.5f) && firstJumpTimer >= 15)
		{
			firstJumpTimer = 0;
			grounded = true;
			hasDoubleJumped = false;
		}


		// rotate the capsule around the y axis at the rate determined by moveHorizontal * rotationSpeed)
		transform.Rotate(Vector3.up, moveHorizontal * rotationSpeed);
		// rotate the capsule around the x axis at the rate determined by moveMouseVertical * rotationSpeed)
		characterCam.transform.Rotate(Vector3.right, -moveMouseVertical * rotationSpeed);
		// Clamp the camera if necessary.
		angles = characterCam.transform.eulerAngles;
		if (angles.x < 315.0f && angles.x > 290.0f)
		{
			angles.x = 315.0f;
		}
		else if (angles.x > 40.0f && angles.x < 315.0f)
		{
			angles.x = 40.0f;
		}
		// Res et the camera rotation in case the angle has been clamped.
		targetRotation = Quaternion.Euler(angles.x, angles.y, 0);
		characterCam.transform.rotation = targetRotation;
		// if th e capsule is touching the ground set the grounded bool to true.
		if (Input.GetKeyDown(KeyCode.Z))
		{
			var temp = GameObject.FindGameObjectsWithTag("Pather");
			foreach (var gObject in temp)
			{
				gObject.BroadcastMessage("StartPathing", SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.tag == "PathNode")
		{
			currentGridPosition = other.GetComponent<AStarNode>().GridPosition;
		}
	}
}
