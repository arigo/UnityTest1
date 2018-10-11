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
    internal static extern void SetForegroundWindow(IntPtr hwnd);

    [DllImport("user32")]
    internal static extern int GetWindowTextW(IntPtr hwnd, [Out] char[] lpString, int maxCount);

    [DllImport("WindowCapture")]
    internal static extern int Capture_GetWindowProcess(IntPtr hwnd);

    [DllImport("WindowCapture")]
    internal static extern void Capture_GetWindowSize(IntPtr hwnd, out int width, out int height);

    [DllImport("WindowCapture")]
    internal static extern void Capture_GetWindowPos(IntPtr hwnd, out int x, out int y);

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
    public BaroqueUI.KeyboardClicker keyboard;
    public MirrorWindow windowPrefab;

    Dictionary<IntPtr, MirrorWindow> toplevel_windows;
    IntPtr create_me;
    object create_me_lock;

    private void Start()
    {
        create_me = (IntPtr)0;
        create_me_lock = new object();
        toplevel_windows = new Dictionary<IntPtr, MirrorWindow>();
        new Thread(ThreadUpdateMirrors).Start();
    }

    void ThreadUpdateMirrors()
    {
        try
        {
            UpdateMirrors();
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
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
            IntPtr loc_create_me = (IntPtr)0;

            /* find the windows that have been opened; note that at most one will be mirrored per FixedUpdate() */
            lock (toplevel_windows)
            {
                foreground_window = null;
                for (int i = num_windows - 1; i >= 0; i--)
                {
                    IntPtr hwnd = hWnds[i];
                    if (!toplevel_windows.ContainsKey(hwnd))
                        loc_create_me = hwnd;
                    else
                    {
                        foreground_window = toplevel_windows[hwnd];
                        foreground_window.z_order = i;
                    }
                }
                all_windows = toplevel_windows.Values.ToArray();
            }
            lock (create_me_lock)
                create_me = loc_create_me;

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
        IntPtr hwnd;
        lock (create_me_lock)
        {
            hwnd = create_me;
            if (hwnd == (IntPtr)0)
                return;

            create_me = (IntPtr)0;
        }

        lock (toplevel_windows)
        {
            if (!toplevel_windows.ContainsKey(hwnd))
            {
                int process = CaptureDLL.Capture_GetWindowProcess(hwnd);

                /* find the topmost window from the same process */
                MirrorWindow best_sibling = null;
                int best_z_order = 0x7fffffff;

                foreach (var other in toplevel_windows.Values)
                {
                    if (other.process == process && other.z_order < best_z_order &&
                        other.m_width > 0)
                    {
                        best_z_order = other.z_order;
                        best_sibling = other;
                    }
                }

                var mirror = Instantiate<MirrorWindow>(windowPrefab, transform);
                mirror.toplevel_updater = this;
                mirror.hWnd = hwnd;
                mirror.process = process;
                mirror.z_order = 0;   /* may be wrong, but will be fixed later */
                mirror.transform.localScale = Vector3.one / pixelsPerMeter;

                if (best_sibling == null)
                {
                    float rx = randomRange.x;
                    float rz = randomRange.y;
                    mirror.transform.localPosition = new Vector3(UnityEngine.Random.Range(-rx, rx), 0, UnityEngine.Random.Range(-rz, rz));
                    mirror.transform.localRotation = Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0);
                }
                else
                {
                    int ox, oy, nx, ny, sizex, sizey;
                    CaptureDLL.Capture_GetWindowPos(best_sibling.hWnd, out ox, out oy);
                    CaptureDLL.Capture_GetWindowPos(hwnd, out nx, out ny);
                    CaptureDLL.Capture_GetWindowSize(hwnd, out sizex, out sizey);

                    Vector3 new_attach_point = new Vector3(
                        (ox + 0.5f * best_sibling.m_width) - (nx + 0.5f * sizex),
                        (oy + best_sibling.m_height) - (ny + sizey),
                        5);
                    //UnityEngine.Debug.Log(new_attach_point);

                    if (best_sibling.transform.InverseTransformPoint(BaroqueUI.Baroque.GetHeadTransform().position).z < 0)
                    {
                        new_attach_point.x = -new_attach_point.x;
                        new_attach_point.z = -new_attach_point.z; 
                    }

                    mirror.transform.localRotation = best_sibling.transform.localRotation;
                    mirror.transform.position = best_sibling.transform.TransformPoint(new_attach_point);
                }

                toplevel_windows[hwnd] = mirror;
            }
        }
    }

    public void DestroyedWindow(IntPtr hwnd)
    {
        lock (toplevel_windows)
            toplevel_windows.Remove(hwnd);
    }

    public bool IsForeground(MirrorWindow window)
    {
        return window.z_order == 0;
    }
}
