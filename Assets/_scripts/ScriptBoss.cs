using UnityEngine;
using System.Collections;

public class ScriptBoss : MonoBehaviour {

	// general health and movement
	private float speed = 1.3f;					// boss move speed
	private int health = 40;					// boss current health
	private int healthTotal;					// boss total health
	private UnityEngine.UI.Slider healthbar;	// UI health display

	// fired projectile variables
	public GameObject bullet;			// unique enemy bullet prefab dropped by gravity
	public GameObject explosion;		// same explosion used for asteroids
	private float fireRate = 0.9f;		// fire speed (tweaked randomly after first fire)
	private float fireCountup = 0f;		// incremented counter for fire-ready

	// Audio music variables
	private AudioSource cameraAudio;
	public AudioClip defaultMusic;		// default game music for return to normal
	public AudioClip bossMusic;			// boss music for atmosphere


	void Awake () {

		// tell game manager boss is alive
		GameManager.bossAlive = true;

		// begin boss music
		cameraAudio = GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<AudioSource>();
		cameraAudio.clip = bossMusic;
		cameraAudio.Play ();

		// grab UI health, display it and set boss health
		healthbar = GameObject.FindGameObjectWithTag ("Boss Health").GetComponent<UnityEngine.UI.Slider>();
		healthTotal = health;
		healthbar.transform.localScale = new Vector3 (1f,1f,1f);		// scale up slider to show on screen
		healthbar.value = healthbar.maxValue;
	}


	void Update () {
		// move into position
		if (transform.position.y >= 3f) {
			transform.Translate (Vector3.back * Time.deltaTime);

		// once in position - move, fire and take damage
		} else {

			// check if time to fire bullet
			if (fireCountup >= fireRate) {
				// Instantiate fire (falls along y with gravity)
				Instantiate (bullet, new Vector3(transform.position.x, transform.position.y-1f,0f), Quaternion.identity);
				// Reset fire
				fireRate = Random.Range (0.5f, 1f);
				fireCountup = 0f;
			} else {
				// count up to next fire
				fireCountup += Time.deltaTime;
			}

			// switch direction if reach x border
			if (Mathf.Abs (transform.position.x) >= GameManager.screenBounds.x - 1f) {
				speed = -speed;
			}
			// move along x
			transform.Translate (new Vector3 (speed * Time.deltaTime, 0f, 0f));
		}

	}


	/**
	 * 	Determine effects of collision with this boss
	 */
	void OnTriggerEnter (Collider other) {
		// decrement health when hit by projectile, then destroy hit
		if (other.gameObject.tag == "Bullet") {
			UpdateHealth ();
			Destroy (other.gameObject);
			// determine impact on boss health
			if (health <= 0) {
				StartCoroutine("KillBoss");
			} else {
				StartCoroutine("HurtBoss");
			}
		}
		// destroy player when runs into boss
		else if (other.gameObject.tag == "Player") {
			GameManager.isDead = true;
			Destroy (other.gameObject);
		}
		// hit by something other than bullet or player
		else {
		}
	}


	/**
	 * 	Simulate player projectile impact
	 */
	IEnumerator HurtBoss () {
		// time flicker for effect
		this.gameObject.GetComponent<MeshRenderer>().enabled = false;
		yield return new WaitForSeconds (0.05f);
		this.gameObject.GetComponent<MeshRenderer>().enabled = true;
		yield return null;
	}


	/**
	 * 	Spawn timed explosions around self, then destroy self
	 */
	IEnumerator KillBoss () {
		// time explosions for effect
		Instantiate (explosion, new Vector3 (transform.position.x-1f, transform.position.y, transform.position.z), Quaternion.identity);
		yield return new WaitForSeconds (0.02f);
		Instantiate (explosion, new Vector3 (transform.position.x+1f, transform.position.y, transform.position.z), Quaternion.identity);
		yield return new WaitForSeconds (0.02f);
		Instantiate (explosion, new Vector3 (transform.position.x, transform.position.y-1f, transform.position.z), Quaternion.identity);

		// return to regular game music
		cameraAudio.clip = defaultMusic;
		cameraAudio.Play ();

		// defeat boss
		healthbar.transform.localScale = new Vector3 (0f,0f,0f);	// scale down slider to remove from screen
		GameManager.bossesDefeated ++;								// tell Game Manager a new boss was defeated
		GameManager.bossAlive = false;								// tell Game Manager boss gameplay is now over
		Destroy (this.gameObject);
	}


	/**
	 * 	Damage boss and update health bar
	 */
	void UpdateHealth () {
		// decrement current health
		health--;

		// display health ratio of total health on slider bar
		healthbar.value = (float)health/(float)healthTotal * 100f;
	}

}