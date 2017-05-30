using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BaroqueUI;
using System;

public class NewObjectMenu : MonoBehaviour
{
    public Transform objectScene;
    public PhysicMaterial physics;

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

        playingScene = new GameObject("Playing Scene").transform;
        for (int i = 0; i < objectScene.childCount; i++)
        {
            var child = objectScene.GetChild(i);
            var clone = Instantiate(child, playingScene, worldPositionStays: true);
            Destroy(clone.GetComponent<MoveObject>());
            Destroy(clone.GetComponent<BoxCollider>());

            var coll = clone.gameObject.AddComponent<MeshCollider>();
            coll.inflateMesh = true;
            coll.skinWidth = 0.0001f;
            coll.convex = true;
            coll.sharedMaterial = physics;

            var rb = clone.gameObject.AddComponent<Rigidbody>();
            var scale = clone.transform.localScale;
            rb.mass = scale.x * scale.y * scale.z;
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
