using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] GameObject dirtTilePrefab;
    [SerializeField] int dirtTileSize;
    [SerializeField] float groundHeightOffset;
    [SerializeField] float timeOfDay;

    private Camera mainCamera;
    private Dictionary<Vector2Int, GameObject> tiles = new Dictionary<Vector2Int, GameObject>();
    private float groundHeight;
    private Vector2Int currentTile;
    private Vector2Int? previousTile;

    private void Start()
    {
        mainCamera = Camera.main;
        groundHeight = Camera.main.ViewportToWorldPoint( new Vector3( 0.0f, 1.0f, 0.0f ) ).y - dirtTileSize / 2.0f;

        for( int y = -1; y <= 1; y++ )
        {
            for( int x = -1; x <= 1; x++ )
            {
                TryConstructTile( new Vector2Int( x, y ) );
            }
        }
    }

    private int CeilAwayFromZero( float f )
    {
        return ( int )( f + Mathf.Sign( f ) * 0.5f );
    }

    private void Update()
    {
        var cameraPos = mainCamera.transform.position;
        previousTile = currentTile;
        currentTile = new Vector2Int( ( int )( cameraPos.x / dirtTileSize ), ( int )( cameraPos.y / dirtTileSize ) );
        var cameraSize = mainCamera.ViewportToWorldPoint( new Vector3( 1.0f, 1.0f, 0.0f ) ) - cameraPos;
        var bounds = new Rect( cameraPos - cameraSize, cameraSize * 2.0f );

        for( int i = -1; i <= 1; i++ )
        {
            TryConstructTile( new Vector2Int( CeilAwayFromZero( bounds.xMin / dirtTileSize ), currentTile.y + i ) );
            TryConstructTile( new Vector2Int( CeilAwayFromZero( bounds.xMax / dirtTileSize ), currentTile.y + i ) );
            TryConstructTile( new Vector2Int( currentTile.x + i, CeilAwayFromZero( ( bounds.yMin - Mathf.Sign( groundHeight ) ) / dirtTileSize ) ) );
            TryConstructTile( new Vector2Int( currentTile.x + i, CeilAwayFromZero( ( bounds.yMax - Mathf.Sign( groundHeight ) ) / dirtTileSize ) ) );
        }

        TryConstructTile( new Vector2Int( CeilAwayFromZero( bounds.xMin / dirtTileSize ), CeilAwayFromZero( ( bounds.yMin - Mathf.Sign( groundHeight ) ) / dirtTileSize ) ) );
        TryConstructTile( new Vector2Int( CeilAwayFromZero( bounds.xMax / dirtTileSize ), CeilAwayFromZero( ( bounds.yMin - Mathf.Sign( groundHeight ) ) / dirtTileSize ) ) );
        TryConstructTile( new Vector2Int( CeilAwayFromZero( bounds.xMin / dirtTileSize ), CeilAwayFromZero( ( bounds.yMax - Mathf.Sign( groundHeight ) ) / dirtTileSize ) ) );
        TryConstructTile( new Vector2Int( CeilAwayFromZero( bounds.xMax / dirtTileSize ), CeilAwayFromZero( ( bounds.yMax - Mathf.Sign( groundHeight ) ) / dirtTileSize ) ) );
    }

    private void TryConstructTile( Vector2Int pos )
    {
        if( tiles.ContainsKey( pos ) )
            return;

        var newTile = Instantiate( dirtTilePrefab, new Vector3( ( float )pos.x * dirtTileSize, groundHeight + pos.y * dirtTileSize, 0.0f ), Quaternion.identity );
        tiles.Add( pos, newTile );

        if( previousTile.HasValue )
        {
            var movement = currentTile - previousTile.Value;
            //if( tiles.Remove( pos - movement * 3, out var found ) )
            //    found.Destroy();
        }
    }
}
