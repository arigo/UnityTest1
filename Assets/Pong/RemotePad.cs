using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RemotePad : MonoBehaviour {

    float full_delta_time, arrival_time;
    Vector3 target_position;
    Quaternion target_rotation;


    public void Configure(float fullDeltaTime)
    {
        full_delta_time = fullDeltaTime;
        arrival_time = -1;
    }

    public void MessageMoveTo(Vector3 position, Quaternion rotation)
    {
        target_position = position;
        target_rotation = rotation;
        if (arrival_time >= 0)
            arrival_time = Time.time + full_delta_time;
    }

    void Update()
    {
        if (arrival_time <= 0)
            return;

        float remaining = arrival_time - Time.time;
        if (remaining <= 0)
        {
            transform.position = target_position;
            transform.rotation = target_rotation;
            arrival_time = 0;
        }
        else
        {
            float rem_f = remaining / full_delta_time;
            transform.position = Vector3.Lerp(target_position, transform.position, rem_f);
            transform.rotation = Quaternion.Slerp(target_rotation, transform.rotation, rem_f);
        }
    }
}

