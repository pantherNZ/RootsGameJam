using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuLetter : MonoBehaviour
{
    private Camera mainCamera;
    private float moveTimeBase = 1.0f;
    private float moveTimeRand = 0.5f;
    private Vector3 startPos;

    private void Awake()
    {
        var constants = GameController.Instance.Constants;
        moveTimeBase = constants.menuSlerpSpeed;
        moveTimeRand = constants.menuSlerpSpeed / 4.0f;

        mainCamera = Camera.main;
        startPos = transform.position;
        Reset();
    }

    Vector3 GeneratePos()
    {
        var newPos = Utility.RandomBool() ?
            new Vector2( Utility.RandomBool() ? 0.5f : -0.5f, Random.value - 0.5f ) :
            new Vector2( Random.value - 0.5f, Utility.RandomBool() ? 0.5f : -0.5f );

        const float margin = 100.0f;
        var pos = new Vector3(
            newPos.x * ( mainCamera.pixelWidth + margin ),
            newPos.y * ( mainCamera.pixelHeight + margin ),
            0.0f );
        pos += new Vector3( mainCamera.pixelWidth / 2.0f, mainCamera.pixelHeight / 2.0f );
        return pos;
    }

    public void Reset()
    {
        transform.position = GeneratePos();
    }

    public float GenerateTime()
    {
        return moveTimeBase + ( Random.value - 0.5f ) * moveTimeRand;
    }

    public void Hide( float? timeOverride = null )
    {
        this.InterpolatePosition( GeneratePos(), timeOverride ?? GenerateTime(), false, true );
    }

    public void Show( float? timeOverride = null )
    {
        this.InterpolatePosition( startPos, timeOverride ?? GenerateTime(), false, false );
    }
}
