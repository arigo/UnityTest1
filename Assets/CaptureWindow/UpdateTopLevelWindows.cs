using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Linq;
using System.Diagnostics;


internal class CaptureDLL
{
    [DllImport("WindowCapture")]
    internal static extern int Capture_ListTopLevelWindows(IntPtr[] hwndarray, int maxcount);

    [DllImport("WindowCapture")]
    internal static extern void Capture_GetWindowSize(IntPtr hwnd, out int width, out int height);

    [DllImport("WindowCapture")]
    internal static extern int Capture_UpdateContent(IntPtr hwnd, IntPtr pixels, int width, int height);
}


public class UpdateTopLevelWindows : MonoBehaviour
{
    public float pixelsPerMeter = 400;
    public float updatesPerSecond = 20;
    public int maxWindows = 150;

    Dictionary<IntPtr, MirrorWindow> toplevel_windows;
    volatile IntPtr hwnd_create_me;

    private void Start()
    {
        hwnd_create_me = (IntPtr)0;
        toplevel_windows = new Dictionary<IntPtr, MirrorWindow>();
        new Thread(UpdateMirrors).Start();
    }

    public void UpdateMirrors()
    {
        int MAX_WINDOWS = maxWindows;
        IntPtr[] hWnds = new IntPtr[MAX_WINDOWS];
        Stopwatch stopwatch = new Stopwatch();

        while (true)
        {
            stopwatch.Start();

            int num_windows = CaptureDLL.Capture_ListTopLevelWindows(hWnds, MAX_WINDOWS);
            MirrorWindow[] all_windows;

            /* find the windows that have been opened; note that at most one will be mirrored per FixedUpdate() */
            lock (toplevel_windows)
            {
                for (int i = 0; i < num_windows; i++)
                {
                    if (!toplevel_windows.ContainsKey(hWnds[i]))
                        hwnd_create_me = hWnds[i];
                }
                all_windows = toplevel_windows.Values.ToArray();
            }

            /* update the window contents */
            foreach (MirrorWindow mirror in all_windows)
            {
                mirror.RenderWindow();
            }

            stopwatch.Stop();
            int remaining_sleep = (int)(1000f / updatesPerSecond - stopwatch.ElapsedMilliseconds);
            if (remaining_sleep <= 0)
                remaining_sleep = 1;
            Thread.Sleep(remaining_sleep);
            stopwatch.Reset();
        }
    }

    private void FixedUpdate()
    {
        IntPtr hwnd = hwnd_create_me;
        if (hwnd != (IntPtr)0)
        {
            var gobj = new GameObject("window");
            gobj.transform.position = transform.position +
                new Vector3(UnityEngine.Random.Range(-5f, 5f), 0, UnityEngine.Random.Range(-5f, 5f));
            gobj.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0);
            gobj.transform.localScale = Vector3.one / pixelsPerMeter;
            gobj.transform.SetParent(transform);

            var mirror = gobj.AddComponent<MirrorWindow>();
            mirror.toplevel_updater = this;
            mirror.hWnd = hwnd;
            lock (toplevel_windows)
                toplevel_windows[hwnd] = mirror;

            hwnd_create_me = (IntPtr)0;
        }
    }

    public void DestroyedWindow(IntPtr hWnd)
    {
        lock (toplevel_windows)
            toplevel_windows.Remove(hWnd);
    }
}
