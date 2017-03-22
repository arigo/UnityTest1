using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ColoredBehaviour : MonoBehaviour
{
    public Kinds m_kind;
    private bool m_disabled;

    public Kinds kind
    {
        get { return m_kind; }
        set { m_kind = value; SetColor(GetKindColor()); }
    }

    public bool disabled
    {
        get { return m_disabled; }
        set { m_disabled = value; SetColor(GetKindColor()); }
    }

    protected Color GetKindColor()
    {
        Color res = GlobalData.instance.kind2Color[(int)kind];
        if (disabled)
            res = Color.Lerp(res, Color.clear, 0.5f);
        return res;
    }

    protected void SetColor(Color c)
    {
        Renderer rend = GetComponent<Renderer>();
        rend.material.SetColor("_Color", c);
    }
}


public class BallController : ColoredBehaviour
{
    const float maxSqrMagnitude = 75000;

    private void FixedUpdate()
    {
        if (GetComponent<Transform>().position.sqrMagnitude > maxSqrMagnitude)
            Destroy(gameObject);
    }

    void MsgInteractionStart()
    {
        disabled = true;
    }
}