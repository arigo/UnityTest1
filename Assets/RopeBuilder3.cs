using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeBuilder3 : MonoBehaviour {

    public Rigidbody firstLink;
    public GameObject handle;
    public int nbSegments = 30;
    public bool closedLoop = false;

    Rigidbody[] links;
    Vector3 org_position;

    private void Start()
    {
        links = new Rigidbody[nbSegments];
        links[0] = firstLink;
        firstLink.isKinematic = true;

        Vector3 pos = firstLink.transform.localPosition;

        for (int i = 1; i < nbSegments; i++)
        {
            Rigidbody rb = Instantiate<Rigidbody>(firstLink, transform);
            pos.z += 2f;
            rb.transform.localPosition = pos;
            rb.isKinematic = false;
            rb.GetComponent<Joint>().connectedBody = links[i - 1];
            links[i] = rb;
        }

        if (closedLoop)
        {
            links[0].GetComponent<Joint>().connectedBody = links[nbSegments - 1];
        }

        org_position = handle.transform.InverseTransformPoint(firstLink.transform.position);
    }

    private void Update()
    {
        firstLink.position = handle.transform.TransformPoint(org_position);
    }
}
