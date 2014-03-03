using UnityEngine;
using System.Collections;

public class FollowerCritter : MonoBehaviour {
	public bool following;
	private Vector3 critterDirection = new Vector3(0,0,0);
	public GameObject player;
	public float critterSpeed;
	public AudioClip ghostHitWall, ghostTraveling;

	private bool stuck;

	private Vector3 startPosition;

	// Use this for initialization
	void Start () {
		following = false;
		startPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
	}
	
	// Update is called once per frame
	void Update () {
	
		 if (following && !stuck) {
		 	SetDirectionValues(player);
		 	following = false;
		 } else if (following && stuck) {
		 	audio.clip = ghostHitWall;
			if( !audio.isPlaying) {
				audio.Play();
			}
			following = false;
		 }

		 if (critterDirection.x != 0 && critterDirection.y != 0) {
		 		audio.clip = ghostTraveling;
				if( !audio.isPlaying) {
					audio.Play();
				}
		 }

		 critterDirection = transform.TransformDirection(critterDirection);

		 transform.Translate(critterDirection * Time.deltaTime);

	}

	void SetDirectionValues(GameObject goalPosition){
		//Debug.Log("player x, y: " + player.transform.position.x +", "+ player.transform.position.y);
	 	//Debug.Log("critter x, y: " + transform.position.x +", "+ transform.position.y);
	 	/*float unitScaling = Mathf.Sqrt(
	 						Mathf.Pow((goalPosition.transform.position.x - transform.position.x), 2) +
	 						Mathf.Pow((goalPosition.transform.position.y - transform.position.y), 2)
							);*/

		float unitScaling = 1f;

		critterDirection.x = (goalPosition.transform.position.x - transform.position.x)/unitScaling;
		critterDirection.y = (goalPosition.transform.position.y - transform.position.y)/unitScaling;

		//Debug.Log("new vector direction: " + critterDirection.x + ", " + critterDirection.y + ", " + critterDirection.z);
	}

	void OnCollisionEnter(Collision collision) {
		critterDirection = new Vector3(0,0,0);
		audio.clip = ghostHitWall;
		if( !audio.isPlaying) {
			audio.Play();
		}

		following = false;
		stuck = true;
	}

	void OnTriggerEnter(Collider collide) {
		critterDirection = new Vector3(0,0,0);
		audio.clip = ghostHitWall;
		if( !audio.isPlaying) {
			audio.Play();
		}

		following = false;
		stuck = true;
	}

}
