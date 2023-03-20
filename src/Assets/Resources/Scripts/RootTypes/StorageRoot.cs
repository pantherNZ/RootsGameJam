using UnityEngine;

public class StorageRoot : BaseRoot
{
    public override void OnPlacement()
    {
        base.OnPlacement();

        EventSystem.Instance.TriggerEvent( new ModifyStorageEvent()
        {
            res = type.storage,
        } );
    }

    private void OnDisable()
    {
        EventSystem.Instance.TriggerEvent( new ModifyStorageEvent()
        {
            res = -type.storage,
        } );
    }
}
