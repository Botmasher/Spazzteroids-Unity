using UnityEngine;
using System.Collections;

public class ScriptAsteroid : MonoBehaviour {

	// Inspector vars
	public static float speed = 2f;			// asteroid movement speed
	public GameObject explosion;			// particle fx prefab to instantiate on bullet or player contact

	private float asteroidRotationSpeed = 0.1f;		// starting rotation speed factor for random rotate effect

	void Awake() {
		// set default speed if wakes with none
		if (speed == 0f) { speed = 2f; }

		// start with random rotation (adjust speed through Asteroid Rotation Speed var above
		this.gameObject.GetComponent<Rigidbody>().angularVelocity = Random.insideUnitSphere * asteroidRotationSpeed;
	}


	void Update () {
		// move downscreen
		transform.Translate(Vector3.down * speed * Time.deltaTime);

		// if reaches position below gamescreen, destroy it
		if (transform.position.y <= -GameManager.screenBounds.y) {
			Destroy (this.gameObject);

			// tell Player Script this asteroid is not alive
			GameManager.asteroidCount --;
		}

		/* Avoid phony z-axis collisions
		 * 		/!\ this is now handled with a deep box collider
		 * 
		// if asteroid reaches too deep or shallow along z, reel it in
		else if (transform.position.z > 1f) {
			transform.position = new Vector3 (transform.position.x, transform.position.y, 0f);
		}
		*/

	}


	/**
	 * 		Collision detection and evaluation:
	 */
	void OnTriggerEnter(Collider other) {

		// determine game consequence
		if (other.gameObject.tag == "Shield") {
			GameManager.score ++;
			ScriptShield.shieldIsActive = false;
		} else if (other.gameObject.tag == "Bullet") {
			GameManager.score ++;
		} else if (other.gameObject.tag == "Player") {
			// player gets killed and turns on death sound
			GameManager.playHurt = true;		// tell game manager to play sfx
			GameManager.isDead = true;			// access death branch of gamemanager update
		}

		// destruction sequence if collided thing is not a pickup
		if (other.gameObject.tag != "Pickup" && other.gameObject.tag != "Enemy") {
			// spawn explosion effect
			Instantiate (explosion, this.transform.position, Quaternion.identity);
			Destroy (other.gameObject);
			GameManager.asteroidCount--;
			// destroy self (inform Player, who tracks number of onscreen asteroids)
			Destroy (this.gameObject);
		}

	}


}
