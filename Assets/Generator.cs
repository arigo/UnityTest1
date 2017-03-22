using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Generator : ColoredBehaviour {

    public GameObject ball;
    public float interval = 2.0f;

    float m_nextTime;

    // Use this for initialization
    void Start () {
        kind = kind;   /* set up color */
        m_nextTime = Time.time + interval;
    }

    // Update is called once per frame
    void Update () {
        float t1 = (m_nextTime - Time.time) / interval;
        t1 = 1.0f - t1;

        Material mat = GetComponent<Renderer>().material;
        mat.SetColor("_EmissionColor", new Color(t1, t1, t1));

        if (Time.time >= m_nextTime)
        {
            Debug.Log("Adding! " + transform.position);
            m_nextTime += interval;

            GameObject b = Instantiate(ball, transform.position, Quaternion.identity,
                                       GlobalData.instance.transform);
            b.GetComponent<BallController>().kind = kind;

            if (GlobalData.instance.Interacting())
                b.GetComponent<BallController>().disabled = true;
        }
	}
}
