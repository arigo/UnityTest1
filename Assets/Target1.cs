using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Target1 : MonoBehaviour {

    const float blinkTime = 0.8f;
    float blinkStartTime = -1;

    Color blinkColor = new Color(1, 1, 1);
    Color restColor;

    public GameObject scoreText;
    int counter = 0;
    List<object> interacting;

    private void Start()
    {
        Renderer rend = GetComponent<Renderer>();
        restColor = rend.material.GetColor("_Color");
        interacting = new List<object>();
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Target: OnCollisionEnter!");

        if (collision.gameObject.tag != "Ball")
            return;

        Destroy(collision.gameObject);
        blinkStartTime = Time.time;

        if (interacting.Count == 0)
        {
            counter++;
            ContactPoint contact0 = collision.contacts[0];
            GameObject txt = Instantiate(scoreText,
                                contact0.point + 0.5f * Vector3.up,
                                Quaternion.LookRotation(contact0.normal));
            txt.SendMessage("MsgSetText", counter);
            txt.SendMessage("MsgStartAnimation", blinkTime);
        }
    }

    void MsgInteractStart(object o)
    {
        interacting.Add(o);
    }

    void MsgInteractStop(object o)
    {
        interacting.Remove(o);
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
