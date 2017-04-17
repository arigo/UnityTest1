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

    public BallInfo(Transform tr)
    {
        transform = tr;
        speed = SPEED_LIMIT * 10f;
        radius = tr.lossyScale.y * 0.5f;
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
}


public class BallScene : MonoBehaviour {

    public Renderer endWall1, endWall2;
    public Text scoreText;
    public int score1, score2;

    List<BallInfo> balls;

    void Start()
    {
        balls = new List<BallInfo>();
        ClearRenderTexture(endWall1, Color.white);
        ClearRenderTexture(endWall2, Color.white);
    }

    public void NewBall(Transform ball)
    {
        balls.Add(new BallInfo(ball));
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
                float side_position = Vector3.Dot(axis, coll.transform.position - ball.transform.position);
                float side_movement = Vector3.Dot(axis, ball.transform.forward);
                if (side_position * side_movement > 0)
                {
                    ball.transform.forward = Vector3.Reflect(ball.transform.forward, axis);
                    pad_index.HapticPulse(0.5f);
                }
                float move = ball.radius - Mathf.Abs(side_position);
                if (move > 0)
                    ball.transform.position -= axis * move * Mathf.Sign(side_position);
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

    Texture GetSplashTexture(float radius)
    {
        int r = (int)(2 * radius * RENDER_SIZE) + 1;
        Texture2D tex2d = new Texture2D(r, r);
        tex2d.wrapMode = TextureWrapMode.Clamp;
        for (int j = 0; j < r; j++)
            for (int i = 0; i < r; i++)
                tex2d.SetPixel(i, j, Color.clear);

        for (int i = 0; i < NB_DOTS; i++)
        {
            float angle = Random.value * 2 * Mathf.PI;
            float distance = Random.value;
            distance = distance * distance * radius;
            float dx = Mathf.Sin(angle) * distance;
            float dy = Mathf.Cos(angle) * distance;
            int x = (int)((dx + radius) * RENDER_SIZE);
            int y = (int)((dy + radius) * RENDER_SIZE);
            tex2d.SetPixel(x, y, Color.red);
        }
        tex2d.Apply();
        return tex2d;
    }

    void AddSplash(BallInfo ball, Renderer endWall, ref int current_score)
    {
        if (endWall == null)
            return;

        Vector3 position = ball.transform.position;
        position = endWall.transform.InverseTransformPoint(position);
        float radius = splashRadius / endWall.transform.lossyScale.magnitude;

        float px = position.x + 0.5f;
        float py = position.y + 0.5f;

        float x = (px - radius) * RENDER_SIZE;
        float y = (py - radius) * RENDER_SIZE;
        float r = (2 * radius) * RENDER_SIZE;

        Texture splashTexture = GetSplashTexture(radius);

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

        Destroy(ball.transform.gameObject);
        balls.Remove(ball);

        current_score++;
        scoreText.text = score2 + " - " + score1;
    }
}
