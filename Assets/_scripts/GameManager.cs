using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	// game stats and game flow triggers
	public static int score = 0;			// player score
	public static int pointsTillBoss = 50;	// score that activates boss fight
	public static int bossesDefeated = 0;	// number of bosses killed throughout gameplay
	public static Vector3 screenBounds;		// screen resolution (sets player movement bounds)
	public static bool bossAlive = false;	// check to control boss gameplay
	public static bool isAlive = false;		// check to control pre or post game flow
	public static bool isDead = false;		// check to control pre or post game flow

	// UI title screen elements
	public UnityEngine.UI.Text scoreText;
	public UnityEngine.UI.Text titleText;
	public UnityEngine.UI.Image screenOverlay;

	// UI option menu elements
	public UnityEngine.UI.Image optionsOverlay;
	public UnityEngine.UI.Text optionsText;
	public UnityEngine.UI.Slider optionsSlider1;
	public UnityEngine.UI.Slider optionsSlider2;
	private bool optionsMenu = false;				// check if options menu is onscreen

	// spawnable gameobjects
	public GameObject player;			// spawn player when level loads; see player script for behavior
	public GameObject boss;				// spawn boss when player reaches
	public GameObject asteroid;			// see asteroid script for behavior
	public GameObject pointLight;		// respawn light when level loads

	// sounds plus play switches (checked in Update; accessed from other scripts)
	public AudioClip audioHurt;
	public static bool playHurt;
	public AudioClip audioShot;
	public static bool playShot;
	
	// asteroid tracking and spawning variables
	public static int asteroidCount = 0;	// current number of asteroids on screen
	public static int maxAsteroids = 4;		// maximum number of asteroids on screen
	public float asteroidSpawnRate = 0.9f;	// how fast to spawn asteroids in a row
	private float asteroidSpawnWait = 0f;	// counter for comparing time between asteroid spawns;
	

	void Awake () {
		// overwrite zero gamespace to avoid nullifying player movement at start
		screenBounds = new Vector3 (4f, 4.3f, 3f);

		// Default title text
		titleText.text = "SNAZZTEROIDS\n\n\n\nG to get you going\n\nO to set you up";

		// hide options menu
		ToggleOptionsMenu (false);
	}

	
	void Update (){
		// pregame
		if (isAlive == false && isDead == false) {
			// start game on player input
			if (Input.GetKeyDown(KeyCode.G) && optionsMenu == false) {
				StartCoroutine ("StartGame");
			}
			// toggle options menu on player input
			else if (Input.GetKeyDown (KeyCode.O)) {
				if (optionsMenu == true) {
					// hide options menu
					optionsMenu = false;
					ToggleOptionsMenu(false);
				} else {
					// bring up options menu
					optionsMenu = true;
					ToggleOptionsMenu (true);
				}
			}
			// escape from open UI menus
			else if (Input.GetKeyDown(KeyCode.Escape) && optionsMenu == true) {
				// remove options menu during coroutine
				optionsMenu = false;
			}
			// do while UI options menu is active
			else if (optionsMenu == true) {
				// track options text and values based on player input
				optionsText.text = "OPTIONS\n\nPlayer speed: "+optionsSlider1.value+"\n\n\nRox at once: "+optionsSlider2.value;
				ScriptPlayer.speed = optionsSlider1.value;
				maxAsteroids = (int)optionsSlider2.value;
			}
			// hide options menu
			else if (optionsMenu == false) {
				ToggleOptionsMenu (false);
			}
		}

		// main game
		if (isAlive == true && isDead == false) {
			SpawnAsteroids ();
			scoreText.text = "Score: " + score + " / " + pointsTillBoss;
			SpawnBoss();
		}
		PlaySFX ();		// play any sounds that have been triggered

		// post game
		if (isDead == true && isAlive == true) {
			StartCoroutine ("EndGame");
		}
		// reset game
		else if (isDead == true && isAlive == false) {
			titleText.text = "REPLAY? \n press G";
			if (Input.GetKeyDown (KeyCode.G)) {
				// reset game vars and reload level
				score = 0;
				Application.LoadLevel (Application.loadedLevel);
				isDead = false;
			}
		}

	}


	/**
	 *	Turn options menu on or off
	 */
	void ToggleOptionsMenu (bool isActive) {
		optionsOverlay.enabled = isActive;
		optionsText.enabled = isActive;
		Vector3 scale;
		/** Slider Visibility:
		 *  	Scale input sliders up/down if menu is on/off
		 */
		if (isActive == false) {
			scale = new Vector3(0f, 0f, 0f);
		} else {
			scale = new Vector3(1f, 1f, 1f);
		}
		optionsSlider1.transform.localScale = scale;
		optionsSlider2.transform.localScale = scale;
	}


	/** 
	 * 	Manage pre-game screen and immediate game start actions
	 */
	IEnumerator StartGame () {
		// time and fade intro screen elements
		scoreText.CrossFadeAlpha (0f, 1f, false);

		screenOverlay.CrossFadeAlpha (0f, 1f, false);
		titleText.CrossFadeAlpha (0f, 1f, false);

		isAlive = true;
		isDead = false;

		yield return new WaitForSeconds(0.5f);

		scoreText.CrossFadeAlpha (1f, 1f, false);

		// spawn player and access Update main gameplay branch
		Instantiate (player, this.transform.position, Quaternion.identity);
		Instantiate (pointLight, this.transform.position, Quaternion.identity);
	}
	

	/**
	 * 	Win/loss screen fades, score text and game reset
	 */
	IEnumerator EndGame () {
		// show endgame stats
		titleText.text = "ASTEROIDS BLASTED\n" + score + "\n\nBOSSES WHACKED\n" + bossesDefeated;
		titleText.CrossFadeAlpha (1f, 2f, false);
		screenOverlay.CrossFadeAlpha (1f, 2f, false);

		// wait while displaying endgame text
		yield return new WaitForSeconds (6f);

		// change to replay state
		isAlive = false;
	}


	/**
	 * 	Manage asteroid spawns using totals and times between spawns
	 */
	private void SpawnAsteroids () {
		/** If not all asteroids are spawned and it's been long enough since last asteroid:
		 * 		-  Increment the asteroid counter
		 * 		-  Reset the time since last spawn
		 * 		-  Spawn one asteroid just upwards of screen boundary y, but at any point along x
		 */
		asteroidSpawnWait += Time.deltaTime;
		if (asteroidCount < maxAsteroids && asteroidSpawnWait >= asteroidSpawnRate) {
			asteroidCount ++;
			asteroidSpawnWait = 0f;
			Instantiate (asteroid, new Vector3(Random.Range(-screenBounds.x, screenBounds.x), screenBounds.y+2f, 0f), Quaternion.identity);
		}
	}

	/**
	 * 	Spawn boss and reset boss activation conditions
	 */
	private void SpawnBoss () {
		if (score >= pointsTillBoss) {
			Instantiate (boss);
			pointsTillBoss += 30;
		}
	}

	
	/**
	 * 	Browse sfx switches and play clips that have been switched on
	 */
	private void PlaySFX() {
		/**
		 *  Access boolean switches and associated audio clips.
		 * 	If an audio switch has flipped (from other scripts):
		 *		-  Play that file this frame
		 *		-  Flip the switch back for future playbacks
		 */
		if (playHurt == true) {
			GetComponent<AudioSource>().clip = audioHurt;
			GetComponent<AudioSource>().Play();
			playHurt = false;
		} else if (playShot == true) {
			GetComponent<AudioSource>().clip = audioShot;
			GetComponent<AudioSource>().Play();
			playShot = false;
		}
	}
	
}
