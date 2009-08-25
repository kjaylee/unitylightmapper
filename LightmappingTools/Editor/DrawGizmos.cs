using UnityEngine;
using UnityEditor;
using System.Collections;

public class DrawGizmos
{

    // Use this for initialization
    static public void drawGizmo(Renderer tr)
    {
        Debug.DrawLine(tr.bounds.center, new Vector3(0, 0, 0));
    }
}
