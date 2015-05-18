using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PickupManager : MonoBehaviour {

	// game objects used to clearly display pickup effect
	public UnityEngine.UI.Text titleText;		// temporarily show effect text
	public AudioSource cameraAudio;				// tell camera to change music during effects
	public AudioClip gameMusic;					// normal bg music
	public AudioClip effectsMusic;				// pickup fx music
	public GameObject backgroundScreen;			// bg to move onscreen during effects

	// pickup components (toggled to sim drop into / remove from world)
	private MeshRenderer thisMesh;
	private BoxCollider thisCollider;

	// random time between pickup spawns
	private float spawnRate; 
	private float timeSinceSpawn = 0f;

	// shield spawn variables
	public GameObject shield;
	private float shieldSpawnRate;
	private float timeSinceShield = 0f;

	// pickup flow and choice variables
	private bool pickupIsActive = false;
	private bool firstPickup = true;	// determines effect of player's first pickup (affordance: pickups are helpful)
	private List<Pickup> pickups = new List<Pickup>();	// list of pickups with effects, text displays, music, ...
	private Pickup thisPickup;

	// create pickup instances to display and use specific effects (currently shield)
	class Pickup {
		private int effect;				// index of this effect to set its behavior
		private string text;			// effect text to display to player on pickup

		public Pickup (int pickupType, string pickupText) {
			this.effect = pickupType;
			this.text = pickupText;
		}

		// change gameplay effect, display effect text, play effect music
		public void BehaviorStart (UnityEngine.UI.Text textBox, AudioSource audioPlayer, AudioClip fxMusic) {
			textBox.text = text;
			if (this.effect == 0) {
				ScriptAsteroid.speed *= 5f;
			} else if (this.effect == 1) {
				ScriptPlayer.speed *= 4f;
			} else {
				ScriptPlayer.fireRate /= 4f;
			}
			if (GameManager.bossAlive == false) {
				audioPlayer.clip = fxMusic;
				audioPlayer.Play ();
			}
		}

		// reset gameplay effect, display old text, stop effect music
		public void BehaviorEnd (UnityEngine.UI.Text textBox, AudioSource audioPlayer, AudioClip gameMusic) {
			textBox.text = "";
			if (this.effect == 0) {
				ScriptAsteroid.speed /= 5f;
			} else if (this.effect == 1) {
				ScriptPlayer.speed /= 4f;
			} else {
				ScriptPlayer.fireRate *= 4f;
			}
			if (GameManager.bossAlive == false) {
				audioPlayer.clip = gameMusic;
				audioPlayer.Play ();
			}
		}

		public int Effect {
			get { return effect; }
			set { effect = value; }
		}

		public string Text {
			get { return text; }
			set { text = value; }
		}
	}

	
	void Awake () {
		// set when shows up
		ResetSpawnRate ();

		// fill list of pickup effects to select randomly on player pickup
		pickups.Add(new Pickup (0, "FASTEROIDS!"));
		pickups.Add(new Pickup (1, "SPEEDYALATER!"));
		pickups.Add(new Pickup (2, "FIRE UPON!"));

		// separate shield spawn rate (treated as separate game item from random pickups)
		shieldSpawnRate = Random.Range (7f, 34f);

		// grab pickup components and turn them off (simulate pickup not yet in world)
		thisMesh = this.gameObject.GetComponent<MeshRenderer>();
		thisCollider = this.gameObject.GetComponent<BoxCollider>();
		thisMesh.enabled = false;
		thisCollider.enabled = false;
	}


	void Update () {
		// instantiate only if game is going 
		if (GameManager.isAlive == true) {
			SpawnPickup ();
			SpawnShield ();
		}

		// rotate to attract player attention
		transform.Rotate (Vector3.up * 30f * Time.deltaTime);
	}


	/**
	 * 	Check for object interaction with this pickup
	 */
	void OnTriggerEnter (Collider other) {
		// player picked it up
		if (other.gameObject.tag == "Player") {
			StopCoroutine("PickupDisappear");		// stop unpickedup disappear
			StartCoroutine("PickupBehavior");		// start pickup effect
		}
	}


	/** 
	 * 	Track spawnrate (time til spawn) and enabled status (spawned or not)
	 * 		-  If time to spawn and not spawned, spawn
	 * 		-  If already spawned, keep time since spawn from reaching another time to spawn
	 * 		-  If passing thru, count up to next spawn
	 */
	void SpawnPickup() {
		// spawn pickup and initiate unpickedup disappear
		if (timeSinceSpawn >= spawnRate && pickupIsActive == false) {
			StartCoroutine("PickupDisappear");
		}
		// pickup already exists in world
		else if (pickupIsActive == true) {
			timeSinceSpawn = 0f;
		}
		// wait to spawn
		else {
			timeSinceSpawn += Time.deltaTime;
		}
	}


	/**
	 * 	Spawn shield if it's time and one doesn't exist
	 */
	void SpawnShield () {
		// reached shield spawn time and there is no shield in the world
		if (timeSinceShield >= shieldSpawnRate && ScriptShield.shieldIsActive == false) {
			// spawn shield at a random position in the reasonably gettable world
			Instantiate (shield, new Vector3(Random.Range(-GameManager.screenBounds.x, GameManager.screenBounds.x), Random.Range(-GameManager.screenBounds.y, GameManager.screenBounds.y-2f), transform.position.z), Quaternion.identity);
			timeSinceShield = 0f;
			shieldSpawnRate = Random.Range (10f, 35f);
		}
		// there is already a shield in the world
		else if (ScriptShield.shieldIsActive == true) {
			timeSinceShield = 0f;
		}
		// count up to next shield spawn
		else {
			timeSinceShield += Time.deltaTime;
		}
	}


	/**
	 *  Turn pickup on, wait for player to pick it up, turn it off
	 */
	IEnumerator PickupDisappear () {
		// activate pickup in world, including render and physics components
		pickupIsActive = true;
		thisCollider.enabled = true;
		thisMesh.enabled = true;
		transform.position = new Vector3 (Random.Range(-GameManager.screenBounds.x,GameManager.screenBounds.x), Random.Range(-GameManager.screenBounds.y, GameManager.screenBounds.y-2f), this.transform.position.z);

		// give player time to reach pickup
		yield return new WaitForSeconds (6.8f);

		// deactivate pickup in world, including render and physics
		thisMesh.enabled = false;
		thisCollider.enabled = false;
		pickupIsActive = false;
		ResetSpawnRate ();
	}


	/**
	 * 	Determine non-shield random pickup impact
	 */
	IEnumerator PickupBehavior () {
		// turn off pickup box
		thisCollider.enabled = false;
		thisMesh.enabled = false;

		// determine effect of this pickup
		if (firstPickup == true) {
			firstPickup = false;
			thisPickup = pickups[2];		// first pickup is training, always beneficial
		} else {
			thisPickup = pickups[Random.Range (0,pickups.Count)];		// subsequent pickups have mixed effects
		}

		// start pickup effect
		thisPickup.BehaviorStart (titleText, cameraAudio, effectsMusic);

		// spawn crazy effect bg screen
		GameObject thisBackground = Instantiate (backgroundScreen, new Vector3(0f,0f,5f), Quaternion.Euler(0f,-180f,-180f)) as GameObject;

		// fade in text, then slowly fade out
		titleText.CrossFadeAlpha(1f,0f,false);
		titleText.CrossFadeAlpha(0f,7f,true);

		// wait for effect to play out
		yield return new WaitForSeconds (9f);

		// end pickup effect
		thisPickup.BehaviorEnd (titleText, cameraAudio, gameMusic);

		// kill crazy bg effect screen
		Destroy (thisBackground.gameObject);

		// control respawning of pickups
		pickupIsActive = false;
		ResetSpawnRate();

		yield return null;
	}


	/**
	 * 	Reset the spawn time (to count up to next active)
	 */
	void ResetSpawnRate () {
		spawnRate = Random.Range(5f,15f);
		timeSinceSpawn = 0f;
	}



}