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

    void BounceOff(int i, Vector3 normal, float bounce=0.00001f)
    {
        Vector3 v = joints_velocity[i];
        if (Vector3.Dot(v, normal) < 0)
        {
            Vector3 delta = Vector3.Project(v, normal);
            delta *= (1 + Random.Range(0, bounciness));
            joints_velocity[i] = v - delta;
            joints[i] += normal.normalized * bounce;
        }
    }

    void Update()
    {
        /* physics update: move all 'joints' slightly */
        float sqr_min_distance = base_scale.z * base_scale.z;

        Vector3[] jdiffs = new Vector3[nb_segments];
        for (int i = 0; i < nb_segments; i++)
            jdiffs[i] = joints[i == nb_segments - 1 ? 0 : i + 1] - joints[i];

        for (int i = 0; i < nb_segments; i++)
        {
            /*   j1 --- j2 --- j3     we're moving j2 in this iteration */

            Vector3 j2 = joints[i];
            Vector3 j21 = -jdiffs[i == 0 ? nb_segments - 1 : i - 1];
            Vector3 j23 = jdiffs[i];
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
                BounceOff(i, hitInfo.normal, 0.0001f);
        }

        /* model update: make all gameobjects in 'links' match the position expected from 'joints' */
        Vector3 j_prev = joints[nb_segments - 1];

        for (int i = 0; i < nb_segments; i++)
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
