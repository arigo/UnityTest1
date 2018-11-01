using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;


public class PadIndex : MonoBehaviour {

    public PongPad controller;

    public uint GetPadIndex()
    {
        return VRTK_DeviceFinder.GetControllerIndex(controller.gameObject);
    }

    public void HapticPulse(float strength)
    {
        //VRTK_SDK_Bridge.HapticPulseOnIndex(GetPadIndex(), strength);
        if (Application.isPlaying)
            SteamVR_Controller.Input((int)controller.GetComponent<SteamVR_TrackedObject>().index).TriggerHapticPulse((ushort)500);
    }
}
