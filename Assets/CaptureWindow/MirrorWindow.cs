using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;


public class MirrorWindow : MonoBehaviour
{
    public UpdateTopLevelWindows toplevel_updater;
    public IntPtr hWnd;

    GameObject quad1, quad2;
    Texture2D m_Texture;
    Color32[] m_Pixels;
    GCHandle m_PixelsHandle;
    int m_width, m_height;
    int m_lastframe, m_counter;
    object lock_obj;

    volatile IntPtr m_rendered;
    IntPtr m_updated;


    private void Start()
    {
        lock_obj = new object();

        quad1 = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad1.transform.SetParent(transform, worldPositionStays: false);

        quad2 = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad2.transform.rotation = Quaternion.Euler(0, 180, 0);
        quad2.transform.SetParent(transform, worldPositionStays: false);
    }

    void ResizeTexture(int width, int height)
    {
        lock (lock_obj)
        {
            /* free for the GC */
            m_PixelsHandle.Free();
            m_Pixels = null;
            m_Texture = null;

            if (width == 0 || height == 0)
            {
                quad1.SetActive(false);
                quad2.SetActive(false);
                return;
            }

            /* allocate texture */
            m_Texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            m_Texture.Apply();

            // Create the pixel array for the plugin to write into
            m_Pixels = m_Texture.GetPixels32(0);

            // "pin" the array in memory, so we can pass direct pointer to it's data to the plugin,
            // without costly marshaling of array of structures.
            m_PixelsHandle = GCHandle.Alloc(m_Pixels, GCHandleType.Pinned);
        }

        quad1.GetComponent<Renderer>().material.mainTexture = m_Texture;
        quad2.GetComponent<Renderer>().material.mainTexture = m_Texture;   /* two-faced, for now */

        quad1.transform.localScale = new Vector3(width, height, 1);
        quad2.transform.localScale = new Vector3(width, height, 1);
        quad1.transform.localPosition = new Vector3(0, height * 0.5f, 0);
        quad2.transform.localPosition = new Vector3(0, height * 0.5f, 0);

        //quad1.AddComponent<RenderWindow>().mirror = this;
        //quad2.AddComponent<RenderWindow>().mirror = this;

        quad1.SetActive(true);
        quad2.SetActive(true);
    }

    void Update()
    {
        CaptureDLL.Capture_GetWindowSize(hWnd, out m_width, out m_height);
        if (m_width < 0)
        {
            Destroy(gameObject);
            return;
        }

        if (m_Texture == null || m_Texture.width != m_width || m_Texture.height != m_height)
        {
            ResizeTexture(m_width, m_height);
        }
        else
        {
            IntPtr rendered = m_rendered;
            if (rendered != m_updated)
            {
                m_Texture.SetPixels32(m_Pixels, 0);
                m_Texture.Apply();
                m_updated = rendered;
            }
        }
    }

    public void RenderWindow()
    {
        /* warning, this runs in the secondary thread */
        lock (lock_obj)
        {
            /* XXX fix me: should handle better the situation where the window is resized a lot */
            if (m_PixelsHandle.IsAllocated)
                if (CaptureDLL.Capture_UpdateContent(hWnd, m_PixelsHandle.AddrOfPinnedObject(), m_width, m_height) != 0)
                    m_rendered = (IntPtr)((long)m_rendered + 1);
        }
    }

    private void OnDestroy()
    {
        toplevel_updater.DestroyedWindow(hWnd);
        lock (lock_obj)
            m_PixelsHandle.Free();
    }
}
