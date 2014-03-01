using UnityEngine;
using System.Collections;

public class ghostTree : MonoBehaviour {

	public AudioClip passingThrough;
	public AnimationCurve volumeFade;

	private bool triggerFade = false;
	private float fadeStart;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		if (triggerFade) FadeVolume();
	
	}

	void OnTriggerStay(Collider collide) {
		if (collide.gameObject.tag == "Player") {
			Debug.Log("in tree");
			triggerFade = false;
			audio.volume=1f;
			audio.clip = passingThrough;
			if( !audio.isPlaying) {
				audio.Play();
			}
		}
	}

	void OnTriggerExit(Collider collide) {
		/*for(float i = 1.0f; i > 0f; i = i- 1.0f) {
			audio.volume=i;
		}*/
		//triggerFade = true;
		fadeStart = Time.time;
		//audio.Stop();
		//audio.volume = 0f;
	}

	void FadeVolume() {
		audio.volume = volumeFade.Evaluate(Time.time - fadeStart);
	}


}
