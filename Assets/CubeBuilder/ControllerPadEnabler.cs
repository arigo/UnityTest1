using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using VRTK;


public class ControllerPadEnabler : MonoBehaviour
{
    public GameObject padObjectPrefab;

    GameObject pad_object;
    SteamVR_Events.Action newPosesAppliedAction;

    void Start()
    {
        newPosesAppliedAction = SteamVR_Events.NewPosesAppliedAction(OnNewPosesApplied);
        newPosesAppliedAction.enabled = true;
    }

    private void OnNewPosesApplied()
    {
        if (!isActiveAndEnabled)
        {
            if (pad_object != null)
            {
                Destroy(pad_object);
                pad_object = null;
            }
            return;
        }
        if (pad_object == null)
        {
            pad_object = Instantiate(padObjectPrefab);
            pad_object.GetComponent<PadToucher>().cev = GetComponent<VRTK_ControllerEvents>();
        }
        pad_object.transform.position = transform.position;
        pad_object.transform.rotation = transform.rotation;
    }
}