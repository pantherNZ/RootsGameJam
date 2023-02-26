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
    public int currentConnections = 1;
    public float tValue = 1.0f;

    private PathCreation.PathCreator spline;

    private void Start()
    {
        if( transform.parent != null )
        {
            var rootCreator = transform.parent.GetComponent<RootMeshCreator>();
            if( rootCreator != null )
                spline = rootCreator.pathCreator;
        }
    }

    private void LateUpdate()
    {
        if( spline != null )
        {
            transform.SetPositionAndRotation(
                spline.path.GetPointAtTime( tValue, PathCreation.EndOfPathInstruction.Stop ),
                spline.path.GetRotation( tValue, PathCreation.EndOfPathInstruction.Stop ) );
        }
    }
}
