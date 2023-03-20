using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu( fileName = "RootData", menuName = "ScriptableObjs/RootData" )]
public class RootData : ScriptableObject
{
    public string rootName;
    public string description;
    public Resource cost;
    public Texture2D icon;
    public int requiredLevel;
    public string placementTagRequirement;
    public float lengthMin;
    public float lengthMax;

    public float gatherTimeSec;
    public Resource consume;
    public Resource gain;
    public bool showGainToastUI;

    public Resource storage;
}
