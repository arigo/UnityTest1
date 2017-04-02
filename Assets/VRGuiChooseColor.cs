using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VRTK;

public class VRGuiChooseColor : MonoBehaviour {

    public GameObject markerPrefab;
    GameObject marker;

    private void Start()
    {
        GetComponent<VRTK_InteractableObject>().InteractableObjectTouched += VRGuiChooseColor_InteractableObjectTouched;
    }

    private void VRGuiChooseColor_InteractableObjectTouched(object sender, InteractableObjectEventArgs e)
    {
        Transform tr = e.interactingObject.transform;
        Ray ray = new Ray(tr.position, transform.position - tr.position);
        RaycastHit hit;
        if (GetComponent<Collider>().Raycast(ray, out hit, 2.0f))
        {
            if (marker != null)
                Destroy(marker);
            marker = Instantiate(markerPrefab, hit.point, Quaternion.LookRotation(hit.normal, Vector3.up));
        }
    }
}
