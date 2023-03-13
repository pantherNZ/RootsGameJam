using System;
using System.Collections.Generic;
using UnityEngine;

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
