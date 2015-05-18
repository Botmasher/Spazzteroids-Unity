using UnityEngine;
using System.Collections;

public class ScriptExplosion : MonoBehaviour {


	void Start () {
		StartCoroutine ("WaitDie");
	}


	IEnumerator WaitDie(){
		// give particle fx and sfx time to play, then destroy self
		yield return new WaitForSeconds(2.3f);
		Destroy (this.gameObject);
	}

}
