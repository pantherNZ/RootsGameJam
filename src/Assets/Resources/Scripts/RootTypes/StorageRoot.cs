using UnityEngine;

public class StorageRoot : BaseRoot
{
    [SerializeField] int waterStorage = 100;
    [SerializeField] int nutrientStroage = 100;

    public override void OnPlacement()
    {
        EventSystem.Instance.TriggerEvent( new ModifyStorageEvent()
        {
            water = waterStorage,
            nutrients = nutrientStroage
        } );
    }

    private void OnDestroy()
    {
        EventSystem.Instance.TriggerEvent( new ModifyStorageEvent()
        {
            water = -waterStorage,
            nutrients = -nutrientStroage
        } );
    }
}
