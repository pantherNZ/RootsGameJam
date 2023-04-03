using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu( fileName = "DefenceData", menuName = "ScriptableObjs/DefenceData" )]
public class DefenceData : BaseObjectData
{
    public float attackTimeSec;
    public int damage;
    public DamageType damageType;
    public float numProjectiles;
    public float projectileSpeed;
}
