using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;


public class PadIndex : MonoBehaviour {

    public GameObject controller;

    public void HapticPulse(float strength)
    {
        uint pad_index = VRTK_DeviceFinder.GetControllerIndex(controller);
        VRTK_SDK_Bridge.HapticPulseOnIndex(pad_index, 0.2f);
    }

}
