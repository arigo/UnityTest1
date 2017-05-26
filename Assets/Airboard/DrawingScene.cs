using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BaroqueUI;


public class DrawingScene : MonoBehaviour
{
    public Transform remotePadPrefab, remoteHeadSet;
    public LineRenderer lineRendererPrefab;
    public NetworkConnecter1 networkConnecter;
    public float padUpdatesFrequency = 30f;
    public float padExpectedUpdatesFrequency = 20f;
    public EControllerButton controllerButton;

    List<LineRenderer> lines;
    Dictionary<int, LineRenderer> currently_drawing_line;
    RemotePad[] remote_pads;

    void Awake()
    {
        remote_pads = new RemotePad[0];
    }

    void Start()
    {
        MsgReset();
        StartCoroutine(SendPadUpdates().GetEnumerator());
    }

    public void MsgReset()
    {
        if (lines != null)
            foreach (var line in lines)
                Destroy(line.gameObject);
        lines = new List<LineRenderer>();
        currently_drawing_line = new Dictionary<int, LineRenderer>();
        Debug.Log("MsgReset");
    }

    public void MsgRemotePadCount(int count)
    {
        if (remote_pads.Length == count)
            return;

        int old_length = remote_pads.Length;
        for (int i = count; i < old_length; i++)
            if (i != 0)
                Destroy(remote_pads[i].gameObject);

        Array.Resize<RemotePad>(ref remote_pads, count);

        for (int i = old_length; i < count; i++)
        {
            Transform tr = i == 0 ? remoteHeadSet : Instantiate<Transform>(remotePadPrefab);
            foreach (var coll in tr.GetComponentsInChildren<Collider>())
                coll.enabled = false;
            RemotePad rp = tr.GetComponent<RemotePad>();
            if (rp == null)
                rp = tr.gameObject.AddComponent<RemotePad>();
            rp.Configure(1f / padExpectedUpdatesFrequency);
            remote_pads[i] = rp;
        }
    }

    public void MsgRemotePad(int index, Vector3 position, Quaternion rotation, float drawing)
    {
        remote_pads[index].MessageMoveTo(position, rotation);

        int key = -(int)drawing;
        if (key == 0)
            return;
        if (key <= -100)
        {
            key += 100;
            currently_drawing_line.Remove(key);
        }
        DrawLine(key, position, rotation);
    }

    IEnumerable SendPadUpdates()
    {
        var local_pads = new List<Transform>();
        var local_drawings = new List<float>();

        while (true)
        {
            yield return new WaitForSeconds(1f / padUpdatesFrequency);

            local_pads.Clear();
            local_drawings.Clear();

            local_pads.Add(Baroque.GetHeadTransform());
            local_drawings.Add(0);

            int index = 0;
            foreach (var controller in Baroque.GetControllers())
            {
                index += 1;
                if (controller.isActiveAndEnabled)
                {
                    local_pads.Add(controller.transform);
                    local_drawings.Add(UpdateController(controller, index));
                }
                else
                    currently_drawing_line.Remove(index);
            }

            int n = local_pads.Count;
            var positions = new Vector3[n];
            var rotations = new Quaternion[n];

            for (int i = 0; i < n; i++)
            {
                Transform tr = local_pads[i];
                positions[i] = tr.position;
                rotations[i] = tr.rotation;
            }
            networkConnecter.SendPads(positions, rotations, local_drawings.ToArray());
        }
    }

    void DrawLine(int index, Vector3 c_position, Quaternion c_rotation)
    {
        LineRenderer drawing_line;
        if (!currently_drawing_line.TryGetValue(index, out drawing_line))
        {
            drawing_line = Instantiate<LineRenderer>(lineRendererPrefab);
            currently_drawing_line[index] = drawing_line;
            lines.Add(drawing_line);
        }
        int i = drawing_line.numPositions;
        drawing_line.numPositions = i + 1;
        drawing_line.SetPosition(i, c_position + c_rotation * Vector3.forward * 0.08f);
    }

    float UpdateController(Controller controller, int index)
    {
        controller.SetPointer(remotePadPrefab.gameObject);

        int result;
        if (controller.GetButton(controllerButton))
        {
            if (currently_drawing_line.ContainsKey(index))
                result = index;
            else
                result = index + 100;
            DrawLine(index, controller.position, controller.rotation);
        }
        else
        {
            currently_drawing_line.Remove(index);
            result = 0;
        }
        return result;
    }
}
