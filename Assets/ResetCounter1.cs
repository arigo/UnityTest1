using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class ResetCounter1 : MonoBehaviour {

    private void Start()
    {
        VRTK_InteractableObject io = GetComponent<VRTK_InteractableObject>();
        io.InteractableObjectGrabbed += Grab;
        io.InteractableObjectUngrabbed += UnGrab;
    }

    private void Grab(object o, InteractableObjectEventArgs e)
    {
        GlobalData.instance.InteractionStart();
    }

    private void UnGrab(object o, InteractableObjectEventArgs e)
    {
        GlobalData.instance.InteractionStop();
    }
}
