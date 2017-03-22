using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Target1 : ColoredBehaviour {

    public GameObject scoreText;

    const float blinkTime = 0.8f;
    float m_blinkEndTime = -1;
    static Color blinkColor = new Color(1, 1, 1);

    int m_counter_index;

    int counter
    {
        get { return GlobalData.instance.GetCounter(m_counter_index); }
        set { GlobalData.instance.SetCounter(m_counter_index, value); }
    }

    private void Start()
    {
        kind = kind;   /* set up color */
        m_counter_index = GlobalData.instance.CreateCounter();
    }

    void OnCollisionEnter(Collision collision)
    {
        BallController bc = collision.gameObject.GetComponent<BallController>();
        if (bc == null)
            return;

        Destroy(collision.gameObject);
        m_blinkEndTime = Time.time + blinkTime;
        if (bc.disabled || counter < 0)
        {
            say(collision, "X");
            m_blinkEndTime -= blinkTime / 2;
            return;
        }

        if (kind != bc.kind)
        {
            say(collision, ":-(");
            counter = 0;
        }
        else if (counter >= 0)
        {
            counter++;
            say(collision, "" + counter);
        }
    }

    private void say(Collision collision, string what)
    {
        ContactPoint contact0 = collision.contacts[0];
        GameObject txt = Instantiate(scoreText,
                                     contact0.point + 0.5f * Vector3.up,
                                     Quaternion.LookRotation(contact0.normal));
        txt.GetComponent<ScoreText1>().SetText(what, blinkTime);
    }

    private void Update()
    {
        if (m_blinkEndTime >= 0)
        {
            float t1 = (m_blinkEndTime - Time.time) / blinkTime;
            SetColor(Color.Lerp(GetKindColor(), blinkColor, t1));

            if (t1 <= 0)
                m_blinkEndTime = -1;
        }
    }
}