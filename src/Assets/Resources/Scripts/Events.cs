public class GainResourcesEvent : IBaseEvent 
{
    public int nutrients;
    public int water;
}

public class ModifyStorageEvent : IBaseEvent
{
    public int nutrients;
    public int water;
}
