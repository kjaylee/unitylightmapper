using System;
using System.Collections;
using UnityEngine;

class Area 
{
    public int x;
    public int y;
    public int order;
    public Rect rc;
    public Area(int x, int y, int order)
    {
        this.x = x;
        this.y = y;
        this.order = order;
    }
    public Area(Rect rc, int order)
    {
        this.order = order;
        this.rc = rc;
    }
}

