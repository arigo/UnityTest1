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

    [DllImport("user32")]
    internal static extern IntPtr GetForegroundWindow();

    [DllImport("user32")]
    internal static extern void SetForegroundWindow(IntPtr hwnd);

    [DllImport("WindowCapture")]
    internal static extern void Capture_GetWindowSize(IntPtr hwnd, out int width, out int height);

    [DllImport("WindowCapture")]
    internal static extern int Capture_UpdateContent(IntPtr hwnd, IntPtr pixels, int width, int height);

    [DllImport("WindowCapture")]
    internal static extern void Capture_SendMouseEvent(IntPtr hWnd, int kind, int x, int y);

    [DllImport("WindowCapture")]
    internal static extern void Capture_SendKeyEvent(int unichar);
}


public class UpdateTopLevelWindows : MonoBehaviour
{
    public float pixelsPerMeter = 1200;
    public int maxWindows = 50;
    public Vector2 randomRange = new Vector2(3, 3);
    public KeyboardClicker keyboard;
    public MirrorWindow windowPrefab;

    Dictionary<IntPtr, MirrorWindow> toplevel_windows;
    volatile IntPtr hwnd_create_me;
    volatile IntPtr hwnd_foreground;

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
        //Stopwatch stopwatch = new Stopwatch();

        while (true)
        {
            //stopwatch.Start();

            int num_windows = CaptureDLL.Capture_ListTopLevelWindows(hWnds, MAX_WINDOWS);
            MirrorWindow[] all_windows;
            MirrorWindow foreground_window;

            /* find the windows that have been opened; note that at most one will be mirrored per FixedUpdate() */
            lock (toplevel_windows)
            {
                for (int i = 0; i < num_windows; i++)
                {
                    if (!toplevel_windows.ContainsKey(hWnds[i]))
                        hwnd_create_me = hWnds[i];
                }
                all_windows = toplevel_windows.Values.ToArray();

                hwnd_foreground = CaptureDLL.GetForegroundWindow();
                toplevel_windows.TryGetValue(hwnd_foreground, out foreground_window);
            }
            if (foreground_window != null)
                foreground_window.RenderAsynchronously();

            /* update the window contents.  Update the foreground window a whole lot more often */
            foreach (MirrorWindow mirror in all_windows)
            {
                mirror.RenderAsynchronously();
                if (foreground_window != null)
                    foreground_window.RenderAsynchronously();
            }

            /*stopwatch.Stop();
            int remaining_sleep = (int)(1000f / updatesPerSecond - stopwatch.ElapsedMilliseconds);
            if (remaining_sleep <= 0)
                remaining_sleep = 1;
            Thread.Sleep(remaining_sleep);
            stopwatch.Reset();*/
            Thread.Sleep(1);
        }
    }

    private void FixedUpdate()
    {
        IntPtr hwnd = hwnd_create_me;
        if (hwnd != (IntPtr)0)
        {
            var mirror = Instantiate<MirrorWindow>(windowPrefab, transform);
            float rx = randomRange.x;
            float rz = randomRange.y;
            mirror.transform.localPosition = new Vector3(UnityEngine.Random.Range(-rx, rx), 0, UnityEngine.Random.Range(-rz, rz));
            mirror.transform.localRotation = Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0);
            mirror.transform.localScale = Vector3.one / pixelsPerMeter;

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

    public bool IsForeground(IntPtr hwnd)
    {
        return hwnd == hwnd_foreground;
    }
}
