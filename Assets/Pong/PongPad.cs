using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class PongPad : MonoBehaviour {

    public GameObject padObjectPrefab;
    public BallScene ballScene;

    GameObject padObject;
    SteamVR_Events.Action newPosesAppliedAction;
    Vector3 current_velocity;
    Vector3 previous_position;
    float previous_time, current_delta_time;
    bool pad_visible;

    void Start()
    {
        padObject = Instantiate(padObjectPrefab);
        padObject.SetActive(false);
        pad_visible = false;

        padObject.GetComponent<PadIndex>().controller = this;

        newPosesAppliedAction = SteamVR_Events.NewPosesAppliedAction(OnNewPosesApplied);
        newPosesAppliedAction.enabled = true;
    }

    private void OnNewPosesApplied()
    {
        if (!isActiveAndEnabled)
        {
            if (pad_visible) {
                padObject.SetActive(false);
                ballScene.RemovePad(transform);
                pad_visible = false;
            }
            return;
        }
        Vector3 old_position;
        if (!pad_visible)
        {
            ballScene.AddPad(transform);
            padObject.SetActive(true);
            pad_visible = true;

            old_position = transform.position;
            current_velocity = Vector3.zero;
        }
        else
        {
            old_position = previous_position;
            current_velocity = (transform.position - previous_position) / (Time.time - previous_time);
        }
        previous_position = transform.position;
        previous_time = Time.time;

        padObject.transform.position = transform.position;
        padObject.transform.rotation = transform.rotation;

        foreach (var ball in ballScene.balls)
            if (BounceBall(ball, old_position))
                ballScene.UpdateBall(ball);
    }

    public bool BounceBall(BallInfo ball, Vector3 old_position)
    {
        Vector3 axis = transform.up;
        Vector3 relative_velocity = ball.GetVelocity() - current_velocity;
        float side_position = Vector3.Dot(axis, old_position - ball.transform.position);
        float side_movement = Vector3.Dot(axis, relative_velocity);
        if (side_position * side_movement <= 0)
            return false;

        /* c1 = current ball position.
         * c2 = theoretical position of the ball before the last pad movement,
         *      if the whole world was moving such that the pad didn't move
         */
        Vector3 c1 = ball.transform.position;
        Vector3 c2 = c1 + (previous_position - old_position);

        if (!MovingBallTouch(ball, c1, c2))
            return false;

        /* hit!  try to fix the position of the ball.  This is a cast from c2 to c1. */
        RaycastHit hit;
        float distance;
        float max_distance = (c1 - c2).magnitude;
        if (!Physics.SphereCast(c2, ball.radius, c1 - c2, out hit, max_distance,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            /* the old ball position, c2, is probably already touching. */
            distance = 0;
        }
        else
        {
            distance = hit.distance;
        }
        ball.transform.position = Vector3.Lerp(c2, c1, distance / max_distance);

        /* now change the velocity of the ball */
        relative_velocity = Vector3.Reflect(relative_velocity, axis);
        relative_velocity -= Mathf.Sign(side_position) * 2f * axis;   /* automatic extra impulse */
        ball.SetVelocity(relative_velocity + current_velocity);

        padObject.GetComponent<PadIndex>().HapticPulse(0.6f);

        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
        //emitParams.applyShapeToPosition = true;
        ParticleSystem ps = ballScene.particleSystem;
        emitParams.applyShapeToPosition = true;
        emitParams.position = ps.transform.InverseTransformPoint(ball.transform.position);
        ps.Emit(emitParams, 20);

        return true;
    }

    bool MovingBallTouch(BallInfo ball, Vector3 c1, Vector3 c2)
    {
        Collider[] colls = Physics.OverlapCapsule(c1, c2, ball.radius,
            Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < colls.Length; i++)
        {
            PadIndex pad_index = colls[i].GetComponentInParent<PadIndex>();
            if (pad_index != null && pad_index.controller == this)
                return true;
        }
        return false;
    }
}
