using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlattenedDrawing : MonoBehaviour {

    RenderTexture renderTexture;

    const int SIZE = 4096;
    const int NB_DOTS = 1500;
    const float splashRadius = 0.05f;

	void Start()
    {
        renderTexture = (RenderTexture)GetComponent<Renderer>().material.mainTexture;
        ClearRenderTexture(new Color(0.8f, 0.8f, 0.8f));
        AddSplash(new Vector3(-3.09f, 1.158f, -4.982f), new Color(1, 0, 0.5f));
    }

    void ClearRenderTexture(Color color)
    {
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = renderTexture;
        GL.Clear(true, true, color);
        RenderTexture.active = prev;
    }

    Texture GetSplashTexture()
    {
        float radius = splashRadius / transform.lossyScale.magnitude;
        int r = (int)(2 * radius * SIZE) + 1;
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
            int x = (int)((dx + radius) * SIZE);
            int y = (int)((dy + radius) * SIZE);
            tex2d.SetPixel(x, y, Color.white);
        }
        tex2d.Apply();
        return tex2d;
    }

    public void AddSplash(Vector3 position, Color color)
    {
        position = transform.InverseTransformPoint(position);
        float radius = splashRadius / transform.lossyScale.magnitude;

        float px = position.x + 0.5f;
        float py = position.y + 0.5f;

        float x = (px - radius) * SIZE;
        float y = (py - radius) * SIZE;
        float r = (2 * radius) * SIZE;

        Texture splashTexture = GetSplashTexture();

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = renderTexture;
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, SIZE, 0, SIZE);
        Graphics.DrawTexture(new Rect(x, y, r, r),
                             splashTexture,
                             new Rect(0, 0, 1, 1),
                             0, 0, 0, 0,
                             color);
        GL.PopMatrix();
        RenderTexture.active = prev;
    }
}
