using UnityEngine;
using System.Collections;

public class ScriptProjectile : MonoBehaviour {

	// Inspector vars
	public float speed = 20f;			// projectile speed multiplier
	public float lifeTime = 10f;		// number of seconds to exist before destruction

	// private vars
	private float timeAlive = 0f;		// how long projectile has been alive (compared to life time)


	void Awake () {
		// tell game manager to play sfx
		GameManager.playShot = true;
	}


	void Update () {
		// move projectile up at constant speed
		transform.Translate (Vector3.up * speed * Time.deltaTime);

		/**
		 * 	Count how long projectile has been alive.
		 * 	Once it reaches its max lifetime, destroy it.
		 */
		timeAlive += Time.deltaTime;
		if (timeAlive >= lifeTime) {
			Destroy (this.gameObject);
		}

	}

}
