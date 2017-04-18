﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class BallInfo
{
    public const float SPEED_LIMIT = 1.1f;
    public const float SPEED_EXPONENT = -5f;
    public const float MIN_X = -4f;
    public const float MAX_X = 4f;
    public const float MIN_Y = 0.32f;
    public const float MAX_Y = 2.2f;
    public const float MIN_Z = -1.25f;
    public const float MAX_Z = 1.25f;

    public Transform transform;
    public float speed, radius;
    public int id;

    public BallInfo(Transform _transform, int _id)
    {
        transform = _transform;
        id = _id;
        speed = SPEED_LIMIT * 10f;
        radius = transform.lossyScale.y * 0.5f;
    }

    public int Update(float dt, float speed_reduction)
    {
        Vector3 forward = transform.forward;
        Vector3 p = transform.position + forward * (dt * speed);
        transform.position = p;

        int bump = -1;
        if (p.x - radius <= MIN_X) { forward.x = Mathf.Abs(forward.x); bump = 0; }
        if (p.x + radius >= MAX_X) { forward.x = -Mathf.Abs(forward.x); bump = 1; }
        if (p.y - radius <= MIN_Y) { forward.y = Mathf.Abs(forward.y); bump = 2; }
        if (p.y + radius >= MAX_Y) { forward.y = -Mathf.Abs(forward.y); bump = 3; }
        if (p.z - radius <= MIN_Z) { forward.z = Mathf.Abs(forward.z); bump = 4; }
        if (p.z + radius >= MAX_Z) { forward.z = -Mathf.Abs(forward.z); bump = 5; }
        if (bump >= 0) transform.forward = forward;

        speed = (speed - SPEED_LIMIT) * speed_reduction + SPEED_LIMIT;
        return bump;
    }

    public void Destroy()
    {
        GameObject.Destroy(transform.gameObject);
    }
}


public class BallScene : MonoBehaviour {

    public Renderer endWall1, endWall2;
    public Text scoreText;
    public int score1, score2;
    public Transform remoteBallPrefab;
    public Transform remotePadPrefab;
    public NetworkConnecter networkConnecter;

    List<BallInfo> balls;
    Dictionary<int, BallInfo> balls_by_id;
    Transform[] local_pads, remote_pads;

    void Awake()
    {
        local_pads = new Transform[0];
        remote_pads = new Transform[0];
    }

    void Start()
    {
        MsgReset();
        StartCoroutine(SendPadUpdates().GetEnumerator());
    }

    public void NewBall(Transform ball)
    {
        int new_id;
        while (true)
        {
            new_id = UnityEngine.Random.Range(1, 0x000fffff);
            if (!balls_by_id.ContainsKey(new_id))
                break;
        }
        var info = new BallInfo(ball, new_id);
        balls.Add(info);
        balls_by_id[new_id] = info;

        UpdateBall(info);
    }

    void UpdateBall(BallInfo info)
    {
        networkConnecter.SendSpawn(info.id, info.transform.position, info.transform.forward * info.speed);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        float speed_reduction = Mathf.Exp(dt * BallInfo.SPEED_EXPONENT);

        for (int i = balls.Count - 1; i >= 0; i--)
        {
            var ball = balls[i];
            int bump = ball.Update(dt, speed_reduction);
            if (bump == 0) AddSplash(ball, endWall1, ref score2);
            if (bump == 1) AddSplash(ball, endWall2, ref score1);
        }
    }

    void FixedUpdate()
    {
        Collider[] colls = new Collider[1];
        foreach (var ball in balls)
            if (Physics.OverlapSphereNonAlloc(ball.transform.position, ball.radius, colls,
                                              Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) > 0)
            {
                Collider coll = colls[0];
                PadIndex pad_index = coll.GetComponentInParent<PadIndex>();
                if (pad_index == null)
                    continue;
                Vector3 axis = coll.transform.up;
                bool send_update = false;
                float side_position = Vector3.Dot(axis, coll.transform.position - ball.transform.position);
                float side_movement = Vector3.Dot(axis, ball.transform.forward);
                if (side_position * side_movement > 0)
                {
                    ball.transform.forward = Vector3.Reflect(ball.transform.forward, axis);
                    pad_index.HapticPulse(0.5f);
                    send_update = true;
                }
                float move = ball.radius - Mathf.Abs(side_position);
                if (move > 0)
                    ball.transform.position -= axis * move * Mathf.Sign(side_position);

                if (send_update)
                    UpdateBall(ball);
            }
    }

    /********************************************************************************************/

    const int RENDER_SIZE = 1024;
    const int NB_DOTS = 2000;
    const float splashRadius = 0.12f;

    void ClearRenderTexture(Renderer endWall, Color color)
    {
        if (endWall == null)
            return;
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = (RenderTexture)endWall.material.mainTexture;
        GL.Clear(true, true, color);
        RenderTexture.active = prev;
    }

