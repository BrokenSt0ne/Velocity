﻿using UnityEngine;
using System.Collections;

public abstract class Movement : MonoBehaviour
{
	public bool canMove = true;
	public bool autojump = true;
	public float speed = 1f;
	public float maxSpeed = 10f;
	public float jumpForce = 1f;
	public LayerMask groundLayers;
	public PhysicMaterial friction;
	public PhysicMaterial noFriction;

	public GameObject camObj;
	public bool crouched = false;
	public float lastJumpPress = -1f;

	void Awake()
	{
		camObj = transform.FindChild("Camera").gameObject;
	}
	
	void Start()
	{
		GameInfo.info.addWindowLine("XZ-Speed: ", getXzVelocityString);
		GameInfo.info.addWindowLine("Y-Speed: ", getYVelocityString);
		GameInfo.info.addWindowLine("Speed 'limit': ", getMaxSpeedString);
		GameInfo.info.addWindowLine("Crouched: ", getCrouchedString);
		GameInfo.info.addWindowLine("On Ground: ", getGroundString);
	}
	
	void Update()
	{
		if(Input.GetButtonDown("Jump") || autojump && Input.GetButton("Jump"))
		{
			lastJumpPress = Time.time;
		}
		
		if(Input.GetButtonDown("Respawn"))
		{
			respawnPlayer();
		}
		if(Input.GetButtonDown("Reset"))
		{
			resetPlayer();
			WorldInfo.info.reset();
		}
		if(Input.GetButton("Crouch"))
		{
			setCrouched(true);
		}
		else
		{
			setCrouched(false);
		}
	}

	void FixedUpdate()
	{
		if(canMove)
		{
			Vector3 additionalVelocity = calculateAdditionalVelocity();
			
			//Apply
			if(!rigidbody.isKinematic)
			{
				rigidbody.velocity += additionalVelocity;
			}
		}
	}

	public virtual Vector3 calculateAdditionalVelocity()
	{
		return Vector3.zero;
	}
	
	void OnTriggerEnter(Collider other)
	{
		if(other.tag.Equals("Teleporter"))
		{
			Teleporter tp = other.GetComponent<Teleporter>();
			transform.position = tp.target;
			if(tp.applyRotation)
			{
				transform.rotation = tp.targetRotation;
			}
			if(tp.cancelVelocity)
			{
				rigidbody.velocity = Vector3.zero;
			}
		}
	}

	void OnCollisionEnter(Collision col)
	{
		foreach(ContactPoint point in col.contacts)
		{
			if(point.normal.y > 0.5f)
			{
				col.gameObject.collider.material = friction;
			}
			else
			{
				col.gameObject.collider.material = noFriction;
			}
		}
	}

	private void spawnPlayer(Respawn spawn)
	{
		if(spawn != null)
		{
			transform.position = spawn.getSpawnPos();
			camObj.transform.rotation = spawn.getSpawnRot();
			rigidbody.velocity = Vector3.zero;
			lastJumpPress = -1f;
		}
		else
		{
			print("Tried to respawn, but no spawnpoint selected. RIP :(");
		}
	}
	
	private void respawnPlayer()
	{
		spawnPlayer(GameInfo.info.getCurrentSpawn());
	}

	private void resetPlayer()
	{
		spawnPlayer(GameInfo.info.getFirstSpawn());
	}
	
	public bool checkGround()
	{
		Vector3 pos = new Vector3(transform.position.x, transform.position.y - collider.bounds.extents.y + 0.05f, transform.position.z);
		Vector3 radiusVector = new Vector3(collider.bounds.extents.x, 0f, 0f);
		return checkCylinder(pos, radiusVector, 8);
	}

	private bool checkCylinder(Vector3 origin, Vector3 radiusVector, int rayCount)
	{
		for(int i = 0; i < rayCount; i++)
		{
			Vector3 radius = Quaternion.Euler(new Vector3(0f, i * (360f / rayCount), 0f)) * radiusVector;
			Vector3 circlePoint = origin + radius;

			RaycastHit hit;
			bool hasHit = Physics.Raycast(circlePoint, -Vector3.up, out hit, 0.1f, groundLayers);
			Debug.DrawLine(circlePoint, circlePoint - Vector3.up);
			//Collided with something
			if(hasHit)
			{
				//Maybe do some angle calculations here to avoid jumping up slopes?
				return true;
			}
		}
		return false;
	}
	
	private void setCrouched(bool state)
	{
		MeshCollider col = (MeshCollider)collider;

		if(!crouched && state)
		{
			//crouch
			col.transform.localScale = new Vector3(col.transform.localScale.x, 0.5f, col.transform.localScale.z);
			transform.position += new Vector3(0f,0.5f,0f);
			camObj.transform.localPosition += new Vector3(0f,-0.25f,0f);
			crouched = true;
		}
		else if(crouched && !state)
		{
			//uncrouch
			Ray ray = new Ray(transform.position - new Vector3(0f, -0.5f, 0f), Vector3.up);

			//Todo unrouch by extending down except when on ground
			if(!Physics.SphereCast(ray, 0.5f, 2f, groundLayers))
			{
				col.transform.localScale = new Vector3(col.transform.localScale.x, 1f, col.transform.localScale.z);
				transform.position += new Vector3(0f,0.5f,0f);
				camObj.transform.localPosition += new Vector3(0f,0.25f,0f);
				crouched = false;
			}
		}
	}
		
	private float getVelocity()
	{
		return Vector3.Magnitude(rigidbody.velocity);
	}
	
	private string getXzVelocityString()
	{
		float mag = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z).magnitude;
		string magstr = mag.ToString();
		if(magstr.ToLower().Contains("e"))
		{
			return "0";
		}
		return roundString(magstr, 2);
	}
	
	private string getYVelocityString()
	{
		string v = rigidbody.velocity.y.ToString();
		if(v.ToLower().Contains("e"))
		{
			return "0";
		}
		return roundString(v, 2);
	}
	
	private string getMaxSpeedString()
	{
		return maxSpeed.ToString();
	}
	
	private string getCrouchedString()
	{
		return crouched.ToString();
	}

	private string getGroundString()
	{
		return checkGround().ToString();
	}
	
	private string roundString(string input, int digitsAfterDot)
	{
		if(input.Contains("."))
		{
			return input.Substring(0, input.IndexOf('.') + digitsAfterDot);
		}
		else
		{
			return input;
		}
	}
}
