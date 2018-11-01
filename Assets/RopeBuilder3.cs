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
            //rb.GetComponent<Joint>().connectedAnchor = Vector3.zero;
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

        Vector3 p0 = links[0].position;
        Vector3 p1 = Vector3.Lerp(links[0].position, links[1].position, 1f / 3f);
        int nbSteps = 4;
        float one_step = 1f / nbSteps;
        Vector3[] vertices = new Vector3[(nbSegments-1) * nbSteps + 1];
        int nbVertices = 0;
        vertices[nbVertices++] = p0;

        for (int i = 1; i < nbSegments; i++)
        {
            Vector3 p3 = links[i].position;
            Vector3 tg = (links[i < nbSegments - 1 ? i + 1 : i].position - links[i - 1].position) * (1f / 3f);
            Vector3 p2 = p3 - tg;

            for (int step = 1; step < nbSteps; step++)
            {
                float t = step * one_step;
                Vector3 q0 = Vector3.Lerp(p0, p1, t);
                Vector3 q1 = Vector3.Lerp(p1, p2, t);
                Vector3 q2 = Vector3.Lerp(p2, p3, t);
                Vector3 r0 = Vector3.Lerp(q0, q1, t);
                Vector3 r1 = Vector3.Lerp(q1, q2, t);
                Vector3 b = Vector3.Lerp(r0, r1, t);
                vertices[nbVertices++] = b;
            }
            vertices[nbVertices++] = p3;
            p0 = p3;
            p1 = p3 + tg;
        }

        LineRenderer rend = GetComponent<LineRenderer>();
        if (rend.positionCount != vertices.Length)
            rend.positionCount = vertices.Length;
        rend.SetPositions(vertices);
    }
}
