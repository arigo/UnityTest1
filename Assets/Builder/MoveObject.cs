using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BaroqueUI;
using System;

public class MoveObject : MonoBehaviour
{
    public Material outlineMaterial;
    public Material outlineMaterialGrabbed;
    public Transform faceIndicator;
    public Material faceIndicatorGrabbed;

    Renderer rend;
    BoxCollider coll;
    Transform face_select;
    Vector3 current_face;
    public HashSet<MoveObject> touching;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        coll = GetComponent<BoxCollider>();

        var ct = Controller.HoverTracker(this);
        ct.onEnter += (ctrl) => { Entering(); ctrl.HapticPulse(250); };
        ct.onLeave += (ctrl) => { Leaving(); };
        ct.onMoveOver += OnMoveOver;
        ct.onTriggerDown += OnTriggerDown;
        ct.onTriggerDrag += OnTriggerDrag;
        ct.onTriggerUp += OnTriggerUp;
        ct.onGripDown += OnGripDown;
        ct.onGripDrag += OnGripDrag;
        ct.onGripUp += OnGripUp;
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("OnTriggerEnter: " + gameObject.name + " -> " + other.gameObject.name);
        var mo = other.GetComponent<MoveObject>();
        if (mo != null)
        {
            if (touching == null)
                touching = new HashSet<MoveObject>();
            touching.Add(mo);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        //Debug.Log("OnTriggerExit: " + gameObject.name + " -> " + other.gameObject.name);
        var mo = other.GetComponent<MoveObject>();
        if (mo != null)
        {
            if (touching != null)
                touching.Remove(mo);
        }
    }


    /* =====   Selection   ===== */

    void SelectOutline(Material outline)
    {
        var materials = new Material[] { rend.sharedMaterial, outline };
        rend.sharedMaterials = materials;
    }

    void SelectFace(Vector3 face)
    {
        if (face != current_face)
        {
            if (face == Vector3.zero)
                DestroyImmediate(face_select.gameObject);
            else
            {
                if (current_face == Vector3.zero)
                    face_select = Instantiate(faceIndicator, transform);
                face_select.localPosition = face * 0.51f * Mathf.Abs(Vector3.Dot(face, coll.size));
                face_select.localRotation = Quaternion.LookRotation(face);
                face_select.localScale = new Vector3(1.15f, 1.15f, 1.15f);
            }
            current_face = face;
        }
    }

    void Entering()
    {
        SelectFace(Vector3.zero);
        SelectOutline(outlineMaterial);
    }

    void Leaving()
    {
        SelectFace(Vector3.zero);
        rend.sharedMaterials = new Material[] { rend.sharedMaterial };
    }

    private void OnMoveOver(Controller controller)
    {
        /* compute pos_outside, which is a few centimeters back from the controller position in the
         * direction of the controller's torus.  If pos_outside is really outside, select the face
         * of the BoxCollider that is most appropriate.
         */
        Quaternion angle = Quaternion.Euler(-35, 0, 0);
        Vector3 pos_outside = controller.position + controller.rotation * angle * new Vector3(0, 0, -0.05f);
        //Baroque.DrawLine(pos_inside, pos_outside);

        Vector3 p_in = transform.InverseTransformPoint(pos_outside) - coll.center;
        float dx = Mathf.Abs(p_in.x) - coll.size.x * 0.5f;
        float dy = Mathf.Abs(p_in.y) - coll.size.y * 0.5f;
        float dz = Mathf.Abs(p_in.z) - coll.size.z * 0.5f;
        float dmax = Mathf.Max(dx, dy, dz);
        Vector3 face = Vector3.zero;

        if (dmax > 0)
        {
            if (dmax == dx)
                face = new Vector3(Mathf.Sign(p_in.x), 0, 0);
            else if (dmax == dz)
                face = new Vector3(0, 0, Mathf.Sign(p_in.z));
            else
                face = new Vector3(0, Mathf.Sign(p_in.y), 0);
        }
        SelectFace(face);
    }


    /* =====   Moving around   ===== */

    int grabbed;
    Vector3 position_ofs;

    private void OnTriggerDown(Controller controller)
    {
        if (current_face == Vector3.zero)
        {
            SelectOutline(outlineMaterialGrabbed);
            position_ofs = transform.position - controller.position;
            grabbed = 1;
        }
        else
        {
            foreach (var rend in face_select.GetComponentsInChildren<Renderer>())
                rend.material = faceIndicatorGrabbed;
            float f = 0.5f * Mathf.Abs(Vector3.Dot(current_face, coll.size));
            Vector3 face_center = transform.TransformPoint(current_face * f);
            position_ofs = face_center - controller.position;
            grabbed = 2;
        }
    }

    private void OnTriggerDrag(Controller controller)
    {
        Vector3 new_point = controller.position + position_ofs;

        switch (grabbed)
        {
            case 1:
                transform.position = new_point;
                break;

            case 2:
                //Baroque.DrawLine(transform.position + Vector3.one, new_point);
                float m = Vector3.Dot(transform.InverseTransformPoint(new_point), current_face);
                m = Mathf.Abs(m * 2f);
                // the goal is to change localScale until the above formula would give m == 1
                Vector3 s = transform.localScale;
                if (current_face.x != 0)
                s.x *= m / coll.size.x;
                else if (current_face.y != 0)
                    s.y *= m / coll.size.y;
                else
                    s.z *= m / coll.size.z;
                transform.localScale = s;
                break;
        }
    }

    private void OnTriggerUp(Controller controller)
    {
        grabbed = 0;
    }


    /* =====   Moving & rotating around   ===== */

    Quaternion rotation_ofs;
    HashSet<MoveObject> moving_together;


    private void OnGripDown(Controller controller)
    {
        var together = new HashSet<MoveObject>();
        var pending = new List<MoveObject>();
        pending.Add(this);
        while (pending.Count > 0)
        {
            MoveObject mo = pending[pending.Count - 1];
            pending.RemoveAt(pending.Count - 1);

            if (together.Contains(mo) || mo.grabbed != 0)
                continue;

            together.Add(mo);
            if (mo.touching != null)
                foreach (var mo2 in mo.touching)
                    pending.Add(mo2);
        }
        if (together.Count == 0)
            return;

        SelectFace(Vector3.zero);
        moving_together = together;
        foreach (var mo in together)
        {
            mo.rotation_ofs = Quaternion.Inverse(controller.rotation) * mo.transform.rotation;
            mo.position_ofs = Quaternion.Inverse(mo.transform.rotation) * (mo.transform.position - controller.position);
            mo.grabbed = 3;
        }
    }

    private void OnGripDrag(Controller controller)
    {
        if (moving_together != null)
        {
            foreach (var mo in moving_together)
            {
                if (mo.grabbed == 3)
                {
                    mo.transform.rotation = controller.rotation * mo.rotation_ofs;
                    mo.transform.position = controller.position + mo.transform.rotation * mo.position_ofs;
                }
            }
        }
    }

    private void OnGripUp(Controller controller)
    {
        if (moving_together != null)
        {
            foreach (var mo in moving_together)
            {
                if (mo.grabbed == 3)
                    mo.grabbed = 0;
            }
            moving_together = null;
        }
        Entering();
    }
}