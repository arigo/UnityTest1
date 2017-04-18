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
        VRTK_SDK_Bridge.HapticPulseOnIndex(GetPadIndex(), strength);
    }
}
