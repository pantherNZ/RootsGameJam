using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] Tilemap tileMap;
    [SerializeField] LevelData data;

    private Camera mainCamera;
    private Dictionary<Vector2Int, GameObject> chunks = new Dictionary<Vector2Int, GameObject>();
    private float groundHeight;
    private Tree tree;
    private int levelSeed;

    private void Start()
    {
        if( data.levelSeed == 0 )
            levelSeed = UnityEngine.Random.Range( 0, int.MaxValue - 1 );

        mainCamera = Camera.main;
        groundHeight = -( data.dirtChunkPrefab.transform as RectTransform ).rect.size.y / 2.0f;
        tree = FindObjectOfType<Tree>();

        for( int y = -data.initGenerateChunks; y <= data.initGenerateChunks; y++ )
        {
            for( int x = -data.initGenerateChunks; x <= data.initGenerateChunks; x++ )
            {
                TryConstructTile( new Vector2Int( x, y ) );
            }
        }
    }

    private void Update()
    {
        AddNewTiles();
        RemoveOldTiles();
    }

    private void AddNewTiles()
    {
        var cameraPos = mainCamera.transform.position;
        var currentTile = new Vector2Int( ( int )( cameraPos.x / data.dirtChunkSize.x ), ( int )( cameraPos.y / data.dirtChunkSize.y ) );
        var cameraSize = mainCamera.ViewportToWorldPoint( new Vector3( 1.0f, 1.0f, 0.0f ) ) - cameraPos;
        var bounds = new Rect( cameraPos - cameraSize, cameraSize * 2.0f );

        for( int i = -1; i <= 1; i++ )
        {
            TryConstructTile( new Vector2Int( RoundAwayFromZero( bounds.xMin / data.dirtChunkSize.x ), currentTile.y + i ) );
            TryConstructTile( new Vector2Int( RoundAwayFromZero( bounds.xMax / data.dirtChunkSize.x ), currentTile.y + i ) );
            TryConstructTile( new Vector2Int( currentTile.x + i, RoundAwayFromZero( ( bounds.yMin - Mathf.Sign( groundHeight ) ) / data.dirtChunkSize.y ) ) );
            TryConstructTile( new Vector2Int( currentTile.x + i, RoundAwayFromZero( ( bounds.yMax - Mathf.Sign( groundHeight ) ) / data.dirtChunkSize.y ) ) );
        }

        TryConstructTile( new Vector2Int(
            RoundAwayFromZero( bounds.xMin / data.dirtChunkSize.x ),
            RoundAwayFromZero( ( bounds.yMin - Mathf.Sign( groundHeight ) ) / data.dirtChunkSize.y ) ) );
        TryConstructTile( new Vector2Int(
            RoundAwayFromZero( bounds.xMax / data.dirtChunkSize.x ),
            RoundAwayFromZero( ( bounds.yMin - Mathf.Sign( groundHeight ) ) / data.dirtChunkSize.y ) ) );
        TryConstructTile( new Vector2Int(
            RoundAwayFromZero( bounds.xMin / data.dirtChunkSize.x ),
            RoundAwayFromZero( ( bounds.yMax - Mathf.Sign( groundHeight ) ) / data.dirtChunkSize.y ) ) );
        TryConstructTile( new Vector2Int(
            RoundAwayFromZero( bounds.xMax / data.dirtChunkSize.x ),
            RoundAwayFromZero( ( bounds.yMax - Mathf.Sign( groundHeight ) ) / data.dirtChunkSize.y ) ) );
        TryConstructTile( currentTile );
    }

    private void TryConstructTile( Vector2Int chunk )
    {
        if( chunk.y > 0 || chunks.ContainsKey( chunk ) || ( chunk.x == 0 && chunk.y == 0 ) )
            return;

        var newChunkPos = new Vector3( chunk.x * data.dirtChunkSize.x, groundHeight + chunk.y * data.dirtChunkSize.y, 0.0f );
        var newChunk = Instantiate( data.dirtChunkPrefab, newChunkPos, Quaternion.identity );
        chunks.Add( chunk, newChunk );

        ConstructEnvObjects( chunk, newChunk );

        ConstructGrass( chunk, newChunk );
    }

    private void ConstructEnvObjects( Vector2Int chunk, GameObject newChunk )
    {
        var seedOffset = chunk.x * 13 + chunk.y * 5;

        if( chunk.y >= 0 || ( chunk.x == 0 && chunk.y == 0 ) )
            return;

        foreach( var env in data.envObjects )
        {
            seedOffset += 11;

            float random = RandomFromHash( 1211 + seedOffset );
            float spawnChance = env.spawnChancePercent + env.spawnChancePercentPerChunkDepth * Math.Abs( chunk.y );
            if( spawnChance < random * 100.0f )
                continue;

            int numEnvObj = env.numPerChunkMin + ( int )( RandomFromHash( 91 + seedOffset ) * ( env.numPerChunkMax - env.numPerChunkMin ) );
            List<Pair<Vector2, float>> spawned = new()
            {
                new Pair<Vector2, float>( tree.transform.position, 3.0f )
            };

            for( int i = 0; i < numEnvObj; ++i )
            {
                int safety = 0;
                Vector2? spawnPos = null;
                do
                {
                    Vector2 randPos = newChunk.transform.position.ToVector2() + new Vector2(
                        ( RandomFromHash( 99945 + seedOffset + safety * 69 ) - 0.5f ) * data.dirtChunkSize.x,
                        ( RandomFromHash( 9345 + seedOffset + safety * 69 ) - 0.5f ) * data.dirtChunkSize.y );
                    if( spawned.All( x => Vector2.SqrMagnitude( x.First - randPos ) >= Mathf.Pow( Mathf.Max( env.minDistBetween, x.Second ), 2.0f ) ) )
                        spawnPos = randPos;
                }
                while( safety++ < 50 && !spawnPos.HasValue );

                if( spawnPos.HasValue )
                {
                    spawned.Add( new Pair<Vector2, float>( spawnPos.Value, env.minDistBetween ) );

                    if( env.objPrefab != null )
                    {
                        var envObj = Instantiate( env.objPrefab, spawnPos.Value, Quaternion.Euler( 0.0f, 0.0f, UnityEngine.Random.value * Mathf.PI * 2.0f ) );
                        envObj.transform.parent = newChunk.transform;
                    }
                    else
                    {
                        int numTiles = env.sizeInTilesMin + ( int )( RandomFromHash( 123331 + seedOffset ) * ( env.sizeInTilesMax - env.sizeInTilesMin ) );
                        HashSet<Vector3Int> unvisited = new HashSet<Vector3Int>
                        {
                            tileMap.WorldToCell( spawnPos.Value )
                        };
                        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

                        while( numTiles-- > 0 && unvisited.Count > 0 )
                        {
                            var cell = RandomItem( 991 + seedOffset + numTiles, unvisited );
                            unvisited.Remove( cell );
                            visited.Add( cell );
                            tileMap.SetTile( cell, env.tilePrefab );
                            tileMap.SetTile( cell + new Vector3Int( 1, 0, 0 ), env.tilePrefab );
                            tileMap.SetTile( cell + new Vector3Int( 0, 1, 0 ), env.tilePrefab );
                            tileMap.SetTile( cell + new Vector3Int( 1, 1, 0 ), env.tilePrefab );
                            var horiz = RandomBoolFromHash( 211 + numTiles + seedOffset );
                            var dir = RandomBoolFromHash( 2191 + numTiles + seedOffset );
                            if( !visited.Contains( cell + new Vector3Int( 2, 0, 0 ) ) ) unvisited.Add( cell + new Vector3Int( 2, 0, 0 ) );
                            if( !visited.Contains( cell + new Vector3Int( -2, 0, 0 ) ) ) unvisited.Add( cell + new Vector3Int( -2, 0, 0 ) );
                            if( !visited.Contains( cell + new Vector3Int( 0, 2, 0 ) ) ) unvisited.Add( cell + new Vector3Int( 0, 2, 0 ) );
                            if( !visited.Contains( cell + new Vector3Int( 0, -2, 0 ) ) ) unvisited.Add( cell + new Vector3Int( 0, -2, 0 ) );
                        }
                    }
                }
            }
        }
    }

    private void ConstructGrass( Vector2Int tile, GameObject newTile )
    {
        // Grass height at y == 0, ignore root tile
        if( tile.y != 0 || tile.x == 0 )
            return;

        var grass = Instantiate( data.grassObjects.RandomItem(), new Vector3( newTile.transform.position.x, 0.0f, 0.0f ), Quaternion.identity );
        grass.transform.parent = newTile.transform.parent;
    }

    private void RemoveOldTiles( )
    {
        var currentTile = new Vector2Int(
            RoundAwayFromZero( mainCamera.transform.position.x / data.dirtChunkSize.x ),
            RoundAwayFromZero( mainCamera.transform.position.y / data.dirtChunkSize.y ) );

        bool removed = true;
        while( removed && !chunks.IsEmpty() )
        {
            foreach( var (key, value) in chunks )
            {
                removed = Mathf.Abs( key.x - currentTile.x ) > data.initGenerateChunks + 1 || 
                    Mathf.Abs( key.y - currentTile.y ) > data.initGenerateChunks + 1;
                if( removed )
                {
                    chunks.Remove( key );
                    var botLeft = ( value.transform as RectTransform ).GetWorldRect().BottomLeft();
                    var topRight = ( value.transform as RectTransform ).GetWorldRect().TopRight();

                    for( float x = botLeft.x; x <= topRight.x; x++ )
                    {
                        for( float y = botLeft.y; y <= topRight.y; y++ )
                        {
                            tileMap.SetTile( tileMap.WorldToCell( new Vector3( x, y, 0.0f ) ), null );
                        }
                    }
                    
                    value.Destroy();
                    break;
                }
            }
        }
    }

    // Utility ---------------
    private int RoundAwayFromZero( float f )
    {
        return ( int )( f + Mathf.Sign( f ) * 0.5f );
    }

    private float RandomFromHash( int lseed )
    {
        float seed = levelSeed * 0.4f + 23;
        seed *= 37 + lseed * 13;
        return xxHashSharp.xxHash.CalculateHash( BitConverter.GetBytes( seed ) ) / ( float )( uint.MaxValue - 1 );
    }

    private bool RandomBoolFromHash( int lseed )
    {
        float seed = levelSeed * 0.4f + 23;
        seed *= 37 + lseed * 13;
        return ( xxHashSharp.xxHash.CalculateHash( BitConverter.GetBytes( seed ) ) & 1 ) == 1;
    }

    private TKey RandomItem<TKey>( int lseed, HashSet<TKey> dict )
    {
        float seed = levelSeed * 0.4f + 23;
        seed *= 37 + lseed * 13;
        var rand = xxHashSharp.xxHash.CalculateHash( BitConverter.GetBytes( seed ) );
        int idx = Utility.Mod( ( int )rand, dict.Count );
        return dict.ElementAt( idx );
    }
    // Utility ---------------
}
