using UnityEngine;
using System.Collections;

public enum CritterState{
	Following,
	Blocked,
	Launched
}

public class FollowerCritter : MonoBehaviour {
	public bool following;
	private Vector3 critterDirection = new Vector3(0,0,0);
	public GameObject player;
	public float critterSpeed;
	public AudioClip ghostHitWall, ghostTraveling;

	private bool stuck;

	private Vector3 startPosition;
	public CritterState state;
	private bool blockedToRight;
	private int launchCount;
	public int launchDuration;

	private float destinationX, destinationY;

	float unitScaling = 1f;

	// Use this for initialization
	void Start () {
		following = true;
		startPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
		state = CritterState.Following;
		launchCount = 0;
	}
	
	// Update is called once per frame
	void Update () {

		switch(state) {
			case CritterState.Following: 	HandleFollowing();  break;
			case CritterState.Blocked: 		HandleBlocked();   	break;
			case CritterState.Launched:		HandleLaunched(); 	break;
		};


		 if (critterDirection.x != 0 && critterDirection.y != 0) {
		 		audio.clip = ghostTraveling;
				if( !audio.isPlaying) {
					audio.Play();
				}
		 }

		 critterDirection = transform.TransformDirection(critterDirection);

		 transform.Translate(critterDirection * Time.deltaTime);

	}

	void HandleFollowing() {
		SetDirectionValues(player);
	}

	void HandleBlocked() {
		//distress noises
			audio.clip = ghostHitWall;
			if( !audio.isPlaying) {
				audio.Play();
			}

			if(blockedToRight) {
				if(player.transform.position.x < transform.position.x) {
					state = CritterState.Following;
				}
			} else {
				if(player.transform.position.x > transform.position.x) {
					state = CritterState.Following;
				}
			}
	}

	void HandleLaunched(){
		Debug.Log("launching char");
		if(launchCount == 0) { //replace with curve
			destinationX = ((player.transform.position.x - transform.position.x)/unitScaling);
			destinationY = ((player.transform.position.y - transform.position.y)/unitScaling);
		} else if (launchCount == launchDuration) {
			launchCount = 0;
			state = CritterState.Following;
		}
		critterDirection.x = destinationX;
		critterDirection.y = destinationY+(float)launchCount;

		launchCount ++;
	}

	void SetDirectionValues(GameObject goalPosition){

		critterDirection.x = ((goalPosition.transform.position.x - transform.position.x)/unitScaling)*critterSpeed;
		critterDirection.y = 0;

	}

	void OnTriggerEnter(Collider collide) {
		critterDirection = new Vector3(0,0,0);
		audio.clip = ghostHitWall;
		if( !audio.isPlaying) {
			audio.Play();
		}

		state = CritterState.Blocked;
		if(transform.position.x < player.transform.position.x) {
				blockedToRight = true;
			}else {
				blockedToRight = false;
			}
	}

}
