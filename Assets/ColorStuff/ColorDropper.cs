using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class ColorDropper : MonoBehaviour {

    public GameObject dropPrefab;
    public float dropFrequency = 0.1f;

    float nextDropTime = -1;

    private void Start()
    {
        GetComponent<VRTK_ControllerEvents>().TriggerPressed += DoTriggerPressed;
        GetComponent<VRTK_ControllerEvents>().TriggerReleased += DoTriggerReleased;
    }

    private void DoTriggerPressed(object sender, ControllerInteractionEventArgs e)
    {
        nextDropTime = Time.time;
    }

    private void DoTriggerReleased(object sender, ControllerInteractionEventArgs e)
    {
        nextDropTime = -1;
    }

    private void Update()
    {
        if (nextDropTime >= 0 && nextDropTime <= Time.time)
        {
            GameObject drop = Instantiate(dropPrefab);
            drop.transform.position = transform.position;
            drop.GetComponent<Renderer>().material.color = GetComponent<ColorChanger>().selected_color;
            VRTK_SDK_Bridge.HapticPulseOnIndex(VRTK_DeviceFinder.GetControllerIndex(gameObject), 0.2f);
            nextDropTime += dropFrequency;
        }
    }
}
