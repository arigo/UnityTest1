using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomLightmap1 : MonoBehaviour {
    public Texture2D myTexture;

	private void Start()
    {
        Renderer r = GetComponent<Renderer>();

        var lightmap_data = new LightmapData();
        /* renamed 'lightmapColor' in Unity 5.6? */
        lightmap_data.lightmapLight = myTexture;

        var lightmaps = new LightmapData[] { lightmap_data };
        LightmapSettings.lightmapsMode = LightmapsMode.NonDirectional;
        LightmapSettings.lightmaps = lightmaps;
        r.lightmapScaleOffset = new Vector4(1f, 1f, 0.3f, 0.3f);
        r.lightmapIndex = 0;
	}
}
