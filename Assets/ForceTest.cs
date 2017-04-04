using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceTest : MonoBehaviour {

	void Start () {
		
	}
	
	void FixedUpdate () {
        GetComponent<Rigidbody>().AddForce(0, -0.02f, 0);
	}
}
