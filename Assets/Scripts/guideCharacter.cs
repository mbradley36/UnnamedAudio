using UnityEngine;
using System.Collections;

public class guideCharacter : MonoBehaviour {

	public GameObject player;
	private AvatarScript2 playerScript;
	public AudioClip whimper;

	// Use this for initialization
	void Start () {
		playerScript = (AvatarScript2)player.GetComponent ("AvatarScript2");
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnCollisionEnter(Collision collision){
		if (collision.gameObject.tag == "platform") {
			playerScript.pauseMotors();
		}
	}

	void OnTriggerEnter(Collider collide){
		if (collide.gameObject.tag == "fallingEdge") {
			//Debug.Log("player y: " + player.transform.localPosition.y + "platform y: " + collide.gameObject.transform.position.y);
			if(player.transform.localPosition.y > collide.gameObject.transform.position.y) {
				if(!audio.isPlaying)audio.PlayOneShot(whimper, 1.0f);
			}
		}
	}

	void OnCollisionExit(Collision collision) {
		if (collision.gameObject.tag == "platform") {
			playerScript.resumeMotors();
		}
	}

}
