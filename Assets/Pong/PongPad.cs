using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PongPad : MonoBehaviour {

    public GameObject padObjectPrefab;
    public BallScene ballScene;

    GameObject padObject;
    SteamVR_Events.Action newPosesAppliedAction;
    int pad_index;

    void Start()
    {
        padObject = Instantiate(padObjectPrefab);
        padObject.SetActive(false);
        pad_index = -1;

        padObject.GetComponent<PadIndex>().controller = gameObject;

        newPosesAppliedAction = SteamVR_Events.NewPosesAppliedAction(OnNewPosesApplied);
        newPosesAppliedAction.enabled = true;
    }

    private void OnNewPosesApplied()
    {
        if (!isActiveAndEnabled)
        {
            if (pad_index >= 0) {
                padObject.SetActive(false);
                ballScene.SetPad(pad_index, null);
                pad_index = -1;
            }
            return;
        }
        if (pad_index < 0)
        {
            pad_index = (int)padObject.transform.GetComponent<PadIndex>().GetPadIndex();
            ballScene.SetPad(pad_index, transform);
            padObject.SetActive(true);
        }
        padObject.transform.position = transform.position;
        padObject.transform.rotation = transform.rotation;
    }
}
