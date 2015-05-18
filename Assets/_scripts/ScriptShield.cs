using UnityEngine;
using System.Collections;

public class ScriptShield : MonoBehaviour {

	// components on this object
	private MeshRenderer thisMesh;
	private BoxCollider thisCollider;
	
	private float timeUp = 6.2f;				// how long to exist unpickedup before disappearing

	// checker for pickupmanager script
	public static bool shieldIsActive;

	// follow player if picked up
	private bool snaggedByPlayer;
	private Collider player;


	void Awake () {
		shieldIsActive = true;
		snaggedByPlayer = false;
		StartCoroutine ("Disappear");
	}

	void Update () {
		if (snaggedByPlayer == true) {
			// follow player around
			this.gameObject.transform.position = player.transform.position;
		} else {
			// wait to be picked up
		}
	}

	/**
	 * 	Wait for pickup, then destroy unpickedup shield
	 */
	IEnumerator Disappear () {
		yield return new WaitForSeconds (timeUp);
		shieldIsActive = false;
		Destroy (this.gameObject);
	}


	/**
	 * 	Stop self-destruct if picked up
	 */
	void OnTriggerEnter (Collider other) {
		if (other.gameObject.tag == "Player") {
			StopCoroutine("Disappear");
		}
	}


	/**
	 * 	Check collisions once shield spawned
	 */
	void OnTriggerStay (Collider other) {
		// Shield follows player (see asteroid script for impact effect)	
		if (this.gameObject.tag == "Shield" && other.gameObject.tag == "Player") {
			snaggedByPlayer = true;
			player = other;
		}

	}

}