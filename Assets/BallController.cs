using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour {

    const float maxSqrMagnitude = 75000;

	// Update is called once per frame
	void Update () {
        if (GetComponent<Transform>().position.sqrMagnitude > maxSqrMagnitude)
            Destroy(gameObject);
	}
}
