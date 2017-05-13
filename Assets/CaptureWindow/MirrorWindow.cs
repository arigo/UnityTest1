using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using BaroqueUI;
using UnityEngine;


public class MirrorWindow : ControllerTracker
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
    
    enum Mode { Nonhighlight, Highlight, Drag };
    Mode mode;


    private void Start()
    {
        lock_obj = new object();
        quad1 = transform.Find("Front").gameObject;
        quad2 = transform.Find("Back").gameObject;
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

        UpdateTexture();

        quad1.transform.localScale = new Vector3(width, height, 1);
        quad2.transform.localScale = new Vector3(width, height, 1);
        quad1.transform.localPosition = new Vector3(0, height * 0.5f, 0);
        quad2.transform.localPosition = new Vector3(0, height * 0.5f, 0);

        //quad1.AddComponent<RenderWindow>().mirror = this;
        //quad2.AddComponent<RenderWindow>().mirror = this;

        quad1.SetActive(true);
        quad2.SetActive(true);

        GetComponent<BoxCollider>().center = new Vector3(0, height * 0.5f, 0);
        GetComponent<BoxCollider>().size = new Vector3(width, height, 1000);
    }

    void UpdateTexture(bool only_if_changed = false)
    {
        if (toplevel_updater.IsForeground(hWnd))
        {
            if (mode == Mode.Nonhighlight)
                mode = Mode.Highlight;
            else if (!only_if_changed)
                return;
        }
        else
        {
            if (mode == Mode.Highlight)
                mode = Mode.Nonhighlight;
            else if (!only_if_changed)
                return;
        }

        string mat_name = "Nonhighlight";
        switch (mode)
        {
            case Mode.Highlight: mat_name = "Highlight"; break;
            case Mode.Drag: mat_name = "Drag"; break;
        }

        Renderer rend = quad1.GetComponent<Renderer>();
        rend.material = Resources.Load<Material>("CaptureWindow/" + mat_name);
        rend.material.mainTexture = m_Texture;
        quad2.GetComponent<Renderer>().sharedMaterial = rend.sharedMaterial;
    }

    private void Update()
    {
        IntPtr rendered = m_rendered;
        if (rendered != m_updated)
        {
            UpdateTexture(only_if_changed: true);
            m_Texture.SetPixels32(m_Pixels, 0);
            m_Texture.Apply();
            m_updated = rendered;
        }
    }

    void FixedUpdate()
    {
        CaptureDLL.Capture_GetWindowSize(hWnd, out m_width, out m_height);

        if (m_width < 0)
        {
            Destroy(gameObject);
            return;
        }

        if (m_Texture == null || m_Texture.width != m_width || m_Texture.height != m_height)
            ResizeTexture(m_width, m_height);
    }

    public void RenderAsynchronously()
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


    /***********************************************************************************************
     * Hovering...
     */

    struct FlatPoint
    {
        public int x, y;
    }

    public GameObject mousePointerPrefab;
    Transform mousePointer;
    Vector3 mousePointerScale;

    float DistanceToPlane(Controller controller)
    {
        FlatPoint ignored;
        return DistanceToPlane(controller, out ignored);
    }

    float DistanceToPlane(Controller controller, out FlatPoint screen_pos)
    {
        screen_pos = new FlatPoint();

        var plane = new Plane(transform.forward, transform.position);
        var ray = new Ray(controller.position, controller.rotation * Vector3.forward);
        float distance;
        if (plane.Raycast(ray, out distance) && distance < 10)
        {
            int sign = plane.GetSide(controller.position) ? -1 : 1;
            Vector3 p = ray.origin + distance * ray.direction;
            p = transform.InverseTransformPoint(p);
            screen_pos.x = (int)(m_width * 0.5f + sign * p.x);
            screen_pos.y = (int)(m_height - p.y);
            if (screen_pos.x >= 0 && screen_pos.x < m_width &&
                screen_pos.y >= 0 && screen_pos.y < m_height)
                return distance;
            return -1;
        }
        return -2;
    }

    public override void OnEnter(Controller controller)
    {
        CaptureDLL.SetForegroundWindow(hWnd);
    }

    public override void OnMoveOver(Controller controller)
    {
        FlatPoint pt;
        ShowMouseLaser(controller, out pt);
    }

    bool ShowMouseLaser(Controller controller, out FlatPoint pt)
    {
        if (mousePointer == null)
        {
            mousePointer = Instantiate(mousePointerPrefab).transform;
            mousePointerScale = mousePointer.localScale;
        }
        float dist = DistanceToPlane(controller, out pt);
        mousePointer.position = controller.position;
        mousePointer.rotation = controller.rotation;
        mousePointerScale.z = dist * 0.5f;
        mousePointer.localScale = mousePointerScale;
        mousePointer.gameObject.SetActive(dist > 0);

        return dist >= -1;   /* raycast worked, but might be a bit off the window */
    }

    void HideMouseLaser()
    {
        Destroy(mousePointer.gameObject);
        mousePointer = null;
    }

    public override void OnLeave(Controller controller)
    {
        HideMouseLaser();
    }

    public override float GetPriority(Controller controller)
    {
        float dist = DistanceToPlane(controller);
        if (dist <= 0)
            return float.NegativeInfinity;
        return -dist;
    }

    public override bool CanStartTeleportAction(Controller controller)
    {
        return false;
    }

    /***********************************************************************************************
     * Clicking...
     */

