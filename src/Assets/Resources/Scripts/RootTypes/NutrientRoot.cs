using UnityEngine;

public class NutrientRoot : BaseRoot
{
    private Vector3 center;

    public override void OnPlacement()
    {
        base.OnPlacement();

        center = transform.GetComponentInChildren<Renderer>().bounds.center;
        Utility.FunctionTimer.CreateTimer( type.gatherTimeSec, GainResources, "RootNutrient" + gameObject.GetInstanceID(), true );
    }

    private void OnDisable()
    {
        Utility.FunctionTimer.StopTimer( "RootNutrient" + gameObject.GetInstanceID() );
    }

    private void GainResources()
    {
        EventSystem.Instance.TriggerEvent( new GainResourcesEvent()
        {
            res = type.gain - type.consume,
            originForUIDisplay = type.showGainToastUI ? center : null
        } );
    }
}
