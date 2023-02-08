using UnityEngine;

public class StorageRoot : BaseRoot
{
    [SerializeField] Resource storage;
    public override void OnPlacement()
    {
        EventSystem.Instance.TriggerEvent( new ModifyStorageEvent()
        {
            res = storage,
        } );
    }

    private void OnDestroy()
    {
        EventSystem.Instance.TriggerEvent( new ModifyStorageEvent()
        {
            res = -storage,
        } );
    }
}
