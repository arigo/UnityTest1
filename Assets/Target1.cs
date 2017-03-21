using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Target1 : MonoBehaviour {

    const float blinkTime = 0.8f;
    float blinkStartTime = -1;

    Color blinkColor = new Color(1, 1, 1);
    Color restColor;

    public GameObject scoreText;
    public int counter = 0;
    int interacting = 0;

    private void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        restColor = rend.material.GetColor("_Color");
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag != "Ball")
            return;

        Color ballColor = collision.gameObject.GetComponent<Renderer>().material.GetColor("_Color");
        Destroy(collision.gameObject);
        blinkStartTime = Time.time;

        Color thiscolor = GetComponent<Renderer>().material.GetColor("_Color");
        
        if (thiscolor.r != ballColor.r || thiscolor.g != ballColor.g || thiscolor.b != ballColor.b)
        {
            say(collision, ":-(");
            counter = 0;
        }
        else if (interacting == 0)
        {
            counter++;
            say(collision, "" + counter);

            Target1[] targets = GameObject.Find("Targets").GetComponentsInChildren<Target1>();
            bool all_reached = true;
            foreach (Target1 t in targets)
            {
                all_reached = all_reached & (t.counter >= 10);
            }
            if (all_reached)
            {
                int cur = SceneManager.GetActiveScene().buildIndex;
                SceneManager.LoadScene(cur + 1);
            }
        }
        else
            say(collision, "X");
    }

    private void say(Collision collision, string what)
    {
        ContactPoint contact0 = collision.contacts[0];
        GameObject txt = Instantiate(scoreText,
                                     contact0.point + 0.5f * Vector3.up,
                                     Quaternion.LookRotation(contact0.normal));
        txt.SendMessage("MsgSetText", what);
        txt.SendMessage("MsgStartAnimation", blinkTime);
    }

    void MsgInteractStart(object o)
    {
        interacting++;
    }

    void MsgInteractStop(object o)
    {
        Debug.Assert(interacting > 0);
        interacting--;
        counter = 0;
    }

    private void Update()
    {
        if (blinkStartTime >= 0)
        {
            float t1 = (Time.time - blinkStartTime) / blinkTime;

            Renderer rend = GetComponent<Renderer>();
            rend.material.SetColor("_Color", Color.Lerp(blinkColor, restColor, t1));

            if (t1 >= 1)
                blinkStartTime = -1;
        }
    }
}
