using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class ResetCounter1 : MonoBehaviour {

    private void Start()
    {
        VRTK_InteractableObject io = GetComponent<VRTK_InteractableObject>();
        io.InteractableObjectUngrabbed += UnGrab;
    }

    private void UnGrab(object o, InteractableObjectEventArgs e)
    {
        GameObject.Find("Targets").BroadcastMessage("MsgResetCounter");
    }
}
