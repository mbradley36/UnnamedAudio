// Miranda Bradley
// mbradley36
// CS 6457
// all audio from freesound.com

using UnityEngine;
using System.Collections;

/// <summary>
/// PLEASE NOTE:  This is ONLY an example of how to use the CharController Update and 
/// OnControllerColliderHit methods for this example.
/// 
/// It is NOT meant to be an example of the kinds of avatar behavior you should aim for
/// in P1.  The motion of the avatar is INTENTIONALLY bad, serving only to show that you 
/// can really just use whatever inputs you want to control the motion of the transforms.
/// 
/// We recommend that instead of using simple checks on the values of the last moveDirection
/// that you create a simple state machine (storing the state in a private class variable)
/// and base your motion computations in each state with what you want that state to feel like.   
/// For the Mario example in the book, states might include STILL, MOVELEFT, MOVERIGHT,  
/// JUMP, STARTFALL, ENDFALL, LANDED.  A state might be started based on key input (press space),
/// time (first .1 seconds of fall after apex reached), or collision (hit ground, hit wall, hit
/// platform from below).
/// 
/// Think through your states, and how you can tell when they start, and how you compute their 
/// motion during the state.
/// 
/// Remember that Update is called frequently, and you should use Time.deltaTime to see how long 
/// its been since the last update.  By computing your desired motions based on time (e.g., I want
/// the character to move XXX far during the first 0.1 seconds) and then monitoring how long 
/// it's been in a state (e.g., adding Time.deltaTime to a counter that you initialize when you 
/// enter a state), you can create motion that behaves correctly no matter how fast it runs.
/// 
/// </summary>
/// 
public class AvatarScript2 : MonoBehaviour {
	//Physics references
	float speed;
    public float lightSpeed = 6.0F;
    public float heavySpeed = 2.0F;
    public float jumpSpeed = 8.0F;
    public float gravity = 10.0F;

	public AudioClip collideLight; // our clips to be assigned
	public AudioClip landLight;
	public AudioClip collideHeavy;
	public AudioClip landHeavy;
	
    private Vector3 moveDirection = Vector3.zero;
	//Animation references:  if you want to control the kids and do something with them
	int counter = 0;
	GameObject head;
	GameObject body;
	
	// a simple start to Character controller capabilities
	bool bumpTop;
	bool bumpLeft;
	bool bumpRight;
	bool bumpBottom;
	bool onWall = false;
	bool isGrounded = false;

	private int animState; // different states of animation
	const int idle = 0;
	const int walking = 1;
	const int falling = 2;
	const int colliding = 3;
	
	void Start (){
     	// Assign body parts to variables;  
		// -> could also have these as properties you set in editor
		// -> could also have used Transform.Find to only search in the children of this object
		speed = lightSpeed;
	 	head = GameObject.Find ("Head");
     	body = GameObject.Find ("Body");
	 	counter = 0;
		bumpTop = bumpLeft = bumpRight = bumpBottom = false;
		isGrounded = false;
		onWall = false;
	}

