using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreText1 : MonoBehaviour {

    float m_startTime, m_prevTime, m_delay;

    public void SetText(string text, float delay)
    {
        GetComponent<TextMesh>().text = text;
        m_startTime = Time.time;
        m_prevTime = m_startTime;
        m_delay = delay;
    }

    private void Update()
    {
        Transform tr = GetComponent<Transform>();
        tr.position += Vector3.up * (Time.time - m_prevTime) * 0.7f;
        m_prevTime = Time.time;

        float t1 = (Time.time - m_startTime) / m_delay;
        GetComponent<TextMesh>().color = Color.Lerp(Color.white, Color.clear, t1);
        if (t1 >= 1)
            Destroy(gameObject);
    }
}