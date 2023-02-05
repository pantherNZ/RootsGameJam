using UnityEngine;

public class BaseRoot : MonoBehaviour
{
    public string rootName;
    public string description;
    public int waterCost;
    public int foodCost;
    public Texture2D icon;
    [HideInInspector]
    public bool isPlaced;
    public int requiredLevel;

    public virtual void OnPlacement() { }
}