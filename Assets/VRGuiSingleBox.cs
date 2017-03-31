using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;

public class VRGuiSingleBox : MonoBehaviour {

    public GameObject baseCube;
    public Canvas canvasObject;
    public Image imageObject;
    public Text textObject;
    
    const float FADEOUT_TIME = 0.2f;
    float time_to_fade = -1;

    public void fade_out()
    {
        time_to_fade = Time.time;
    }

    private void Update()
    {
        if (time_to_fade < 0)
            return;

        float alpha = (Time.time - time_to_fade) / FADEOUT_TIME;   // 0.0 at start, 1.0 at end
        alpha = 1.0f - alpha * alpha;                              // 1.0 at start, 0.0 at end
        if (alpha <= 0)
        {
            Destroy(gameObject);
            return;
        }

        foreach (var rend in GetComponentsInChildren<Renderer>())
        {
            Color c = rend.material.color;
            c.a = alpha;
            rend.material.color = c;
        }
        foreach (var text in GetComponentsInChildren<Text>())
        {
            Color c = text.color;
            c.a = alpha;
            text.color = c;
        }
    }
}
