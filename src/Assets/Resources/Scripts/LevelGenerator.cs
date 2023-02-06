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

    private void Start()
    {
        mainCamera = Camera.main;
        groundHeight = Camera.main.ViewportToWorldPoint( new Vector3( 0.0f, 1.0f, 0.0f ) ).y - dirtTileSize / 2.0f;

        for( int y = -1; y <= 1; y++ )
        {
            for( int x = -1; x <= 1; x++ )
            {
                ConstructTile( new Vector2Int( x, y ) );
            }
        }
    }

    private void Update()
    {
        var cameraPos = mainCamera.ViewportToWorldPoint( mainCamera.transform.position );
        var cameraSize = mainCamera.ViewportToWorldPoint( new Vector3( 1.0f, 1.0f, 0.0f ) );
        var bounds = new Rect( cameraPos.x, cameraPos.y, cameraSize.x, cameraSize.y );

        for( int x = -1; x <= 1; x++ )
        {
            var pos = new Vector2Int( ( int )( cameraPos.x / dirtTileSize ) + x, Mathf.CeilToInt( bounds.y / dirtTileSize ) );
            if( !tiles.ContainsKey( pos ) )
                ConstructTile( pos );
        }
    }

    private void ConstructTile( Vector2Int pos )
    {
        var newTile = Instantiate( dirtTilePrefab, new Vector3( ( float )pos.x * dirtTileSize, groundHeight + pos.y * dirtTileSize, 0.0f ), Quaternion.identity );
        tiles.Add( pos, newTile );
    }
}
