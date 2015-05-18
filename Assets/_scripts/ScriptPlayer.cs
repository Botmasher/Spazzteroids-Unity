using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScriptPlayer : MonoBehaviour {

	// Inspector vars
	public GameObject projectile;			// bullet prefab to instantiate

	// player speed and fire rate
	public static float speed = 5f;			// player speed multiplier for translates
	public static float fireRate = 0.6f;	// time between bullet instantiates (when mashing/holding fire input)

	// comparative calculation vars
	private Vector3 move;					// stores player movement inputs
	private float timeSinceFire = 0f;		// post-fire bullet counter for fire rate


	void Awake () {
		// Set initial movement vectors
		move = Vector3.zero;
	}

	void Update () {

		// store player's position at start of frame for out-of-bounds checking at end of frame
		Vector3 startSpot = transform.position;

		// store player input
		move.x = Input.GetAxis ("Horizontal") * speed;
		move.y = Input.GetAxis ("Vertical") * speed;
		move.z = 0f;

		// clamp inputs to avoid diagonal stacking along x/y hypotenuse
		move = Vector3.ClampMagnitude (move, speed);

		// use above to rotate player model along horizontal input
		transform.rotation = Quaternion.Euler (0f, (Input.GetAxis("Horizontal")*-46f), 0f);

		// Use above move input to move player
		transform.Translate (move * Time.deltaTime);

		/**
		 * 	Keep player movement within game world
		 * 	If either x or y is out of bounds, reset to x or y from beginning of this frame.
		 * 
		 * 	This was testing an alternative to:
		 * 	
		 * 	transform.position.x = Mathf.Clamp (transform.position.x, screenBounds.x, -screenBounds.x);
		 * 	transform.position.y = Mathf.Clamp (transform.position.y, screenBounds.y, -screenBounds.y);
		 * 
		 */
		if (Mathf.Abs(transform.position.x) >= GameManager.screenBounds.x) {
			transform.position = new Vector3 (startSpot.x, transform.position.y, startSpot.z);
		}
		if (Mathf.Abs(transform.position.y) >= GameManager.screenBounds.y) {
			transform.position = new Vector3 (transform.position.x, startSpot.y, startSpot.z);
		}
		if (Mathf.Abs(transform.position.z) >= 0.1f) {
			transform.position = new Vector3 (transform.position.x, transform.position.y, 0f);
		}

		/**
		 * 	Shoot projectiles.
		 * 		- add up time since last bullet
		 * 		- (check for input and fire rate) -> instantiate projectile prefab
		 * 	see projectile script for movement and destruction
		 */
		timeSinceFire += Time.deltaTime;
		if (Input.GetKey (KeyCode.Space) && timeSinceFire >= fireRate) {
			timeSinceFire = 0;
			Instantiate (projectile, new Vector3(transform.position.x, transform.position.y+0.5f, startSpot.z), Quaternion.identity);
		} else {
			// Do not shoot: no input or not enough time since last fire.
		}

	}

}