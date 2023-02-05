using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Resource
{
    public int food;
    public int water;
}

[Serializable]
[CreateAssetMenu( fileName = "GameConstants", menuName = "ScriptableObjs/GameConstants" )]
public class GameConstants : ScriptableObject
{
    public List<Resource> levelCosts;
    public Resource startingResource;
    public Resource startingMaxResource;
}
