using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PongPad : MonoBehaviour {

    public GameObject padObjectPrefab;
    public BallScene ballScene;

    GameObject padObject;
    SteamVR_Events.Action newPosesAppliedAction;
    public Vector3 current_velocity;
    Vector3 previous_position;
    float previous_time;
    bool pad_visible;

    void Start()
    {
        padObject = Instantiate(padObjectPrefab);
        padObject.SetActive(false);
        pad_visible = false;

        padObject.GetComponent<PadIndex>().controller = this;

        newPosesAppliedAction = SteamVR_Events.NewPosesAppliedAction(OnNewPosesApplied);
        newPosesAppliedAction.enabled = true;
    }

    private void OnNewPosesApplied()
    {
        if (!isActiveAndEnabled)
        {
            if (pad_visible) {
                padObject.SetActive(false);
                ballScene.RemovePad(transform);
                pad_visible = false;
            }
            return;
        }
        if (!pad_visible)
        {
            ballScene.AddPad(transform);
            padObject.SetActive(true);
            pad_visible = true;

            current_velocity = Vector3.zero;
        }
        else
        {
            current_velocity = (transform.position - previous_position) / (Time.time - previous_time);
        }
        previous_position = transform.position;
        previous_time = Time.time;

        padObject.transform.position = transform.position;
        padObject.transform.rotation = transform.rotation;
    }

    public Vector3 GetCurrentVelocity()
    {
        return current_velocity;
    }
}
