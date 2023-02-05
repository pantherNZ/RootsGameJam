using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootConnection : MonoBehaviour
{
    public Root parent;
    public float rootMaxAngleDegrees = 150.0f;
    public bool allowBackwards = false;
    public int numConnectionsAllowed = 1;
}
