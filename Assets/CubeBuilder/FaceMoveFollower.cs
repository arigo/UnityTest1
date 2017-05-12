using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;


public class FaceMoveFollower : MonoBehaviour
{

    public ConvexPolyhedronFace face;
    public VRTK_ControllerEvents cev;
    public float touch_pos;
    public Transform touch_trigger;

    void FixedUpdate()
    {
        if (cev.triggerClicked)
        {
            Vector3 pt1 = touch_trigger.position + touch_trigger.localScale.y * touch_trigger.up;
            Vector3 pt2 = touch_trigger.position - touch_trigger.localScale.y * touch_trigger.up;
            Vector3 position = Vector3.Lerp(pt2, pt1, touch_pos);

            face.plane.distance = -Vector3.Dot(face.plane.normal, position);
            face.polyhedron.RecomputeMesh();
        }
        else
            Destroy(this);
    }
}