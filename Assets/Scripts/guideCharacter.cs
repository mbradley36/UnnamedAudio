using UnityEngine;
using System.Collections;

public class guideCharacter : MonoBehaviour {

	public GameObject player;
	private AvatarScript2 playerScript;
	public AudioClip barkAlert;

	// Use this for initialization
	void Start () {
		playerScript = (AvatarScript2)player.GetComponent ("AvatarScript2");
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnCollisionEnter(Collision collision){
		if (collision.gameObject.tag == "platform") {
			//playerScript.pauseMotors();
		}
	}

	void OnTriggerEnter(Collider collide){
		if (collide.gameObject.tag == "fallingEdge") {
			//Debug.Log("player y: " + player.transform.localPosition.y + "platform y: " + collide.gameObject.transform.position.y);
			if(player.transform.localPosition.y < collide.gameObject.transform.position.y) {
				if(!audio.isPlaying)audio.PlayOneShot(barkAlert, 1.0f);
			} else {
				//if we're standing on a platform near the edge, start motors
				if(playerScript.animState!=2 && playerScript.animState !=3) {
					Debug.Log("triggerEdgeMotor");
					playerScript.motorStart = Time.time;
					playerScript.triggerEdgeMotor();
				}
			}
		}
	}

	void OnTriggerStay(Collider collide) {
		if (collide.gameObject.tag == "fallingEdge") {
			if(player.transform.localPosition.y > collide.gameObject.transform.position.y) {
				//if we're standing on a platform near the edge, start motors
				if(playerScript.animState!=2 && playerScript.animState !=3) {
					Debug.Log("continuingEdgeMotor");
					playerScript.triggerEdgeMotor();
				}
			}
		}
	}

	void OnTriggerExit(Collider collide) {
		if (collide.gameObject.tag == "fallingEdge") {
			if(player.transform.localPosition.y > collide.gameObject.transform.position.y) {
				//if(playerScript.animState!=2 && playerScript.animState !=3) {
					Debug.Log("stopMotors");
					//playerScript.motorStop = Time.time;
					playerScript.stopMotors();
				//}
			}
		}
	}

}
