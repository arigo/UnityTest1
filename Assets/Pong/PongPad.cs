using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PongPad : MonoBehaviour {

    public GameObject padObjectPrefab;
    public BallScene ballScene;

    GameObject padObject;
    SteamVR_Events.Action newPosesAppliedAction;

    void Start()
    {
        padObject = Instantiate(padObjectPrefab);
        padObject.SetActive(false);

        padObject.GetComponent<PadIndex>().controller = gameObject;

        newPosesAppliedAction = SteamVR_Events.NewPosesAppliedAction(OnNewPosesApplied);
        newPosesAppliedAction.enabled = true;
    }

    private void OnNewPosesApplied()
    {
        if (!isActiveAndEnabled)
        {
            padObject.SetActive(false);
            return;
        }
        padObject.SetActive(true);
        padObject.transform.position = transform.position;
        padObject.transform.rotation = transform.rotation;
    }
}
