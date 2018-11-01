using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeBuilder4 : MonoBehaviour {

    public int nbSegments = 150;
    public float segmentLength = 0.02f;

    Vector3[] position, velocity;

    private void Start()
    {
        position = new Vector3[nbSegments];
        velocity = new Vector3[nbSegments];
        for (int i = 0; i < nbSegments; i++)
        {
            position[i] = transform.position + transform.forward * segmentLength * i;
            velocity[i] = Vector3.zero;
        }
    }

    private void FixedUpdate()
    {
        position[0] = transform.position;

        /*for (int j = 0; j < 100; j++)
        {
            for (int i = 1; i < nbSegments; i++)
                ForceSegment(i);
            for (int i = 1; i < nbSegments; i++)
                position[i] += velocity[i];
        }*/
        

        for (int i = 1; i < nbSegments; i++)
            DragSegment(i);

        LineRenderer rend = GetComponent<LineRenderer>();
        if (rend.positionCount != position.Length)
            rend.positionCount = position.Length;
        rend.SetPositions(position);
    }

    void ApplyForce(int index, Vector3 force)
    {
        velocity[index] += force;
        velocity[index] *= 0.99999f;
    }

    void ForceSegment(int index)
    {
        Vector3 diff = position[index] - position[index - 1];
        float correction = segmentLength / diff.magnitude;
        Vector3 force = diff.normalized * (1 - correction) * 0.00001f;

        ApplyForce(index - 1, force);
        ApplyForce(index, -force);
    }

    void DragSegment(int index)
    {
        Vector3 diff = position[index] - position[index - 1];
        diff *= segmentLength / diff.magnitude;
        position[index] = position[index - 1] + diff;
    }
}
