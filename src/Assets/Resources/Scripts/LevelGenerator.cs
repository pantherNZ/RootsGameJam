using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnvObject
{
    public int spawnChancePercent;
    public int numPerTileMin;
    public int numPerTileMax;
    public float minDistBetween;
    public GameObject obj;
}

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] GameObject dirtTilePrefab;
    [SerializeField] int dirtTileSize;
    [SerializeField] float groundHeightOffset;
    [SerializeField] float timeOfDay;
    [SerializeField] List<EnvObject> envObjects;
    [SerializeField] int levelSeed;

    private Camera mainCamera;
    private Dictionary<Vector2Int, GameObject> tiles = new Dictionary<Vector2Int, GameObject>();
    private float groundHeight;
    private Vector3? previousPos;
    private Tree tree;

    private void Start()
    {
        if( levelSeed == 0 )
            levelSeed = UnityEngine.Random.Range( 0, int.MaxValue - 1 );

        mainCamera = Camera.main;
        groundHeight = Camera.main.ViewportToWorldPoint( new Vector3( 0.0f, 1.0f, 0.0f ) ).y - dirtTileSize / 2.0f;
        tree = FindObjectOfType<Tree>();

        for( int y = -1; y <= 1; y++ )
        {
            for( int x = -1; x <= 1; x++ )
            {
                TryConstructTile( new Vector2Int( x, y ) );
            }
        }
    }

    private int RoundAwayFromZero( float f )
    {
        return ( int )( f + Mathf.Sign( f ) * 0.5f );
    }

    private int CeilAwayFromZero( float f )
    {
        return ( int )( f + Mathf.Sign( f ) );
    }

    private void Update()
    {
        var cameraPos = mainCamera.transform.position;
        var currentTile = new Vector2Int( ( int )( cameraPos.x / dirtTileSize ), ( int )( cameraPos.y / dirtTileSize ) );
        var cameraSize = mainCamera.ViewportToWorldPoint( new Vector3( 1.0f, 1.0f, 0.0f ) ) - cameraPos;
        var bounds = new Rect( cameraPos - cameraSize, cameraSize * 2.0f );

        for( int i = -1; i <= 1; i++ )
        {
            TryConstructTile( new Vector2Int( RoundAwayFromZero( bounds.xMin / dirtTileSize ), currentTile.y + i ) );
            TryConstructTile( new Vector2Int( RoundAwayFromZero( bounds.xMax / dirtTileSize ), currentTile.y + i ) );
            TryConstructTile( new Vector2Int( currentTile.x + i, RoundAwayFromZero( ( bounds.yMin - Mathf.Sign( groundHeight ) ) / dirtTileSize ) ) );
            TryConstructTile( new Vector2Int( currentTile.x + i, RoundAwayFromZero( ( bounds.yMax - Mathf.Sign( groundHeight ) ) / dirtTileSize ) ) );
        }

        TryConstructTile( new Vector2Int( RoundAwayFromZero( bounds.xMin / dirtTileSize ), RoundAwayFromZero( ( bounds.yMin - Mathf.Sign( groundHeight ) ) / dirtTileSize ) ) );
        TryConstructTile( new Vector2Int( RoundAwayFromZero( bounds.xMax / dirtTileSize ), RoundAwayFromZero( ( bounds.yMin - Mathf.Sign( groundHeight ) ) / dirtTileSize ) ) );
        TryConstructTile( new Vector2Int( RoundAwayFromZero( bounds.xMin / dirtTileSize ), RoundAwayFromZero( ( bounds.yMax - Mathf.Sign( groundHeight ) ) / dirtTileSize ) ) );
        TryConstructTile( new Vector2Int( RoundAwayFromZero( bounds.xMax / dirtTileSize ), RoundAwayFromZero( ( bounds.yMax - Mathf.Sign( groundHeight ) ) / dirtTileSize ) ) );
        TryConstructTile( currentTile );

        previousPos = cameraPos;
    }
    private float RandomFromHash( int lseed )
    {
        float seed = levelSeed * 0.4f + 23;
        seed *= 37 + lseed * 13;
        return xxHashSharp.xxHash.CalculateHash( BitConverter.GetBytes( seed ) ) / ( float )( uint.MaxValue - 1 );
    }

    private void TryConstructTile( Vector2Int pos )
    {
        if( tiles.ContainsKey( pos ) )
            return;

        var newTilePos = new Vector3( ( float )pos.x * dirtTileSize, groundHeight + pos.y * dirtTileSize, 0.0f );
        var newTile = Instantiate( dirtTilePrefab, newTilePos, Quaternion.identity );
        tiles.Add( pos, newTile );
        var seedOffset = pos.x * 13 + pos.y * 5;

        if( pos.x != 0 || pos.y != 0 )
        {
            foreach( var env in envObjects )
            {
                float random = RandomFromHash( 1211 + seedOffset );
                if( env.spawnChancePercent < random * 100.0f )
                    continue;

                int numEnvObj = env.numPerTileMin + ( int )( RandomFromHash( 91 + seedOffset ) * ( env.numPerTileMax - env.numPerTileMin ) );
                List<Pair<Vector2, float>> spawned = new List<Pair<Vector2, float>>();

                spawned.Add( new Pair<Vector2, float>( tree.transform.position, 3.0f ) );

                for( int i = 0; i < numEnvObj; ++i )
                {
                    int safety = 0;
                    Vector2? spawnPos = null;
                    do
                    {
                        Vector2 randPos = newTilePos.ToVector2() + new Vector2(
                            ( RandomFromHash( 99945 + seedOffset + safety * 69 ) - 0.5f ) * dirtTileSize,
                             ( RandomFromHash( 9345 + seedOffset + safety * 69 ) - 0.5f ) * dirtTileSize );
                        if( spawned.All( x => Vector2.SqrMagnitude( x.First - randPos ) >= Mathf.Pow( Mathf.Max( env.minDistBetween, x.Second ), 2.0f ) ) )
                            spawnPos = randPos;
                    }
                    while( safety++ < 50 && !spawnPos.HasValue );

                    if( spawnPos.HasValue )
                    {
                        spawned.Add( new Pair<Vector2, float>( spawnPos.Value, env.minDistBetween ) );
                        var envObj = Instantiate( env.obj, spawnPos.Value, Quaternion.Euler( 0.0f, 0.0f, UnityEngine.Random.value * Mathf.PI * 2.0f ) );
                        envObj.transform.parent = newTile.transform;
                    }
                }
            }
        }

        var currentTile = new Vector2Int(
            RoundAwayFromZero( mainCamera.transform.position.x / dirtTileSize ),
            RoundAwayFromZero( mainCamera.transform.position.y / dirtTileSize ) );

        bool removed = true;
        while( removed )
        {
            foreach( var (key, value) in tiles )
            {
                removed = Mathf.Abs( key.x - currentTile.x ) > 2 || Mathf.Abs( key.y - currentTile.y ) > 2;
                if( removed )
                {
                    tiles.Remove( key );
                    value.Destroy();
                    break;
                }
            }
        }
    }
}
