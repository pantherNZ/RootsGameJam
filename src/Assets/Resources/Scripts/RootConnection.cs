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
    [ReadOnly]
    public int currentConnections = 1;
    public float tValue = 1.0f;
    public float? distAlongPath;
    public bool autoUpdatePos = true;

    private PathCreation.PathCreator spline;

    private void Start()
    {
        currentConnections = numConnectionsAllowed;

        if( transform.parent != null )
        {
            var rootCreator = transform.parent.GetComponent<RootMeshCreator>();
            if( rootCreator != null )
                spline = rootCreator.pathCreator;
        }
    }

    private void LateUpdate()
    {
        if( spline != null && autoUpdatePos )
        {
            UpdatePosition();
        }
    }

    public void UpdatePosition()
    {
        if( distAlongPath.HasValue )
        {
            transform.SetPositionAndRotation(
                  spline.path.GetPointAtDistance( distAlongPath.Value, PathCreation.EndOfPathInstruction.Stop ),
                  spline.path.GetRotationAtDistance( distAlongPath.Value, PathCreation.EndOfPathInstruction.Stop ) );
        }
        else
        {
            transform.SetPositionAndRotation(
                   spline.path.GetPointAtTime( tValue, PathCreation.EndOfPathInstruction.Stop ),
                   spline.path.GetRotation( tValue, PathCreation.EndOfPathInstruction.Stop ) );
        }
    }
}
