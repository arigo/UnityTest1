using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class VRGuiTestScript2 : MonoBehaviour {

    public VRGuiExtensibleBox extensibleBoxPrefab;

    string[] labels = {
        "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten",
        "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen", "Twenty",
    };
    float[] values;
    VRGuiExtensibleBox extensibleBox;

    private void Start()
    {
        values = new float[labels.Length];
        for (int i = 0; i < values.Length; i++)
            values[i] = 0.01f * (1 + i);

        extensibleBox = Instantiate<VRGuiExtensibleBox>(extensibleBoxPrefab, transform.position, transform.rotation);
        extensibleBox.set_labels(labels, values);
    }
}
