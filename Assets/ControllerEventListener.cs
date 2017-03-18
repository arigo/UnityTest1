using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class ControllerEventListener : MonoBehaviour {

    public GameObject paintDrop;
    public GameObject paintDropCanvas;

    float nextDropTime = -1;

    private void Start()
    {
        if (GetComponent<VRTK_ControllerEvents>() == null)
        {
            Debug.LogError("VRTK_ControllerEvents_ListenerExample is required to be attached to a Controller that has the VRTK_ControllerEvents script attached to it");
            return;
        }

        //Setup controller event listeners
        GetComponent<VRTK_ControllerEvents>().TriggerPressed += new ControllerInteractionEventHandler(DoTriggerPressed);
        GetComponent<VRTK_ControllerEvents>().TriggerReleased += new ControllerInteractionEventHandler(DoTriggerReleased);
    }

    private void DebugLogger(uint index, string button, string action, ControllerInteractionEventArgs e)
    {
        Debug.Log("Controller on index '" + index + "' " + button + " has been " + action
                + " with a pressure of " + e.buttonPressure + " / trackpad axis at: " + e.touchpadAxis + " (" + e.touchpadAngle + " degrees)");
    }

    private void DoTriggerPressed(object sender, ControllerInteractionEventArgs e)
    {
        nextDropTime = Time.time;
        //DebugLogger(e.controllerIndex, "TRIGGER", "pressed", e);
    }

    private void DoTriggerReleased(object sender, ControllerInteractionEventArgs e)
    {
        nextDropTime = -1;
        //DebugLogger(e.controllerIndex, "TRIGGER", "released", e);
    }

    private void Update()
    {
        if (nextDropTime >= 0 && nextDropTime <= Time.time)
        {
            Transform t = GetComponent<Transform>().transform;
            GameObject drop = Instantiate(paintDrop);
            drop.GetComponent<Transform>().position = t.position;
            nextDropTime += 0.1f;
        }
    }
}
