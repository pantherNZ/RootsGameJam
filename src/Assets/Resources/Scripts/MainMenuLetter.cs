using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuLetter : MonoBehaviour
{
    [SerializeField] GameConstants constants;

    private Camera mainCamera;
    private float moveTimeBase = 1.0f;
    private float moveTimeRand = 0.5f;
    private Vector3 startPos;

    private void Start()
    {
        moveTimeBase = constants.menuSlerpSpeed;
        moveTimeRand = constants.menuSlerpSpeed / 4.0f;

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
        this.InterpolatePosition( GeneratePos(), moveTimeBase + ( Random.value - 0.5f ) * moveTimeRand, true, true );
    }

    public void Show()
    {
        this.InterpolatePosition( startPos, moveTimeBase + ( Random.value - 0.5f ) * moveTimeRand, true, false );
    }
}
