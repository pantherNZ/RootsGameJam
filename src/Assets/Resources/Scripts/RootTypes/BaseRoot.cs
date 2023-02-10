using UnityEngine;

public class BaseRoot : MonoBehaviour
{
    public string rootName;
    public string description;
    public Resource cost;
    public Texture2D icon;
    [HideInInspector]
    public bool isPlaced;
    public int requiredLevel;
    public string placementTagRequirement;
    public Collider2D placementRequirement;

    public virtual void OnPlacement() { }
}