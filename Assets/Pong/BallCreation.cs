using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallCreation : MonoBehaviour {

    public Transform ballPrefab;
    public BallScene ballScene;

	void Start()
    {
        StartCoroutine(Create().GetEnumerator());
	}

    IEnumerable Create()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);
            Transform ball = Instantiate<Transform>(ballPrefab, ballScene.transform);
            ball.position = transform.position;
            ball.rotation = transform.rotation * Quaternion.LookRotation(new Vector3(Random.value - 0.5f, Random.value * 0.6f, 0.7f));
            ballScene.NewBall(ball);
        }
    }
}
