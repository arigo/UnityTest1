using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RopeBuilder : MonoBehaviour {

    public Transform segment_prefab;
    public int nb_segments = 36;
    public float segment_length = 0.1f;
    public float tensile_strength = 2f;
    public float bounciness = 0.2f;

    Vector3[] joints;
    Vector3[] joints_velocity;
    Transform[] links;
    Vector3 base_scale;

	void Start()
    {
        float radius = nb_segments * segment_length / (2 * Mathf.PI);
        joints = new Vector3[nb_segments];
        joints_velocity = new Vector3[nb_segments];
        links = new Transform[nb_segments];
        for (int i = 0; i < nb_segments; i++)
        {
            float angle = (2 * Mathf.PI) * i / nb_segments;
            Vector3 position = radius * new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            joints[i] = transform.position + position;
            links[i] = Instantiate<Transform>(segment_prefab, transform);
        }
        base_scale = segment_prefab.localScale;
        if (segment_prefab.gameObject.activeInHierarchy)
            Destroy(segment_prefab.gameObject);
    }

    void BounceOff(int i, Vector3 normal)
    {
        Vector3 v = joints_velocity[i];
        if (Vector3.Dot(v, normal) < 0)
        {
            Vector3 delta = Vector3.Project(v, normal);
            delta *= (1 + Random.Range(0, bounciness));
            joints_velocity[i] = v - delta;
            joints[i] += normal.normalized * 0.0001f;
        }
    }

    void Update()
    {
        /* physics update: move all 'joints' slightly */
        float sqr_min_distance = base_scale.z * base_scale.z;
        int i = Random.Range(0, nb_segments);
        int i_step = Random.Range(0, 2) == 0 ? 1 : -1;

        for (int k = 0; k < nb_segments; k++)
        {
            Vector3 j1 = joints[i > 0 ? i - 1 : nb_segments - 1];
            Vector3 j2 = joints[i];
            Vector3 j3 = joints[i == nb_segments - 1 ? 0 : i + 1];
            Vector3 j21 = j1 - j2;
            Vector3 j23 = j3 - j2;
            Vector3 accel = (j21 + j23) * tensile_strength;
            Vector3 v = joints_velocity[i] + Time.deltaTime * accel;
            joints_velocity[i] = v;
            joints[i] = j2 + Time.deltaTime * v;

            /* change the velocity to avoid coming too close to one of the two nearby joints */
            if (j21.sqrMagnitude < sqr_min_distance)
                BounceOff(i, -j21);

            if (j23.sqrMagnitude < sqr_min_distance)
                BounceOff(i, -j23);

            /* detect collisions */
            Vector3 movement = joints[i] - j2;
            foreach (var hitInfo in Physics.SphereCastAll(j2, base_scale.z, movement,
                                                          movement.magnitude,
                                                          Physics.DefaultRaycastLayers,
                                                          QueryTriggerInteraction.Ignore))
                BounceOff(i, hitInfo.normal);

            i += i_step;
            if (i < 0)
                i += nb_segments;
            else if (i >= nb_segments)
                i -= nb_segments;
        }

        /* model update: make all gameobjects in 'links' match the position expected from 'joints' */
        Vector3 j_prev = joints[nb_segments - 1];

        for (i = 0; i < nb_segments; i++)
        {
            Vector3 j1 = j_prev;
            Vector3 j2 = joints[i];

            Vector3 step = j2 - j1;
            Transform tr = links[i];
            Quaternion rot = Quaternion.LookRotation(step, tr.up);
            tr.rotation = rot;
            Vector3 scale = base_scale;
            scale.z = step.magnitude;
            tr.localScale = scale;
            tr.position = (j1 + j2) * 0.5f;
            
            j_prev = j2;
        }
    }
}
