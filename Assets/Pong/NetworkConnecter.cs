using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;


public class NetworkConnecter : MonoBehaviour {

    public string hostName;
    public const int WEBSOCK_VERSION = 1;
    public BallScene ballScene;

    volatile WebSocket ws1;
    volatile string queued_error;
    Queue<float[]> queued_messages;
    Queue<float[]> queued_outgoing_messages;
    ManualResetEvent send_ready;

    private void Start()
    {
        queued_messages = new Queue<float[]>();
        queued_outgoing_messages = new Queue<float[]>();
        send_ready = new ManualResetEvent(false);
        new Thread(HandleWebSocket).Start();
    }

    private void FixedUpdate()
    {
        string err = queued_error;
        if (err != null)
        {
            queued_error = null;
            ballScene.ReportMessage(err);
        }

        lock (queued_messages)
        {
            ProcessQueuedMessages();
        }
    }

    private void OnApplicationQuit()
    {
        WebSocket ws = ws1;
        if (ws != null)
            ws.Close();
    }

    /************************************************************************/
    /* This section of code runs in a separate thread                       */

    float[] DecodeByteArray(byte[] byte_array)
    {
        float[] result = new float[byte_array.Length / 4];
        byte[] four_bytes = new byte[4];

        for (int i = 0; i < result.Length; i++)
        {
            Array.Copy(byte_array, i * 4, four_bytes, 0, 4);
            if (System.BitConverter.IsLittleEndian)
                Array.Reverse(four_bytes);
            result[i] = System.BitConverter.ToSingle(four_bytes, 0);
        }
        return result;
    }

    byte[] EncodeByteArray(float[] float_array)
    {
        byte[] result = new byte[float_array.Length * 4];

        for (int i = 0; i < float_array.Length; i++)
        {
            byte[] four_bytes = System.BitConverter.GetBytes(float_array[i]);
            if (System.BitConverter.IsLittleEndian)
                Array.Reverse(four_bytes);
            Array.Copy(four_bytes, 0, result, i * 4, 4);
        }
        return result;
    }

    void GotMessageAsync(byte[] raw_data)
    {
        float[] message = DecodeByteArray(raw_data);
        lock(queued_messages)
        {
            queued_messages.Enqueue(message);
        }
    }

    private void HandleWebSocket()
    {
        WebSocket ws = new WebSocket("ws://" + hostName + "/websock/" + WEBSOCK_VERSION);
        ws.OnMessage += (sender, e) => GotMessageAsync(e.RawData);
        ws.OnError += (sender, e) => { queued_error = e.Message; ws1 = null; };
        ws.Connect();
        ws1 = ws;

        while (true)
        {
            send_ready.WaitOne();
            lock (queued_outgoing_messages)
            {
                if (queued_outgoing_messages.Count > 0)
                {
                    ws.Send(EncodeByteArray(queued_outgoing_messages.Dequeue()));
                }
                else
                    send_ready.Reset();
            }
        }
    }

    /************************************************************************/

    const int MSG_RESET = 0;
    const int MSG_SPAWN = 1;
    const int MSG_PADS  = 2;

    void ProcessQueuedMessages()
    {
        while (queued_messages.Count > 0)
        {
            float[] message = queued_messages.Dequeue();
            switch ((int)message[0])
            {
                case MSG_RESET:
                    Debug.Log("Connected to remote!");
                    ballScene.MsgReset();
                    break;

                case MSG_SPAWN:
                    ballScene.MsgSpawn(-(int)message[1],
                        new Vector3(-message[2], message[3], -message[4]),
                        new Vector3(-message[5], message[6], -message[7]));
                    break;

                case MSG_PADS:
                    int n, count = (message.Length - 1) / 6;
                    ballScene.MsgRemotePadCount(count);
                    for (n = 0; n < count; n++)
                    {
                        int i = 1 + n * 6;
                        ballScene.MsgRemotePad(n,
                            new Vector3(-message[i], message[i + 1], -message[i + 2]),
                            Quaternion.Euler(message[i + 3], message[i + 4] + 180, message[i + 5]));
                    }
                    break;
            }
        }
    }

    void _Send(float[] data)
    {
        if (ws1 != null)
        {
            lock (queued_outgoing_messages)
            {
                queued_outgoing_messages.Enqueue(data);
                send_ready.Set();
            }
        }
    }

    public void SendReset()
    {
        _Send(new float[] { MSG_RESET });
    }

    public void SendSpawn(int id, Vector3 position, Vector3 velocity)
    {
        _Send(new float[] { MSG_SPAWN, id,
            position.x, position.y, position.z,
            velocity.x, velocity.y, velocity.z });
    }

    public void SendPads(Vector3[] positions, Quaternion[] rotations)
    {
        var data = new float[1 + 6 * positions.Length];
        data[0] = MSG_PADS;
        for (int n = 0; n < positions.Length; n++)
        {
            Vector3 position = positions[n];
            Vector3 angles = rotations[n].eulerAngles;
            int i = 1 + 6 * n;
            data[i + 0] = position.x;
            data[i + 1] = position.y;
            data[i + 2] = position.z;
            data[i + 3] = angles.x;
            data[i + 4] = angles.y;
            data[i + 5] = angles.z;
        }
        _Send(data);
    }
}
