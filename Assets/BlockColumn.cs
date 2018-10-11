using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BaroqueUI;


public class BlockColumn : MonoBehaviour
{
    public int nBlocks;

	void Start()
    {
        float y = 0;
        for (int i = 0; i < nBlocks; i++)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.SetParent(transform);
            go.transform.localScale = new Vector3(5.5f, 1f, 5.5f);
            go.transform.localPosition = new Vector3(0, y + 0.5f, 0);
            go.transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 90f), 0);
            go.AddComponent<Rigidbody>();
            go.AddComponent<RemoveBlock>();
            y += 1;
        }
	}

    class RemoveBlock : MonoBehaviour
    {
        private void Start()
        {
            var ht = Controller.HoverTracker(this);
            ht.onEnter += Ht_onEnter;
            ht.onLeave += Ht_onLeave;
            ht.onTriggerDown += Ht_onTriggerDown;
        }

        private void Ht_onEnter(Controller controller)
        {
            GetComponent<Renderer>().material.color = Color.cyan;
        }

        private void Ht_onLeave(Controller controller)
        {
            GetComponent<Renderer>().material.color = Color.white;
        }

        private void Ht_onTriggerDown(Controller controller)
        {
            Destroy(gameObject);
        }
    }
}
