using UnityEngine;

public class GainResourcesEvent : IBaseEvent 
{
    public Resource res;
    public Vector3? originForUIDisplay;
}

public class ModifyStorageEvent : IBaseEvent
{
    public Resource res;
}

public class GameOverEvent : IBaseEvent
{
}

public class ResetGameEvent : IBaseEvent
{
}