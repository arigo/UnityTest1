﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Impulser1 : MonoBehaviour
{
    const float impulseFactor = 5.0f;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter!");

        if (other.GetComponent<BallController>() == null)
            return;

        Transform mytr = GetComponent<Transform>();
        Rigidbody rb = other.GetComponent<Rigidbody>();
        rb.position = mytr.position;
        rb.velocity = impulseFactor * mytr.forward;
    }
}