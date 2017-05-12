using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;


public class PadToucher : MonoBehaviour
{
    public Transform moveOnly;
    public Transform rotateOnly;

    public VRTK_ControllerEvents cev;

    /************************************************************************************************/

    enum EFindFace { OUTSIDE, FACE, INSIDE };

    EFindFace FindFace(ConvexPolyhedron poly, Transform trigger, out ConvexPolyhedronFace touch_face, out float touch_pos)
    {
        Vector3 pt1 = trigger.position + trigger.localScale.y * trigger.up;
        Vector3 pt2 = trigger.position - trigger.localScale.y * trigger.up;
        float best_dist = float.PositiveInfinity;
        touch_face = null;
        touch_pos = 0;

        for (int i = 0; i < poly.faces.Count; i++)
        {
            var face = poly.faces[i];
            float dist1 = face.plane.GetDistanceToPoint(pt1);
            float dist2 = face.plane.GetDistanceToPoint(pt2);
            if (dist2 <= 0)
                continue;               /* the pt on the side of the player is inside the plane defining that face */
            else if (dist1 > 0)
                return EFindFace.OUTSIDE;    /* both pt1 and pt2 are outside that face => completely outside */
            else
            {
                /* the segment penetrates the plane of the face, at the position 'v' */
                float frac = dist2 / (dist2 - dist1);
                Vector3 v = Vector3.Lerp(pt2, pt1, frac);

                /* how much outside another plane is v? */
                float dist_max = 0f;
                foreach (var face2 in poly.faces)
                    dist_max = Mathf.Max(dist_max, face2.plane.GetDistanceToPoint(v));
                if (dist_max < best_dist)
                {
                    best_dist = dist_max;
                    touch_face = face;
                    touch_pos = frac;
                }
            }
        }
        if (touch_face != null)
            return EFindFace.FACE;
        return EFindFace.INSIDE;
    }

    void OnTriggerStay(Collider other)
    {
        ConvexPolyhedron poly = other.GetComponent<ConvexPolyhedron>();
        if (poly == null)
            return;

        Dictionary<ConvexPolyhedronFace, Color> faces_enabled = new Dictionary<ConvexPolyhedronFace, Color>();
        foreach (var mf in other.GetComponents<FaceMoveFollower>())
            faces_enabled[mf.face] = new Color(0.45f, 0.45f, 1);

        ConvexPolyhedronFace touch_face;
        float touch_pos;
        switch (FindFace(poly, moveOnly.transform, out touch_face, out touch_pos))
        {
            case EFindFace.INSIDE:
                foreach (var face in poly.faces)
                    faces_enabled[face] = new Color(0.7f, 1, 0.7f);
                break;

            case EFindFace.FACE:
                if (cev.triggerClicked)
                {
                    if (!faces_enabled.ContainsKey(touch_face))
                    {
                        var mf = other.gameObject.AddComponent<FaceMoveFollower>();
                        mf.face = touch_face;
                        mf.cev = cev;
                        mf.touch_pos = touch_pos;
                        mf.touch_trigger = moveOnly.transform;
                    }
                }
                faces_enabled[touch_face] = new Color(0.6f, 0.6f, 1);
                break;

            case EFindFace.OUTSIDE:
                break;
        }

        MeshRenderer mr = other.GetComponent<MeshRenderer>();
        Material[] materials = mr.materials;

        for (int i = 0; i < materials.Length; i++)
        {
            ConvexPolyhedronFace key = poly.faces[i];
            Color c = faces_enabled.ContainsKey(key) ? faces_enabled[key] : Color.white;
            materials[i].color = c;
        }
        mr.materials = materials;
    }
}