	void Update() {
        if (isGrounded) {
        	if (!onWall) {
	            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, 0f);
	        } else {
	        	if(bumpLeft && Input.GetAxis("Horizontal") > 0) {
	        		moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, 0f);
	        	} else if(bumpLeft) {
	        		moveDirection = new Vector3(0f, 0f, 0f);
	        	} else if (bumpRight && Input.GetAxis("Horizontal") < 0) {
	        		moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, 0f);
	        	} else if (bumpRight) {
	        		moveDirection = new Vector3(0f, 0f, 0f);
	        	}
	        }
	        moveDirection = transform.TransformDirection(moveDirection);
           	moveDirection *= speed;
            
            if (Input.GetButton("Jump")){
                moveDirection.y = jumpSpeed;
				body.transform.Rotate(new Vector3(0,0,0));    
			}
        }
		float delta = (Mathf.Sin(counter*0.2F)/2)+1.37118F;
		float delta2 = (Mathf.Sin(counter*0.2F));

		/*switch (state) { // determine which method to call
			case idle:		Squish(); 		break;
			case walking:		JellySwim();	break;
			case falling:							break;
			case colliding: 					break;
		}*/
				
		if (bumpTop)
		{
			moveDirection.y = 0;
		}
		
		if(moveDirection.y > 1){
			// Jumping 
			head.transform.localPosition = new Vector3(0F,delta,0F);
			body.transform.Rotate(new Vector3(0,0,-body.transform.localEulerAngles.z));
		} else if(moveDirection.y < -0.35){
			// Falling
			head.transform.localPosition = new Vector3(0F,delta,0F);
			body.transform.Rotate(new Vector3(0,0,delta2));
	    } else {
			// Walking
			if(isGrounded && !onWall){
			 	head.transform.localScale = new Vector3 (1F,1F,1F);
			 	head.transform.localPosition = new Vector3 (0F,1.371178F,0F);
			 	if (moveDirection.x != 0 ) { 	
			 		body.transform.Rotate(new Vector3(0,0,delta2));
				} else {
			 		body.transform.Rotate(new Vector3(0,0,-body.transform.localEulerAngles.z));		
			 	}
			}
		}
		
		if (isGrounded) 
		{
		} else {
			if(bumpLeft || bumpRight) { // change x direction if wall hit while falling
				moveDirection.x = -moveDirection.x;
				moveDirection.y -= gravity * Time.deltaTime;
			} else {
				moveDirection.y -= gravity * Time.deltaTime;
			}
		} 
        transform.Translate(moveDirection * Time.deltaTime);
		counter++;
    }
	
    void FixedUpdate() {
		// reinitialize for checking for collisions.  FixedUpdate is called BEFORE any collisions.
		// bumpTop = bumpLeft = bumpRight = bumpBottom = false;
		bumpTop = bumpBottom = false;
		isGrounded = false;
	}
	
//		moveDirection = Vector3.Reflect(moveDirection, hit.normal);
	
	//
	// *** Collision handling.  Update global variables for use in state machines
	//
	void OnCollisionEnter (Collision collision) {
        //Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
        //Vector3 pos = contact.point;

        foreach (ContactPoint c in collision.contacts) {
            //Debug.Log(c.thisCollider.name + " STAY ON " + c.otherCollider.name);
            //Debug.Log("Collision: " + c.point + ", Normal: " + c.normal);
        }
		
		// quick and dirty check on where we might have bumped (top or bottom).  
		// really SHOULD be checking for LOCATION on capsule, rather than direction of motion
		// but this demonstrates one way of looking at the collions
        ContactPoint contact = collision.contacts[0];
		float dot = Vector3.Dot (contact.normal, Vector3.up);
		float dotSide = Vector3.Dot (contact.normal, Vector3.right);

		if (dot < -0.5) {
			bumpTop = true;
			Debug.Log ("Bumped with Top");
		}
		else if (dot > 0.5)
		{
			bumpBottom = true;
			Debug.Log ("Bumped with Bottom");
		}
		else 
		{
			// determine which side has a collision
			if (dotSide < -0.5 ) {
				Debug.Log("Bumped with Right");
				bumpRight = true;
			} else if (dotSide > 0.5 ) {
				Debug.Log("Bumped with Left");
				bumpLeft = true;
			}
		}
	}

	void OnCollisionStay (Collision collision) {
        foreach (ContactPoint c in collision.contacts) {
            //Debug.Log(c.thisCollider.name + " STAY ON " + c.otherCollider.name);
            //Debug.Log("Collision: " + c.point + ", Normal: " + c.normal);
        }
		
		// if we have an ongoing collion, and it's with the bottom, we consider this "grounded"
        ContactPoint contact = collision.contacts[0];
		float dot = Vector3.Dot (contact.normal, Vector3.up);
		float dotSide = Vector3.Dot (contact.normal, Vector3.right);
		if (dot > 0.5)
		{
			isGrounded = true;			
			//Debug.Log ("Collision Stay GROUNDED");
		} else {
			//Debug.Log ("Collision Stay not grounded");
		}

		if (dotSide < -0.5 ) {
			Debug.Log("Bumped with Right");
			bumpRight = true;
			onWall = true;
		} else if (dotSide > 0.5 ) {
			Debug.Log("Bumped with Left");
			bumpLeft = true;
			onWall = true;
		}
	}
	
	void OnCollisionExit () {
		//Debug.Log ("Collision Exit");
		
		// technically, there could be multiple simultaneous collisions (e.g., in a corner), so we should 
		// keep track of which ones are ending here
		isGrounded = false;
		onWall = bumpLeft = bumpRight = false;
	}
}