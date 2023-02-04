using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuLetter : MonoBehaviour
{
    private Camera mainCamera;
    const float moveTimeBase = 1.0f;
    const float moveTimeRand = 0.5f;
    private Vector3 startPos;

    private void Start()
    {
        mainCamera = Camera.main;
        startPos = transform.localPosition;
        Reset();
        Show();
    }

    Vector3 GeneratePos()
    {
        var newPos = Utility.RandomBool() ?
            new Vector2( Utility.RandomBool() ? 0.5f : -0.5f, Random.value - 0.5f ) :
            new Vector2( Random.value - 0.5f, Utility.RandomBool() ? 0.5f : -0.5f );

        const float margin = 100.0f;
        return new Vector3(
            newPos.x * ( mainCamera.pixelWidth + margin ),
            newPos.y * ( mainCamera.pixelHeight + margin ),
            0.0f );
    }

    public void Reset()
    {
        transform.localPosition = GeneratePos();
    }

    public void Hide()
    {
        this.InterpolatePosition( GeneratePos(), moveTimeBase + ( Random.value - 0.5f ) * moveTimeRand, true );
    }

    public void Show()
    {
        this.InterpolatePosition( startPos, moveTimeBase + ( Random.value - 0.5f ) * moveTimeRand, true );
    }
}
