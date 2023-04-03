using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct MonsterData
{
    public GameObject monsterPrefab;
    public int count;
    public float delayPerMonsterSec;
    public int damage;
    public int health;
    public float speed;
    public DamageType damageType;
    public Resource gainOnKill;
}

[Serializable]
public enum SpawnPos
{
    Left,
    Right,
    Random,
    Alternating,
    AlternatingPairs,
}

[Serializable]
public struct Modifier
{
    public float speedModifier;
    public float lifeModifier;
    public float damageModifier;

    public static Modifier operator *( Modifier a, Modifier b ) => new()
    {
        speedModifier = a.speedModifier * b.speedModifier,
        lifeModifier = a.lifeModifier * b.lifeModifier,
        damageModifier = a.damageModifier * b.damageModifier,
    };
}

[Serializable]
public struct Wave
{
    public float delaySec;
    public List<MonsterData> monsters;
    public Modifier modifier;
    public SpawnPos spawnSide;
}

[Serializable]
[CreateAssetMenu( fileName = "WaveData", menuName = "ScriptableObjs/WaveData" )]
public class WaveData : ScriptableObject
{
    public float dayLengthSec = 60;
    public int nightStartHour = 22;
    public int nightEndHour = 6;
    public int startHour = 6;
    public List<Wave> waves;
    public List<Wave> endlessWaves;
    public Modifier endlessPerDayModifier;
}
