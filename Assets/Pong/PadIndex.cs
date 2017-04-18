using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;


public class PadIndex : MonoBehaviour {

    public GameObject controller;

    public uint GetPadIndex()
    {
        return VRTK_DeviceFinder.GetControllerIndex(controller);
    }

    public void HapticPulse(float strength)
    {
        VRTK_SDK_Bridge.HapticPulseOnIndex(GetPadIndex(), strength);
    }

}
