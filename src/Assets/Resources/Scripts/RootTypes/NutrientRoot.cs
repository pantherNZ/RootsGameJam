using UnityEngine;

public class NutrientRoot : BaseRoot
{
    [SerializeField] float gatherTimeSec = 1.0f;
    [SerializeField] Resource gain;

    public override void OnPlacement()
    {
        Utility.FunctionTimer.CreateTimer( gatherTimeSec, GainResources, "RootNutrient" + gameObject.GetInstanceID(), true );
    }

    private void OnDestroy()
    {
        Utility.FunctionTimer.StopTimer( "RootNutrient" + gameObject.GetInstanceID() );
    }

    private void GainResources()
    {
        EventSystem.Instance.TriggerEvent( new GainResourcesEvent()
        {
            res = gain
        } );
    }
}
