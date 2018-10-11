using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TriggerAngle : MonoBehaviour
{
    private void FixedUpdate()
    {
        var ctrls = BaroqueUI.Baroque.GetControllers();
        foreach (var ctrl in ctrls)
            if (ctrl.isActiveAndEnabled)
                Debug.Log("angle = " + ctrl.triggerVariablePressure);
    }
}
