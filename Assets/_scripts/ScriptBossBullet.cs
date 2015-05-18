using UnityEngine;
using System.Collections;

public class ScriptBossBullet : MonoBehaviour {

	public GameObject explosion;


	void Update () {
		// Remove enemy bullet from world when reach bottom of screen
		if (transform.position.y <= -GameManager.screenBounds.y) {
			Destroy (this.gameObject);
		}
	}

	/**
	 * 	Handle destruction of various objects
	 */
	void OnTriggerEnter (Collider other) {
		// interact with other gameobjects
		if (other.gameObject.tag == "Player") {
			// destroy player and tell GameManager player is dead
			Instantiate (explosion, this.transform.position, Quaternion.identity);
			Destroy (other.gameObject);
			Destroy (this.gameObject);
			GameManager.isDead = true;
		}
		// ignore hit asteroids and enemies
		else if (other.gameObject.tag == "Enemy") {
		}
		// remove player projectile, shield, ...
		else {
			Instantiate (explosion, this.transform.position, Quaternion.identity);
			Destroy (other.gameObject);
			Destroy (this.gameObject);
		}
	}

}
