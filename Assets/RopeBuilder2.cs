using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeBuilder2 : MonoBehaviour {

    public Rigidbody firstLink;
    public GameObject handle;
    public int nbSegments = 30;
    public float jointForce = 10;

    Rigidbody[] links;
    Vector3 org_position;
    float global_scale;

	private void Start()
    {
        links = new Rigidbody[nbSegments];
        links[0] = firstLink;
        org_position = handle.transform.InverseTransformPoint(firstLink.transform.position);
        firstLink.isKinematic = true;

        Vector3 pos = firstLink.transform.localPosition;

        for (int i = 1; i < nbSegments; i++)
        {
            Rigidbody rb = Instantiate<Rigidbody>(firstLink, transform);
            pos.z += 1;
            rb.transform.localPosition = pos;
            rb.isKinematic = false;
            links[i] = rb;
        }

        global_scale = transform.lossyScale.magnitude * 0.501f;
    }

    private void Update()
    {
        Vector3 pos = firstLink.position = handle.transform.TransformPoint(org_position);
        Vector3 forward = handle.transform.forward;

        for (int i = 1; i < nbSegments; i++)
        {
            Rigidbody rb = links[i];
            Vector3 forward2 = rb.position - pos;
            float angle = Vector3.Angle(forward, forward2);
            forward = Vector3.Slerp(forward, forward2, 10 / angle);
            forward.Normalize();
            pos += forward * global_scale;
            rb.transform.position = pos;
        }
    }
}
