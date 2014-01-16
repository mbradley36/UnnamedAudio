using UnityEngine;
using System.Collections;

public class guideCharacter : MonoBehaviour {

	public GameObject player;
	private AvatarScript2 playerScript;

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

	void OnCollisionExit(Collision collision) {
		if (collision.gameObject.tag == "platform") {
			playerScript.resumeMotors();
		}
	}
}
