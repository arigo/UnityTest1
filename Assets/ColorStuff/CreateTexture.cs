using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateTexture {

	public static Texture HSV_Texture() {
        const int W = 1024;
        const int H = 1024;

        Color[] colors = new Color[W * H];
        int index = 0;

        for (int j = 0; j < H; j++)
            for (int i = 0; i < W; i++)
                colors[index++] = PositionToColor(((i + 0.5f) / W) * 2f - 1f, ((j + 0.5f) / H) * 2f - 1f, true);

        Texture2D tex2d = new Texture2D(W, H);
        tex2d.SetPixels(colors);
        tex2d.wrapMode = TextureWrapMode.Clamp;
        tex2d.filterMode = FilterMode.Bilinear;
        tex2d.Apply();
        return tex2d;
	}

    public static Color PositionToColor(float x, float y, bool checkBound=false)
    {
        float S = new Vector2(x, y).magnitude;
        if (S > 1 && checkBound)
            return Color.clear;
        float H = Mathf.Atan2(y, x) / (2 * Mathf.PI);
        if (H < 0)
            H += 1;
        return Color.HSVToRGB(H, S, 1f);
    }

    /*public static void ColorToPosition(Color col, out float x, out float y, out float z)
    {
        float H, S;
        Color.RGBToHSV(col, out H, out S, out z);
        H *= 2 * Mathf.PI;
        x = Mathf.Cos(H);
        y = Mathf.Sin(H);
    }*/
}
