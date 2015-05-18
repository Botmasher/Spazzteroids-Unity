using UnityEngine;
using System.Collections;

public class ScriptGround : MonoBehaviour {

	private float turnSpeed = 20f;

	// Update is called once per frame
	void Update () {
		this.transform.Rotate (0f,0f,1f*turnSpeed*Time.deltaTime);
	}
}
