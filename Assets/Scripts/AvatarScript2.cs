// Miranda Bradley
// mbradley36
// CS 6457
// all audio from freesound.com

using UnityEngine;
using System.Collections;
using XInputDotNetPure;
//using XInputInterface;

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
	// movement property references
    public float speed = 6.0F;
    public float jumpSpeed = 8.0F;
	public float gravity = 10.0F;

	public float charWidth;
	private float jumpStartY;
	
	// could get these from the collider, but wanted to be able to change them
	public float cHeight = 4.5f; 
	public float cWidth = 2f; 
	// a small delta for making sure the avatar remains in contact with the object it collided with
	public float cDelta = 0.01f;
	
	// our movement vector
	private Vector3 moveDirection = Vector3.zero;

	public Material lightMTRL; // to change between MTRLs
	public Material heavyMTRL;

	public ParticleSystem scrapeParticles;

	public AnimationCurve motorCurve, motorExitCurve, motorEnterCurve;
	public float motorStart, motorStop;
	//private float motorStart, secondMotorStart;
	private bool motorsOn = false;
	public int motorGapLength;
	public float gapCorrection;
	private int motorTimer = 0;

	public GameObject guideChar;
	private float guideCharsX;

	public int levelHeight;

	public AudioClip collideLight; // our clips to be assigned
	public AudioClip landLight;
	public AudioClip platformLand, ghostPlatformLand;
	public AudioClip collideHeavy;
	public AudioClip landHeavy;
	public AudioClip groundScrape;
	public AudioClip jumpUp;

	private AudioClip[] pitchArray;
	public AudioClip pitch1, pitch2, pitch3, pitch4, pitch5, pitch6, pitch7;
	private int currPitchPos;

	private AudioClip landSound; // to hold current char's clips
	private AudioClip collideSound;

	// Animation references:  if you want to control the kids and do something with them
	int counter = 0;
	GameObject head;
	GameObject body;

	public int animState; // different states of animation
	const int idle = 0;
	const int walking = 1;
	const int jumping = 2;
	const int falling = 3;
	private int prevState;

	private int goalRot = 0; // for character 1's flips
	private float prevDirection; // to compare for direction changes
	private bool directionChange = false;

	private int currChar; // 1 - light, 2 - heavy

	private bool debug = false; // for turning on/off debug log
	private bool firstInit;
	private bool canDoubleJump = false; // for knowing when to switch jump types
	private bool canFirstJump = false;
	private bool doubleJumping = false;


	float delta;
	float delta2;

	// a simple start to Character controller capabilities
	bool bumpTop;
	bool bumpLeft;
	bool bumpRight;
	bool bumpBottom;
	bool isGrounded = false;
	// "depth" of the collider into the object collided with.  Assume it's only in the 4 principle directions 
	float leftD, rightD, topD, bottomD;
	// need to move the clearing of variables out of fixedUpdate since fixedUpdate appears
	// to be called more often then collisions and collisions seem NOT to happen every
	// frame!??
	bool initController = false;
	

	void Start (){
		pitchArray = new AudioClip[]{pitch1, pitch2, pitch3, pitch4, pitch5, pitch6, pitch7};

		jumpStartY = transform.position.y;
		currPitchPos = 1;
     	// Assign body parts to variables;  
		// -> could also have these as properties you set in editor
		// -> could also have used Transform.Find to only search in the children of this object
	 	head = GameObject.Find ("Head");
     	body = GameObject.Find ("Body");

		counter = 0;
		bumpTop = bumpLeft = bumpRight = bumpBottom = false;
		leftD = rightD = topD = bottomD = 0f;
		isGrounded = false;
		initController = false;

		firstInit = true; // set up first character
		currChar = 2;
		InitializeChar (2);
		firstInit = false;
		animState = idle;

		prevDirection = moveDirection.x;

		guideCharsX = guideChar.transform.localPosition.x;

		//motorCurve.preWrapMode = WrapMode.ClampForever;

				//set pitch position based on screen height
		for (int i=1; i < pitchArray.Length; i++) {
			//check our position to the current multiple of screen height
			if (transform.position.y > (i*(levelHeight/pitchArray.Length))) {
				currPitchPos = i;
			}
		}

		if(debug) {
			Debug.Log("current pitch to screen position is: " + currPitchPos);
			Debug.Log("total screen height: " + levelHeight);
			Debug.Log("single screen increment at " + (levelHeight/pitchArray.Length) + " pixels.");
		}

	}
	
	// do you application logic, managing states and so on, in here.  These examples have no explicit
	// states, but you should consider adding some to keep the code better organized
	void Update() {

		//manually stop/resume motors
		/*if (Input.GetKeyDown ("4")) {
			pauseMotors();
		}
		if (Input.GetKeyDown("5")) {
			resumeMotors();
		}*/

		//when to call motors
		/*motorTimer++;
		if (debug) Debug.Log ("motor timer is " + motorTimer + "and motorGapLength is " + motorGapLength);
		if (motorTimer < motorGapLength){
			motorStart = Time.time;
			secondMotorStart = Time.time + motorCurve.length - gapCorrection;
			if (debug) Debug.LogWarning ("setting motor one at " + motorStart + " and motor two at " + secondMotorStart);
		} else if (motorTimer>motorGapLength) {
			triggerLeftMotor();
		}*/

		for ( int i = 1; i < 3; i++ ) { // if 1 or 2 have been pressed, switch characters
			if (Input.GetKeyDown(i.ToString()) && currChar != i) {
				if ( debug ) Debug.Log( "Character changed to: " + i );
				currChar = i;
				InitializeChar(currChar);
			}
		}

	    if ( canDoubleJump && currChar == 1 ){ // allows second jump on char 1
	    	if ( Input.GetButton( "Jump" ) ) {
	    		doubleJumping = true;
	    		goalRot += 180; // increase angle to rotate to
	            moveDirection.y = jumpSpeed;
				body.transform.Rotate(new Vector3(0,0,0)); 
			} 
		}

		// we only do input if on the ground. If you want to do left/right movement in the air, you 
		// need to deal with it differently because you can't just reset the vector (you need to 
		// add the input to the vector, as you do gravity)
        if (isGrounded) { 
			if (jumpStartY != transform.position.y) jumpStartY = transform.position.y;
			//currPitchPos = 1;

            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, 0f);
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= speed;
			
			// move up off the ground by adding an upward impulse
			if ( Input.GetButton("Jump") ) {
				audio.clip = jumpUp;
				if( !audio.isPlaying) {
					audio.Play();
				}
				if (debug) Debug.Log( "first jump" );
                moveDirection.y = jumpSpeed;
				body.transform.Rotate(new Vector3(0,0,0)); 
			}
			
        } else {
        	if ( currChar == 1 ) { // special considerations for char 1
        		if ( canFirstJump ) { // can start first jump in air, since rarely on ground
	        		if ( Input.GetButton("Jump") ) {
					if ( debug ) Debug.Log( "first jump" );
	                moveDirection.y = jumpSpeed;
					body.transform.Rotate(new Vector3(0,0,0)); 
					}	
				}

				if ( animState == falling ) { // allow to change direction if falling
					if ( Input.GetAxis("Horizontal") > 0 ) moveDirection.x ++;
					else if ( Input.GetAxis("Horizontal") < 0 ) moveDirection.x --;
				}
			}

			if ( animState == jumping ) { // allow to change direction while jumping
				if ( Input.GetAxis("Horizontal") > 0 ) moveDirection.x += 0.01f;
				else if ( Input.GetAxis("Horizontal") < 0 ) moveDirection.x += 0.01f;
			}
		}
		
		if ( Input.GetButtonUp("Jump") ) { // switch between single and double jump
        	if( !canDoubleJump && canFirstJump ) { // can't first jump again, can now double
        		canDoubleJump = true; 
        		canFirstJump = false;
        	}
        	else if ( canDoubleJump ) canDoubleJump = false; // can't double jump again until first jump
        } 

		// if we've bumped the top, and are moving upwards, stop the upward movement
		// also, move down "almost" out of whatever we colided with
		if (bumpTop && moveDirection.y > 0)
		{
			moveDirection.y = 0f;
			transform.Translate(new Vector3(0f, -topD + cDelta, 0f));				
		}

		if (moveDirection.x < 0) {
			guideChar.transform.localPosition = new Vector3(-guideCharsX-charWidth, guideChar.transform.localPosition.y, guideChar.transform.localPosition.z);
		} else if (moveDirection.x > 0) {
			guideChar.transform.localPosition = new Vector3(guideCharsX, guideChar.transform.localPosition.y, guideChar.transform.localPosition.z);
		}

		// if we've bumped the left or right, and are moving in that direction, stop the movement
		// also, move "almost" out of whatever we colided with
		if (bumpLeft && moveDirection.x < 0)
		{
			HandleColliding(); // directly called for speed
			transform.Translate(new Vector3(leftD - cDelta, 0f, 0f));						
		} else if (bumpRight && moveDirection.x > 0)
		{
			HandleColliding();
			transform.Translate(new Vector3(-rightD + cDelta, 0f, 0f));							
		}

		// if we are moving up (jumping) do silly things with the head object.  If we are moving down,
		// do something different, yet also silly.  When we are walking, do something different, yet
		// just as silly
		if (isGrounded) 
		{ 
			if( debug ) Debug.Log ("GROUNDED");
			animState = walking;
			jumpStartY = transform.position.y;
		} else {
			if( debug ) Debug.Log ("NOT GROUNDED");
			if(moveDirection.y > 1) {
				animState = jumping;
			} else if(moveDirection.y < -0.35) {
				animState = falling;
				// note if direction suddenly changes while falling
				if ( prevDirection == 0 && moveDirection.x != 0 ) {
					goalRot += 180;
					directionChange = true;
				}
		    } 
		}

		// if we're on the ground, and are "inside" whatever we are on, move "almost" out.  If we are 
		// in the air, apply some gravity
		if (isGrounded) 
		{
			// if below ground
			/*if (bottomD > 0)
			{
				transform.Translate(new Vector3(0f, bottomD - cDelta, 0f));				
			}*/
		} else {
			moveDirection.y -= gravity * Time.deltaTime;
		} 

		if ( moveDirection.x == 0 && moveDirection.y == 0 ) { // set to idle if not moving
			animState = idle;
		}


		//check prev state to see if falling off platform
		/*if (prevState == idle || prevState == walking) {
			if(animState == falling) {
				//set values for scale to play
				currPitchPos = 6;
				jumpStartY = transform.position.y-23;
			}
		}*/
		prevState = animState;


		switch ( animState ) { // determine which method to call
			case idle:		 HandleIdle();			break;
			case walking:	 HandleWalking();		break;
			case jumping:	 HandleJumping();		break;
			case falling:	 HandleFalling();		break;
		}

		// after all the movement is computed ... move!
		transform.Translate(moveDirection * Time.deltaTime);
		counter++;
		// keep track of previous direction to detect sudden changes
		prevDirection = moveDirection.x;
    }


	//------------------------------------------------------------------------------------------------//
	// motor pattern
	//------------------------------------------------------------------------------------------------//
	public void triggerEdgeMotor() {
		Debug.Log("triggerEdgeMotor() running " + (Time.time-motorStart));
		float motorVal = motorEnterCurve.Evaluate(Time.time-motorStart);
		GamePad.SetVibration(PlayerIndex.One, motorVal/2, motorVal);
		motorsOn = true;
	}

	public void stopMotors() {
		Debug.Log("stopMotors() running " + (Time.time-motorStop));
		if (motorsOn) {
			float motorVal = motorExitCurve.Evaluate(Time.time-motorStop);
			GamePad.SetVibration(PlayerIndex.One, 0,0);
		}

		if ((Time.time - motorStop) > motorExitCurve.length) {
			motorsOn = false;
		}
	}
	
    void FixedUpdate() {
		initController = true;
	}
	
	void InitController() {
		// reinitialize for checking for collisions.  FixedUpdate is called BEFORE any collisions.
		bumpTop = bumpLeft = bumpRight = bumpBottom = false;
		leftD = rightD = topD = bottomD = 0f;
		isGrounded = false;
		initController = false;
		if( debug ) Debug.Log ("Init Controller");
	}

	// a simple function that sets the left/right/top/bottom based on a single collision contact point.
	// the function also returns a boolean, indicating if we are "grounded", so that we can call the 
	// function from collisionStay as well as collisionEnter
	bool checkContactPoint (ContactPoint c)
	{			
		float dotUp = Vector3.Dot (c.normal, Vector3.up);
		float dotLeft = Vector3.Dot (c.normal, Vector3.left);
		Vector3 pt = transform.InverseTransformPoint(c.point);
		float ydiff = cHeight - Mathf.Abs (pt.y); 
		float xdiff = cWidth - Mathf.Abs (pt.x);
				
		//if( debug ) Debug.Log ("dots: " + dotUp + " " + dotLeft);
		if (dotUp < -0.5) {
			if (ydiff > topD) 
				topD = ydiff;
			bumpTop = true;
			if( debug ) Debug.Log ("Bumped with Top");
		}
		else if (dotUp > 0.5)
		{
			if (ydiff > bottomD)
				bottomD = ydiff;
			bumpBottom = true;
			if( debug ) Debug.Log ("Bumped with Bottom");
		}
		
		if (dotLeft > 0.5) 
		{
			if (xdiff > rightD)
				rightD = xdiff;
			bumpRight = true;
			if( debug ) Debug.Log ("Bumped with Right");
		}
		else if (dotLeft < -0.5)
		{
			if (xdiff > leftD)
				leftD = xdiff;
			bumpLeft = true;
			if( debug ) Debug.Log ("Bumped with Left");
		}

		if ( bumpRight || bumpLeft || bumpTop ) {
			audio.clip = collideSound;
	 		if( !audio.isPlaying) {
	 			if( debug ) Debug.Log("play collision");
	 			audio.Play();
			}
		}
		
		// return if it's hit the bottom so we can check for grounded below
		return (dotUp > 0.5);
	}
	
	// Collision handling.  Update global variables for use in state machines.
	// DO NOT do any of the application logic associated with states here.  Just compute the 
	// various results of collisions, so that they can be used in Update once all the collisions 
	// are processed
	void OnCollisionEnter (Collision collision) {
        //Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
        //Vector3 pos = contact.point;

		// see if this is the first time this is called for this loop through the 
		// collision routines
		if (initController) InitController();

		foreach (ContactPoint c in collision.contacts) {
            if( debug ) Debug.Log(c.thisCollider.name + " COLLIDES WITH " + c.otherCollider.name);
            if( debug ) Debug.Log("Collision: " + transform.InverseTransformPoint(c.point) + ", Normal: " + c.normal);
    		if (checkContactPoint(c))
			{
				audio.PlayOneShot( landSound ); // play sound for char hitting ground
				isGrounded = true;			
				if( debug ) Debug.Log ("Collision Enter GROUNDED");
			}
		}

		if (collision.gameObject.tag == "platform") {
			if (transform.localPosition.y > collision.transform.position.y) {
				audio.PlayOneShot(platformLand);
			}
		}

		if (collision.gameObject.tag == "ghostPlatform") {
			if (transform.localPosition.y > collision.transform.position.y) {
				audio.PlayOneShot(ghostPlatformLand);
			}
		}
	}

	void OnCollisionStay (Collision collision) {
		// see if this is the first time this is called for this loop through the 
		// collision routines
		if (initController) InitController();

		foreach (ContactPoint c in collision.contacts) {
            if( debug ) Debug.Log(c.thisCollider.name + " STAY ON " + c.otherCollider.name);
            if( debug ) Debug.Log("Collision: " + transform.InverseTransformPoint(c.point) + ", Normal: " + c.normal);
    		if (checkContactPoint(c))
			{
				canFirstJump = true; // reset initial jump
				canDoubleJump = false; // reset double jump
				doubleJumping = false;
				isGrounded = true;			
				if( debug ) Debug.Log ("Collision Stay GROUNDED");
			}
        }
	}
	
	void OnCollisionExit () {
		// see if this is the first time this is called for this loop through the 
		// collision routines
		if (initController) InitController();

		if( debug ) Debug.Log ("Collision Exit");
		
		// technically, there could be multiple simultaneous collisions (e.g., in a corner), so we should 
		// keep track of which ones are ending here
		isGrounded = false;
	}

	void InitializeChar (int i) {
		if ( i == 1 ) {
			if( debug ) Debug.Log("change to 1"); // set lighter sound and material
			landSound = landLight;
			collideSound = collideLight;
			head.renderer.material = lightMTRL;
			body.renderer.material = lightMTRL;
			body.transform.localScale = new Vector3 (3,5,3);

			if(!firstInit) { // increase speed
				speed = speed * 2;
				jumpSpeed = jumpSpeed * 2;
				gravity = (int)((gravity -5) * 0.5);
			}
		} else if ( i == 2 ) {
			if( debug ) Debug.Log("change to 2"); // set heavier sound and material
			landSound = landHeavy;
			collideSound = collideHeavy;
			head.renderer.material = heavyMTRL;
			body.renderer.material = heavyMTRL;
			body.transform.localScale = new Vector3 (5,5,3); // widen character
			body.transform.localPosition = new Vector3(0, -1, 0); // lower body and head
			head.transform.localPosition = new Vector3(0, -1, 0);

			//speed = (int)(speed * 0.5); // decrease speed
			//jumpSpeed = (int)(jumpSpeed * 0.9);
			gravity = (gravity * 1.5f);
		}
	}


	//------------------------------------------------------------------------------------------------//
	// state handling
	//------------------------------------------------------------------------------------------------//
	void HandleIdle () {
		// straighten out characters
		Quaternion goalRotationAngle = Quaternion.Euler(0, 0, 0);
		body.transform.rotation = Quaternion.Lerp(body.transform.rotation, goalRotationAngle, (speed * Time.deltaTime));

		// character 1's idle
		if ( currChar == 1 ) {
			moveDirection.y = (int)(jumpSpeed * 0.1); // lighter char is always moving
		}
		// character 2's idle
		else if ( currChar == 2 ) {
			// move head back
			head.transform.localPosition = Vector3.Lerp(body.transform.localPosition, new Vector3(0,-1,0), (speed*Time.deltaTime));
		}
	}

	void HandleWalking () {
		// character 1's walk
		if ( currChar == 1 ) {
			// steatch out and lean when walking	
	 		body.transform.localScale = Vector3.Lerp(body.transform.localScale, new Vector3(2,6,3), (speed*Time.deltaTime));
	 		head.transform.localPosition = Vector3.Lerp(body.transform.localPosition, new Vector3(0,2,0), (speed*Time.deltaTime));
		}
		// character 2's walk
		else if ( currChar == 2 ) {
			head.transform.localPosition = new Vector3 (0F,-1.5F,0F); // lower head
			if ( moveDirection.x > 0 ) {  // scrape particles and noise
				scrapeParticles.transform.localPosition = new Vector3 (-3F, -4F, 0F); // emit sparks/particles behind char	
		 		scrapeParticles.Emit(3);

		 		audio.clip = groundScrape; // set our audio clip so we can check if playing
		 		if( !audio.isPlaying) {
		 			if ( debug ) Debug.Log("play scrape");
		 			audio.Play();
		 		}
			} else if ( moveDirection.x <0 ) {
				scrapeParticles.transform.localPosition = new Vector3 (3F, -3.5F, 0F); // emit sparks/particles behind char
				scrapeParticles.Emit(3);

				audio.clip = groundScrape; // scrape audio
		 		if( !audio.isPlaying) {
		 			if ( debug ) Debug.Log("play scrape");
		 			audio.Play();
				}
			}
			else {
		 		
			}
		}
	}

	void HandleJumping () {
		// Jumping
		PlayHeightScale (true);

		//pauseMotors();

		if ( currChar == 1 ) {
			// bounce on jumps
			body.transform.localScale = Vector3.Lerp(body.transform.localScale, new Vector3(3,5,3), (speed*Time.deltaTime));
			body.transform.localPosition = Vector3.Lerp(body.transform.localPosition, new Vector3(0,1,0), (speed*Time.deltaTime));
			Vector3 newHeadPosition = head.transform.localPosition;
			newHeadPosition.y ++;
			head.transform.localPosition = Vector3.Lerp(body.transform.localPosition, newHeadPosition, (speed*Time.deltaTime));
		}
		else if ( currChar == 2 ) {
			// no deformation, heavier
		}

		if ( doubleJumping ) { // if either char is in double jump
			if ( debug ) Debug.Log("doubleJumping");
			// make the body do a little flip
			Quaternion goalRotationAngle = Quaternion.Euler(0, 0, goalRot); // set our goal rotation
			body.transform.rotation = Quaternion.Lerp(body.transform.rotation, goalRotationAngle, (speed * Time.deltaTime));
		}
	}

	void HandleFalling () {
		// Falling

		PlayHeightScale (false);

		if ( currChar == 1 ) {
			// squish down
			body.transform.localScale = Vector3.Lerp(body.transform.localScale, new Vector3(5,4,3), (speed*Time.deltaTime));
			head.transform.localPosition = Vector3.Lerp(body.transform.localPosition, new Vector3(0,0,0), (speed*Time.deltaTime));
			body.transform.localPosition = Vector3.Lerp(body.transform.localPosition, new Vector3(0,-1.5f,0), (speed*Time.deltaTime));

			if ( directionChange ) {
				// if direction suddenly changes, do a little flip
				Quaternion goalRotationAngle = Quaternion.Euler(0, 0, goalRot); // set our goal rotation
				body.transform.rotation = Quaternion.Lerp(body.transform.rotation, goalRotationAngle, (speed * Time.deltaTime));
			}
		}
		else if ( currChar == 2 ) {
			// falls faster due to heavier gravity set on InitializeChar()
		}
	}

	void HandleColliding () {
		// for side collisions
		if ( debug ) Debug.Log("colliding");
		if ( currChar == 1 ) { // change direction on collision
			moveDirection.x = -moveDirection.x;		
		}
		else if ( currChar == 2 ) {
			// emit sparks/particles along wall
			if ( bumpRight ) { 
				scrapeParticles.transform.localPosition = new Vector3 (2F, 0F, 0F);	
		 		scrapeParticles.Emit(3);
		 	}
		 	else if ( bumpLeft ) {
		 		scrapeParticles.transform.localPosition = new Vector3 (-2F, 0F, 0F);	
		 		scrapeParticles.Emit(3);
		 	}
		}
	}

	void PlayHeightScale(bool rising) {
		if(rising) {
			if(debug)Debug.Log("rising to pitch position " + currPitchPos + " measures us at screen height " + ((currPitchPos)*(levelHeight/pitchArray.Length)));
			if (transform.position.y > (currPitchPos)*(levelHeight/pitchArray.Length)) {
				audio.PlayOneShot((AudioClip)pitchArray[currPitchPos], 1.0f);
				currPitchPos++;
			}

		} else {
			if(debug)Debug.Log("falling from pitch position " + currPitchPos);
			if (transform.position.y < (currPitchPos)*(levelHeight/pitchArray.Length)) {
				currPitchPos--;
				audio.PlayOneShot((AudioClip)pitchArray[currPitchPos], 1.0f);
			}
		}

	}

}