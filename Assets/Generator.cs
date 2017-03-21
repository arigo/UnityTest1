using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour {

    public GameObject ball;
    float nextTime;
    const float interval = 2.0f;

	// Use this for initialization
	void Start () {
        nextTime = interval;
	}
	
	// Update is called once per frame
	void Update () {
        float t1 = (nextTime - Time.time) / interval;
        t1 = 1.0f - t1;
        
        Renderer rend = GetComponent<Renderer>();
        rend.material.SetColor("_EmissionColor", new Color(t1, t1, t1));

        if (Time.time < nextTime)
            return;

        Debug.Log("Adding! " + transform.position);
        Instantiate(ball, transform.position, Quaternion.identity);
        nextTime += interval;
	}
}
