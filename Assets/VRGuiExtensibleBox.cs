using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRTK;

public class VRGuiExtensibleBox : MonoBehaviour
{
    public GameObject baseCube;
    public Canvas canvasObject;
    public GameObject highlightPrefab;

    const float MARGIN = 0.2f;
    Image[] images;
    Text[] texts;
    float[] values;

    public void set_labels(string[] labels, float[] values)
    {
        int num = labels.Length;
        float scale_mul = num + (num - 1) * MARGIN;

        Vector3 v = baseCube.transform.localScale;
        v.y = 0.1f * (scale_mul + MARGIN);
        baseCube.transform.localScale = v;

        RectTransform r = canvasObject.GetComponent<RectTransform>();
        r.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 20 * scale_mul);

        images = new Image[num];
        texts = new Text[num];
        images[0] = canvasObject.GetComponentInChildren<Image>();
        texts[0] = canvasObject.GetComponentInChildren<Text>();

        for (int i = 1; i < num; i++)
        {
            images[i] = Instantiate(images[0], canvasObject.transform);
            texts[i] = Instantiate(texts[0], canvasObject.transform);
        }
        this.values = values;
        for (int i = 0; i < num; i++)
        {
            images[i].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(
                RectTransform.Edge.Top,
                20 * i * (1 + MARGIN),
                20);
            texts[i].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(
                RectTransform.Edge.Top,
                20 * i * (1 + MARGIN),
                20);
            texts[i].text = labels[i];
            update_fraction(i);
        }
    }

    public void update_fraction(int i)
    {
        images[i].GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(
            RectTransform.Edge.Left,
            0.0f,
            200.0f * values[i]);
    }

    /**********/

    Transform controller_touch, controller_use;
    GameObject highlight;
    float highlight_x;
    int highlight_y;

    private void Start()
    {
        VRTK_InteractableObject io = baseCube.GetComponent<VRTK_InteractableObject>();
        io.InteractableObjectTouched += (sender, e) => controller_touch = e.interactingObject.transform;
        io.InteractableObjectUntouched += (sender, e) => controller_touch = (controller_touch == e.interactingObject.transform) ? null : controller_touch;
        io.InteractableObjectUsed += (sender, e) => controller_use = e.interactingObject.transform;
    }

    void UpdateHighlightCoord(Transform controller, bool change_y=true)
    {
        Vector3 tp = baseCube.transform.InverseTransformPoint(controller.position);
        highlight_x = Mathf.Clamp01(0.5f + tp.z);
        if (change_y)
        {
            highlight_y = values.Length - 1 - (int)((0.5f + tp.y) * values.Length);
            if (highlight_y >= values.Length)
                highlight_y = values.Length - 1;
            if (highlight_y < 0)
                highlight_y = 0;
        }
        //Debug.Log(tp + " => " + highlight_x + " " + highlight_y);
    }

    private void Update()
    {
        Transform controller = null;
        
        if (controller_use != null)
        {
            if (controller_use.GetComponent<VRTK_ControllerEvents>().triggerPressed)
                controller = controller_use;
            else
                controller_use = null;
        }
        
        if (controller == null)
            controller = controller_touch;
        if (controller == null)
        {
            if (highlight != null)
            {
                Destroy(highlight);
                highlight = null;
            }
            return;
        }

        UpdateHighlightCoord(controller, controller_use == null);

        if (highlight == null)
            highlight = Instantiate(highlightPrefab, baseCube.transform);
        highlight.transform.localPosition = new Vector3(
            0.6f,
            (values.Length - 1 - highlight_y + 0.35f) / values.Length - 0.5f,
            highlight_x - 0.5f);

        if (controller_use != null)
        {
            values[highlight_y] = highlight_x;
            update_fraction(highlight_y);
        }
    }
}