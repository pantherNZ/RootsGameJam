using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnvObject
{
    public float spawnChancePercent;
    public float spawnChancePercentPerChunkDepth;
    public int numPerChunkMin;
    public int numPerChunkMax;
    public float minDistBetween;
    public RuleTile tilePrefab;
    public GameObject objPrefab;
    public int sizeInTilesMin;
    public int sizeInTilesMax;
}

[Serializable]
[CreateAssetMenu( fileName = "LevelData", menuName = "ScriptableObjs/LevelData" )]
public class LevelData : ScriptableObject
{
    public List<EnvObject> envObjects;
    public List<GameObject> grassObjects;
    public GameObject dirtChunkPrefab;
    public Vector2 dirtChunkSize;
    public float groundHeightOffset;
    public int levelSeed;
    public int initGenerateChunks = 1;
}
