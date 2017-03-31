using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;


public class VRGuiTestScript : MonoBehaviour {

    public VRTK_ControllerEvents controllerEvents;
    public GameObject singleBoxPrefab;

    string[] labels = { "Eye down", "Mouth open", "Third item", "Fourth item" };
    VRGuiSingleBox[] boxes;
    Vector3 b_origin;
    Quaternion b_rotation;
    float b_scroll = 9999, b_touchpad_prev;

    private void Start()
    {
        VRTK_ControllerEvents cev = controllerEvents;
        cev.TouchpadAxisChanged += Cev_TouchpadAxisChanged;
        cev.TouchpadTouchStart += Cev_TouchpadTouchStart;
        cev.TouchpadTouchEnd += Cev_TouchpadTouchEnd;
    }

    void RemoveAll()
    {
        if (boxes != null)
        {
            foreach (var box in boxes)
                box.fade_out();
            boxes = null;
        }
    }

    void AddBoxes()
    {
        if (boxes == null)
        {
            boxes = new VRGuiSingleBox[labels.Length];
            for (int i = 0; i < labels.Length; i++)
            {
                GameObject go = Instantiate(singleBoxPrefab);
                go.transform.rotation = b_rotation;
                boxes[i] = go.GetComponent<VRGuiSingleBox>();
                boxes[i].textObject.text = labels[i];
                go.SetActive(true);
            }
            if (b_scroll < 0.0f)
                b_scroll = 0.0f;
            float display_pos = ScrollBoxes();
            if (display_pos < 0.0f)
            {
                b_scroll += display_pos;
                ScrollBoxes();
            }
        }
    }

    float ScrollBoxes()
    {
        float display_pos = -b_scroll;

        foreach (var box in boxes)
        {
            Vector3 pos = b_origin + b_rotation * Vector3.down * display_pos;
            box.GetComponent<Transform>().position = pos;
            display_pos += 1.6667f * box.baseCube.transform.lossyScale.y;
        }
        return display_pos;
    }

    private void Cev_TouchpadTouchStart(object sender, ControllerInteractionEventArgs e)
    {
        Transform controller_tr = VRTK_DeviceFinder.GetControllerByIndex(e.controllerIndex, true).transform;
        b_origin = controller_tr.position + 0.2f * controller_tr.forward;
        b_rotation = Quaternion.LookRotation(controller_tr.forward);
        b_touchpad_prev = e.touchpadAxis.y;

        AddBoxes();
    }

    private void Cev_TouchpadTouchEnd(object sender, ControllerInteractionEventArgs e)
    {
        RemoveAll();
    }

    private void Cev_TouchpadAxisChanged(object sender, ControllerInteractionEventArgs e)
    {
        if (boxes != null)
        {
            b_scroll += (e.touchpadAxis.y - b_touchpad_prev) * -0.2f;
            b_touchpad_prev = e.touchpadAxis.y;
            ScrollBoxes();
        }
    }
}