#if false
    struct MouseEvent {
        public IntPtr hWnd;
        public int kind, x, y;
    };

    static PCQueue<MouseEvent> mouse_event_queue;

    static void SendMouseEvents()
    {
        while (true)
        {
            MouseEvent ev = mouse_event_queue.Pop();
            CaptureDLL.Capture_SendMouseEvent(ev.hWnd, ev.kind, ev.x, ev.y);
        }
    }
    
    static void PushMouseEvent(IntPtr hWnd, int kind, int x, int y)
    {
        if (mouse_event_queue == null)
        {
            mouse_event_queue = new PCQueue<MouseEvent>();
            new Thread(SendMouseEvents).Start();
        }
        MouseEvent ev = new MouseEvent { hWnd = hWnd, kind = kind, x = x, y = y };
        mouse_event_queue.Push(ev);
    }
#endif
    static void PushMouseEvent(IntPtr hWnd, int kind, int x, int y)
    {
        CaptureDLL.Capture_SendMouseEvent(hWnd, kind, x, y);
    }

    public override void OnTriggerDown(Controller controller)
    {
        FlatPoint pt;
        if (ShowMouseLaser(controller, out pt))
        {
            PushMouseEvent(hWnd, 1, pt.x, pt.y);
            foreach (var rend in mousePointer.GetComponentsInChildren<Renderer>())
                rend.material.color = Color.red;
        }
    }

    public override void OnTriggerDrag(Controller controller)
    {
        FlatPoint pt;
        if (ShowMouseLaser(controller, out pt))
            PushMouseEvent(hWnd, 2, pt.x, pt.y);
    }

    public override void OnTriggerUp(Controller controller)
    {
        FlatPoint pt;
        if (!ShowMouseLaser(controller, out pt))
            pt = new FlatPoint { x = -1, y = -1 };
        PushMouseEvent(hWnd, 3, pt.x, pt.y);
        HideMouseLaser();   /* reset the normal color */
    }

    /***********************************************************************************************
     * Grabbing...
     */

    Vector3 origin_position;
    Quaternion origin_rotation;

    public override void OnGripDown(Controller controller)
    {
        /* Called when the grip button is pressed. */
        HideMouseLaser();
        origin_rotation = Quaternion.Inverse(controller.rotation) * transform.rotation;
        origin_position = Quaternion.Inverse(transform.rotation) * (transform.position - controller.position);

        /* We also change the texture to use Drag. */
        mode = Mode.Drag;
        UpdateTexture();
    }

    public override void OnGripDrag(Controller controller)
    {
        /* Dragging... */
        transform.rotation = FixOneRotation(controller.rotation * origin_rotation);
        transform.position = controller.position + transform.rotation * origin_position;
    }

    Quaternion FixOneRotation(Quaternion q)
    {
        Vector3 euler = q.eulerAngles;
        euler.z = 0;
        return Quaternion.Euler(euler);
    }

    public override void OnGripUp(Controller controller)
    {
        mode = Mode.Highlight;
        UpdateTexture();
    }
}
