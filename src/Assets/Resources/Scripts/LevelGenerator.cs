using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] GameObject dirtTile;
    [SerializeField] int dirtTileSize;
    [SerializeField] float groundHeightOffset;

    private List<GameObject> tiles = new List<GameObject>();

    private void Start()
    {
        float height = Camera.main.ViewportToWorldPoint( new Vector3( 0.0f, 1.0f, 0.0f ) ).y- dirtTileSize / 2.0f;

        for( int y = -1; y <= 1; y++ )
        {
            for( int x = -1; x <= 1; x++ )
            {
                tiles.Add( Instantiate( dirtTile, new Vector3( ( float )x * dirtTileSize, height + y * dirtTileSize, 0.0f ), Quaternion.identity ) );
            }
        }
    }
}
