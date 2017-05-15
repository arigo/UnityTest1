using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Underwater : MonoBehaviour
{
    public Texture[] images;
    int n;

	void FixedUpdate()
    {
        GetComponent<Projector>().material.mainTexture = images[n / 2];
        n = (n + 1) % (2 * images.Length);
	}
}
