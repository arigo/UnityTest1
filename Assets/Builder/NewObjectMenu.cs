using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BaroqueUI;
using System;

public class NewObjectMenu : MonoBehaviour
{
    public Transform objectScene;

    Transform playingScene;
    bool playing { get { return playingScene != null; } }

	void Start()
    {
        var gt = Controller.GlobalTracker(this);
        gt.onTriggerDown += OnTriggerDown;
        gt.onMenuClick += OnMenuClick;

        /* display a message in the console viewer */
        Debug.Log(">>> Click anywhere to open the New Object menu <<<");
	}

    private void OnMenuClick(Controller controller)
    {
        if (!playing)
            Play();
        else
            Stop();
    }

    private void OnTriggerDown(Controller controller)
    {
        if (playing)
        {
            Stop();
            return;
        }
        var origin = controller.position;
        var menu = new Menu();
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            menu.Add(child.name, () => AddObject(origin, child));
        };
        menu.Add("Play!", Play);
        menu.MakePopup(controller, gameObject);
    }

    void AddObject(Vector3 origin, Transform prefab)
    {
        if (playing)
            return;
        Transform clone = Instantiate(prefab);
        clone.position = origin;
        clone.SetParent(objectScene);
        clone.gameObject.SetActive(true);
    }

    void Play()
    {
        if (playing)
            return;

        Controller.ForceLeave();

        var groups = new RepSep<MoveObject>();
        playingScene = new GameObject("Playing Scene").transform;

        for (int i = 0; i < objectScene.childCount; i++)
        {
            var mo = objectScene.GetChild(i).GetComponent<MoveObject>();
            if (mo.touching != null)
            {
                foreach (var mo2 in mo.touching)
                    groups.Merge(mo, mo2);
            }
        }

        var rbs = new Dictionary<MoveObject, Rigidbody>();
        for (int i = 0; i < objectScene.childCount; i++)
        {
            var source = objectScene.GetChild(i);
            var clone = Instantiate(source);
            Destroy(clone.GetComponent<MoveObject>());
            Destroy(clone.GetComponent<Rigidbody>());

            var coll = clone.GetComponent<MeshCollider>();
            if (coll != null)
            {
                Destroy(clone.GetComponent<BoxCollider>());
                coll.enabled = true;
            }
            else
                clone.GetComponent<BoxCollider>().isTrigger = false;

            var mo = source.GetComponent<MoveObject>();
            mo = groups.GetRep(mo);

            Rigidbody rb;
            if (!rbs.TryGetValue(mo, out rb))
            {
                var go = new GameObject("Group");
                go.transform.SetParent(playingScene);
                rb = go.AddComponent<Rigidbody>();
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.mass = 0;
                rbs[mo] = rb;
            }
            var scale = clone.transform.localScale;
            rb.mass += scale.x * scale.y * scale.z * 1000;

            clone.SetParent(rb.transform);
        }

        objectScene.gameObject.SetActive(false);
    }

    void Stop()
    {
        if (playing)
        {
            objectScene.gameObject.SetActive(true);
            Destroy(playingScene.gameObject);
            playingScene = null;
        }
    }
}
