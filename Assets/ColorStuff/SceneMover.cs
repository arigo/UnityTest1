using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using VRTK;


public class SceneMover : MonoBehaviour {

    public Transform dropScene;
    public GameObject smallCrossPrefab;
    public FlattenedDrawing flattenedDrawing;

    Vector3 prev_position;
    SteamVR_Events.Action newPosesAppliedAction;
    Transform smallCross;
    bool moved_down = false;

    private void Start()
    {
        GetComponent<VRTK_ControllerEvents>().GripPressed += SpheresMover_GripPressed;
        GetComponent<VRTK_ControllerEvents>().GripReleased += SpheresMover_GripReleased;

        newPosesAppliedAction = SteamVR_Events.NewPosesAppliedAction(OnNewPosesApplied);
    }

    Vector3 SpheresMoverOrigin()
    {
        return transform.position + 0.333f * transform.forward;
    }

    private void SpheresMover_GripPressed(object sender, ControllerInteractionEventArgs e)
    {
        prev_position = SpheresMoverOrigin();
        newPosesAppliedAction.Enable(true);
        smallCross = Instantiate(smallCrossPrefab).transform;
    }

    private void SpheresMover_GripReleased(object sender, ControllerInteractionEventArgs e)
    {
        newPosesAppliedAction.Enable(false);
        Destroy(smallCross.gameObject);
        smallCross = null;
    }

    private void OnNewPosesApplied()
    {
        Vector3 new_position = SpheresMoverOrigin();
        moved_down |= (new_position.y < prev_position.y);
        dropScene.transform.position += new_position - prev_position;
        prev_position = new_position;

        smallCross.position = new_position;
    }

    private void FixedUpdate()
    {
        if (moved_down)
        {
            moved_down = false;

            float floor_y = flattenedDrawing.transform.position.y;
            foreach (Renderer rend in dropScene.GetComponentsInChildren<Renderer>())
            {
                if (rend.transform.position.y < floor_y)
                {
                    Color col = rend.material.color;
                    col.a = 1;
                    flattenedDrawing.AddSplash(rend.transform.position, col);
                    Destroy(rend.gameObject);
                }
            }
        }
    }
}
