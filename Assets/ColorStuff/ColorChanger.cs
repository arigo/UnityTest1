using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class ColorChanger : MonoBehaviour {

    public GameObject colorViewParentPrefab;
    public GameObject colorSelectorObject;
    public MeshFilter colorSelectorSolidMesh, colorSelectorFadedMesh;

    GameObject colorViewParent;
    Renderer colorView;
    bool selector_active = false;

    Vector3[] local_pts;
    Color[] local_colors;

    public Color selected_color
    {
        get { return colorView.sharedMaterial.color; }
        set { colorView.sharedMaterial.color = value; }
    }

    SteamVR_Events.Action newPosesAppliedAction;

    private void Start()
    {
        float r3 = Mathf.Sqrt(3) / 2;
        local_pts = new Vector3[] {
            new Vector3(1, 1, 0),
            new Vector3(.5f, 1, r3),
            new Vector3(-.5f, 1, r3),
            new Vector3(-1, 1, 0),
            new Vector3(-.5f, 1, -r3),
            new Vector3(.5f, 1, -r3)
        };
        local_colors = new Color[] {
            new Color(1, 0, 0),
            new Color(1, 1, 0),
            new Color(0, 1, 0),
            new Color(0, 1, 1),
            new Color(0, 0, 1),
            new Color(1, 0, 1)
        };

        /* TESTING */
        TestLocalPositionToColor(new Vector3(0, -1, 0), Color.black);
        TestLocalPositionToColor(new Vector3(0, 1, 0), Color.white);
        for (int i = 0; i < 6; i++)
        {
            Vector3 v = local_pts[i];
            Color c = local_colors[i];
            TestLocalPositionToColor(v, c);
            v = Vector3.Lerp(new Vector3(0, 1, 0), v, 0.42f);
            c = Color.Lerp(Color.white, c, 0.42f);
            TestLocalPositionToColor(v, c);
            v = Vector3.Lerp(v, local_pts[(i + 1) % 6], 0.3f);
            c = Color.Lerp(c, local_colors[(i + 1) % 6], 0.3f);
            TestLocalPositionToColor(v, c);
            v = Vector3.Lerp(v, new Vector3(0, -1, 0), 0.23f);
            c = Color.Lerp(c, Color.black, 0.23f);
            TestLocalPositionToColor(v, c);

            v = Vector3.Lerp(local_pts[i], local_pts[(i + 1) % 6], 0.2f);
            c = Color.Lerp(local_colors[i], local_colors[(i + 1) % 6], 0.2f);
            TestLocalPositionToColor(v, c);
            TestLocalPositionToColorRT(v * 12.34f, c, v);
        }
        /* ^^^ TESTING */

        colorViewParent = Instantiate(colorViewParentPrefab);
        colorView = colorViewParent.GetComponentInChildren<Renderer>();

        colorSelectorObject.SetActive(false);

        GetComponent<VRTK_ControllerEvents>().TouchpadPressed += ColorChanger_TouchpadPressed;
        GetComponent<VRTK_ControllerEvents>().TouchpadReleased += ColorChanger_TouchpadReleased;

        newPosesAppliedAction = SteamVR_Events.NewPosesAppliedAction(OnNewPosesApplied);
        newPosesAppliedAction.enabled = true;
    }

    const float EPSILON = 0.001f;

    Vector3 ColorToLocalPosition(Color col)
    {
        float y = Mathf.Max(col.r, col.g, col.b);
        if (y < EPSILON)
            return new Vector3(0, -1, 0);

        col.r /= y;
        col.g /= y;
        col.b /= y;

        float d = Mathf.Min(col.r, col.g, col.b);
        Vector3 v;
        if (d > 1 - EPSILON)
        {
            v = new Vector3(0, 1, 0);
        }
        else
        {
            col.r = 1 - (1 - col.r) / (1 - d);
            col.g = 1 - (1 - col.g) / (1 - d);
            col.b = 1 - (1 - col.b) / (1 - d);

            int i;
            float f;

            if (col.b < EPSILON)
            {
                if (col.r > 1 - EPSILON) { i = 0; f = col.g; }
                else { i = 1; f = 1 - col.r; }
            }
            else if (col.g < EPSILON)
            {
                if (col.r > 1 - EPSILON) { i = 5; f = 1 - col.b; }
                else { i = 4; f = col.r; }
            }
            else
            {
                if (col.g > 1 - EPSILON) { i = 2; f = col.b; }
                else { i = 3; f = 1 - col.g; }
            }

            v = Vector3.Lerp(local_pts[i], local_pts[(i + 1) % 6], f);
            v = Vector3.Lerp(v, new Vector3(0, 1, 0), d);
            v *= y;
        }
        v.y = y * 2 - 1;
        return v;
    }

    Color LocalPositionToColor(Vector3 pos)
    {
        float value = (pos.y + 1) * 0.5f;
        if (value < EPSILON)
            return Color.black;

        float x = pos.x / value;
        float z = pos.z / value;

        Color c = Color.white;
        if (Mathf.Abs(x) > EPSILON || Mathf.Abs(z) > EPSILON)
        {
            for (int i = 0; i < 6; i++)
            {
                Vector3 pt1 = local_pts[i];
                Vector3 pt2 = local_pts[(i + 1) % 6];

                Vector3 v = new Vector3(x, 0, z);

                float dot1 = z * pt1.x - x * pt1.z;
                float dot2 = x * pt2.z - z * pt2.x;
                if (dot1 < 0 || dot2 < 0)
                    continue;

                float d = Vector3.ProjectOnPlane(v, pt2 - pt1).magnitude;
                d /= (Mathf.Sqrt(3) / 2);
                v /= d;

                float f = Vector3.Dot(v - pt1, pt2 - pt1) / (pt2 - pt1).magnitude;
                Debug.Assert(f > -EPSILON && f < 1 + EPSILON);
                c = Color.Lerp(local_colors[i], local_colors[(i + 1) % 6], f);
                c = Color.Lerp(Color.white, c, d);
                break;
            }
            Debug.Assert(c != Color.white);
        }
        if (value < 1)
            c *= value;
        return c;
    }

    void TestLocalPositionToColorRT(Vector3 pos, Color c, Vector3 pos_roundtrip)
    {
        Vector3 p1 = ColorToLocalPosition(c);
        Debug.Assert(Vector3.Distance(p1, pos_roundtrip) < EPSILON, c + " => " + p1 + " but expected " + pos_roundtrip);
        Color c1 = LocalPositionToColor(pos);
        Debug.Assert(Mathf.Abs(c1.r - c.r) < EPSILON, pos + " => " + c1 + " but expected " + c);
        Debug.Assert(Mathf.Abs(c1.g - c.g) < EPSILON, pos + " => " + c1 + " but expected " + c);
        Debug.Assert(Mathf.Abs(c1.b - c.b) < EPSILON, pos + " => " + c1 + " but expected " + c);
    }

    void TestLocalPositionToColor(Vector3 pos, Color c)
    {
        TestLocalPositionToColorRT(pos, c, pos);
    }

    /************************************************************************************************/


    private void ColorChanger_TouchpadPressed(object sender, ControllerInteractionEventArgs e)
    {
        float H, S, V;
        Color.RGBToHSV(selected_color, out H, out S, out V);
        Vector3 pos = ColorToLocalPosition(selected_color);
        float angle = H * 360 + colorView.transform.rotation.eulerAngles.y + 90;
        colorSelectorObject.transform.rotation = Quaternion.Euler(0, angle, 0);
        colorSelectorObject.transform.position = colorView.transform.position - colorSelectorObject.transform.TransformVector(pos);
        selector_active = true;
        OnNewPosesApplied();
        colorSelectorObject.SetActive(true);
    }

    private void ColorChanger_TouchpadReleased(object sender, ControllerInteractionEventArgs e)
    {
        colorSelectorObject.SetActive(false);
        selector_active = false;
    }

    const float DEPTH = 3f;
    CreatePolyhedron cph;

    private void OnNewPosesApplied()
    {
        if (!isActiveAndEnabled)
        {
            colorViewParent.SetActive(false);
            return;
        }
        colorViewParent.transform.position = transform.position;
        colorViewParent.transform.rotation = transform.rotation;
        colorViewParent.SetActive(true);

        if (selector_active)
        {
            colorViewParent.transform.position += 0.007f * colorViewParent.transform.up;

            Vector3 pcs = colorSelectorObject.transform.InverseTransformPoint(colorView.transform.position);
            Vector3 norm = -colorSelectorObject.transform.InverseTransformDirection(colorView.transform.up);
            colorSelectorSolidMesh.mesh = BuildMesh(norm, pcs);
            //colorSelectorFadedMesh.mesh = BuildMesh(pcs, 1);

            selected_color = LocalPositionToColor(pcs);
        }
    }

    Mesh BuildMesh(Vector3 normal, Vector3 pcs)
    {
        if (cph == null)
            cph = new CreatePolyhedron();
        cph.Setup(new Plane(normal, pcs));

        for (int i0 = 0; i0 < 6; i0++)
        {
            int i1 = (i0 + 1) % 6;
            Vector3 pt1 = new Vector3(0, 1, 0);
            Vector3 pt2 = new Vector3(0, -1, 0);
            Vector3 pt3 = local_pts[i0];
            Vector3 pt4 = local_pts[i1];
            Color col1 = Color.white;
            Color col2 = Color.black;
            Color col3 = local_colors[i0];
            Color col4 = local_colors[i1];

            /*cph.enabled = true;*/
            cph.AddTriangleCut(pt1, pt2, pt3, col1, col2, col3);
            cph.AddTriangleCut(pt1, pt4, pt2, col1, col4, col2);
            /*cph.enabled = false;
            cph.AddTriangleCut(pt1, pt3, pt4, col1, col3, col4);
            cph.AddTriangleCut(pt2, pt3, pt4, col2, col3, col4);
            cph.enabled = true; 
            cph.AddClosingCap();*/
        }
        return cph.BuildMesh();
    }
}
