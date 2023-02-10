using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Resource
{
    public int food;
    public int water;
    public int energy;

    public static Resource operator -( Resource a ) => new()
    {
        food = -a.food,
        water = -a.water,
        energy = -a.energy,
    };

    public static Resource operator +( Resource a, Resource b ) => new()
    {
         food = a.food + b.food,
         water = a.water + b.water,
         energy = a.energy + b.energy
    };

    public static Resource operator -( Resource a, Resource b ) => a + ( -b );

    public bool IsValid()
    {
        return food >= 0 && water >= 0 && energy >= 0;
    }
}

[Serializable]
[CreateAssetMenu( fileName = "GameConstants", menuName = "ScriptableObjs/GameConstants" )]
public class GameConstants : ScriptableObject
{
    public List<Resource> levelCosts;
    public Resource startingResource;
    public Resource startingMaxResource;
    public List<BaseRoot> rootTypes;
    public float menuSlerpSpeed;
}