    Texture GetSplashTexture(float radius, Color color)
    {
        int r = (int)(2 * radius * RENDER_SIZE) + 1;
        Texture2D tex2d = new Texture2D(r, r);
        tex2d.wrapMode = TextureWrapMode.Clamp;
        for (int j = 0; j < r; j++)
            for (int i = 0; i < r; i++)
                tex2d.SetPixel(i, j, Color.clear);

        for (int i = 0; i < NB_DOTS; i++)
        {
            float angle = UnityEngine.Random.value * 2 * Mathf.PI;
            float distance = UnityEngine.Random.value;
            distance = distance * distance * radius;
            float dx = Mathf.Sin(angle) * distance;
            float dy = Mathf.Cos(angle) * distance;
            int x = (int)((dx + radius) * RENDER_SIZE);
            int y = (int)((dy + radius) * RENDER_SIZE);
            tex2d.SetPixel(x, y, color);
        }
        tex2d.Apply();
        return tex2d;
    }

    void AddSplash(BallInfo ball, Renderer endWall, ref int current_score)
    {
        if (endWall == null)
            return;

        Color color = ball.transform.GetComponent<Renderer>().sharedMaterial.color;
        Vector3 position = ball.transform.position;
        position = endWall.transform.InverseTransformPoint(position);
        float radius = splashRadius / endWall.transform.lossyScale.magnitude;

        float px = position.x + 0.5f;
        float py = position.y + 0.5f;

        float x = (px - radius) * RENDER_SIZE;
        float y = (py - radius) * RENDER_SIZE;
        float r = (2 * radius) * RENDER_SIZE;

        Texture splashTexture = GetSplashTexture(radius, color);

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = (RenderTexture)endWall.material.mainTexture;
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, RENDER_SIZE, 0, RENDER_SIZE);
        Graphics.DrawTexture(new Rect(x, y, r, r),
                             splashTexture,
                             new Rect(0, 0, 1, 1),
                             0, 0, 0, 0);
        GL.PopMatrix();
        RenderTexture.active = prev;

        ball.Destroy();
        balls.Remove(ball);
        balls_by_id.Remove(ball.id);

        current_score++;
        ReportMessage(score2 + " - " + score1);
    }

    /********************************************************************************************/

    public void ReportMessage(string text)
    {
        scoreText.text = text;
    }

    public void MsgReset()
    {
        if (balls != null)
            foreach (var ball in balls)
                ball.Destroy();

        balls = new List<BallInfo>();
        balls_by_id = new Dictionary<int, BallInfo>();
        ClearRenderTexture(endWall1, Color.white);
        ClearRenderTexture(endWall2, Color.white);

        ReportMessage("Ready");
    }

    public void MsgSpawn(int id, Vector3 position, Vector3 velocity)
    {
        BallInfo info;

        if (balls_by_id.ContainsKey(id))
            info = balls_by_id[id];
        else
        {
            Transform ball_tr = Instantiate<Transform>(remoteBallPrefab);

            info = new BallInfo(ball_tr, id);
            balls.Add(info);
            balls_by_id[id] = info;
        }

        info.transform.position = position;
        info.transform.rotation = Quaternion.LookRotation(velocity);
        info.speed = velocity.magnitude;
    }

    public void MsgRemotePadCount(int count)
    {
        if (remote_pads.Length == count)
            return;

        int old_length = remote_pads.Length;
        for (int i = count; i < old_length; i++)
            Destroy(remote_pads[i].gameObject);
        Array.Resize<Transform>(ref remote_pads, count);
        for (int i = old_length; i < count; i++)
        {
            Transform tr = Instantiate<Transform>(remotePadPrefab);
            remote_pads[i] = tr;
            foreach (var coll in tr.GetComponentsInChildren<Collider>())
                coll.enabled = false;
        }
    }

    public void MsgRemotePad(int index, Vector3 position, Quaternion rotation)
    {
        Transform tr = remote_pads[index];
        tr.position = position;
        tr.rotation = rotation;
    }

    public void SetPad(int index, Transform transform)   /* or null */
    {
        if (index >= local_pads.Length)
            Array.Resize<Transform>(ref local_pads, index + 1);
        local_pads[index] = transform;
    }

    IEnumerable SendPadUpdates()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.06f);

            int n = 0;
            for (int i = 0; i < local_pads.Length; i++)
                if (local_pads[i] != null)
                    n++;

            var positions = new Vector3[n];
            var rotations = new Quaternion[n];
            n = 0;
            for (int i = 0; i < local_pads.Length; i++)
            {
                Transform tr = local_pads[i];
                if (tr != null)
                {
                    positions[n] = tr.position;
                    rotations[n] = tr.rotation;
                    n++;
                }
            }
            networkConnecter.SendPads(positions, rotations);
        }
    }
}
