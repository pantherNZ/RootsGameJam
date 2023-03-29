using UnityEngine;

public class Parallax : MonoBehaviour
{
    [SerializeField] float scalar = 1.0f;
    [SerializeField] bool affectX = true;
    [SerializeField] bool affectY = false;
    Vector3? previousPos;

    private void Update()
    {
        var pos = GameController.Instance.player.transform.position;

        if( previousPos == null )
            previousPos = pos;

        var diff = previousPos.Value - pos;
        diff = diff.SetX( affectX ? diff.x : 0.0f )
                   .SetY( affectY ? diff.y : 0.0f );
        previousPos = pos;
        if( diff.sqrMagnitude > 0.001f )
            transform.position -= ( diff * Mathf.Abs( transform.position.z ) / 100.0f ) * scalar;
    }
}