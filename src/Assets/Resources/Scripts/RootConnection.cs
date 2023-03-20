using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RootConnection : MonoBehaviour
{
    public Root parent;
    public float rootMaxAngleDegrees = 150.0f;
    public bool allowBackwards = false;
    public int numConnectionsAllowed = 1;
    [ReadOnly]
    public int currentConnections = 1;
    public float tValue = 1.0f;
    public float distFromEnd = 0.0f;
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
        float distanceToUse;

        if( distAlongPath.HasValue )
        {
            distanceToUse = distAlongPath.Value;
        }
        else
        {
            distanceToUse = spline.path.GetClosestDistanceAlongPath( spline.path.GetPointAtTime( tValue, PathCreation.EndOfPathInstruction.Stop ) );
            distanceToUse -= distFromEnd;
        }

        transform.SetPositionAndRotation(
                spline.path.GetPointAtDistance( distanceToUse, PathCreation.EndOfPathInstruction.Stop ),
                spline.path.GetRotationAtDistance( distanceToUse, PathCreation.EndOfPathInstruction.Stop ) );
    }
}
