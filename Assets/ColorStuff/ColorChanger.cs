using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class ColorChanger : MonoBehaviour {

    public GameObject colorViewParent;
    public Renderer colorView;
    public GameObject colorSelectorDisc;
    public Renderer colorSelectorRenderer;

    bool selector_active = false;
    Vector3 color_selection_anchor;
    Vector3 color_from_cylinder_origin;

    public Color selected_color
    {
        get { return colorView.material.color; }
        set { colorView.material.color = value; }
    }

    SteamVR_Events.Action newPosesAppliedAction;

    private void Start()
    {
        colorSelectorDisc.SetActive(false);
        colorSelectorRenderer.material.mainTexture = CreateTexture.HSV_Texture();

        GetComponent<VRTK_ControllerEvents>().TouchpadPressed += ColorChanger_TouchpadPressed;
        GetComponent<VRTK_ControllerEvents>().TouchpadReleased += ColorChanger_TouchpadReleased;

        newPosesAppliedAction = SteamVR_Events.NewPosesAppliedAction(OnNewPosesApplied);
        newPosesAppliedAction.enabled = true;
    }

    const float DEPTH = 3f;

    private void ColorChanger_TouchpadPressed(object sender, ControllerInteractionEventArgs e)
    {
        colorViewParent.transform.rotation = transform.rotation;
        color_selection_anchor = transform.position - transform.rotation * color_from_cylinder_origin;

        selector_active = true;
        OnNewPosesApplied();
        colorSelectorDisc.SetActive(true);
    }

    private void ColorChanger_TouchpadReleased(object sender, ControllerInteractionEventArgs e)
    {
        color_from_cylinder_origin = Quaternion.Inverse(transform.rotation) * (transform.position - color_selection_anchor);

        colorSelectorDisc.SetActive(false);
        selector_active = false;
    }

    private void OnNewPosesApplied()
    {
        colorViewParent.transform.position = transform.position;
        colorViewParent.transform.rotation = transform.rotation;

        if (selector_active)
        {
            colorSelectorDisc.transform.rotation = transform.rotation;
            Vector3 vmain = transform.position - color_selection_anchor;
            Vector3 upv = Vector3.Project(vmain, colorView.transform.up);
            colorSelectorDisc.transform.position = color_selection_anchor + upv;

            colorViewParent.transform.position += 0.007f * colorViewParent.transform.up;

            Vector3 vmainlocal = colorSelectorRenderer.transform.InverseTransformVector(vmain) * 2;
            Color c1 = CreateTexture.PositionToColor(vmainlocal.x, vmainlocal.y);

            float value = Mathf.Clamp(DEPTH * Vector3.Dot(vmain, colorView.transform.up), 0, 1);
            Color c = colorSelectorRenderer.material.color;
            c.r = c.g = c.b = value;
            colorSelectorRenderer.material.color = c;

            c1.r *= value;
            c1.g *= value;
            c1.b *= value;
            selected_color = c1;
        }
    }
}
