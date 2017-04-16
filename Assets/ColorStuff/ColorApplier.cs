using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class ColorApplier : MonoBehaviour {

    public Renderer colorView;

	private void Start()
    {
        GetComponent<VRTK_ControllerEvents>().TriggerPressed += ColorApplier_TriggerPressed;
    }

    private void ColorApplier_TriggerPressed(object sender, ControllerInteractionEventArgs e)
    {
        Color col = colorView.material.color;
        Collider[] colls = Physics.OverlapSphere(transform.position, 0.01f, Physics.AllLayers, QueryTriggerInteraction.Collide);
        foreach (var coll in colls)
        {
            Renderer rend = coll.GetComponent<Renderer>();
            if (rend != null)
                rend.material.color = col;
        }
    }
}
