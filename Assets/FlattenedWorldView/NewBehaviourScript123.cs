using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NewBehaviourScript123 : MonoBehaviour
{
    public Transform plane, head;

    // Update is called once per frame
    void Update()
    {
        transform.position = head.position + new Vector3(10f, 0, 0);
        transform.rotation = head.rotation;
        plane.transform.position = head.position + head.forward;
        plane.transform.rotation = head.rotation;
    }
}
