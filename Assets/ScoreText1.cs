using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreText1 : MonoBehaviour {

    float startTime, prevTime, delay;

    void MsgSetText(string txt)
    {
        GetComponent<TextMesh>().text = txt;
    }

    void MsgStartAnimation(float delay1)
    {
        startTime = Time.time;
        prevTime = startTime;
        delay = delay1;
    }

    private void Update()
    {
        Transform tr = GetComponent<Transform>();
        tr.position += Vector3.up * (Time.time - prevTime) * 0.7f;
        prevTime = Time.time;

        float t1 = (Time.time - startTime) / delay;
        if (t1 >= 1)
            Destroy(gameObject);
        else
            GetComponent<TextMesh>().color = Color.Lerp(Color.white, Color.clear, t1);
    }
}