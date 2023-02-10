using UnityEngine;

public class NutrientRoot : BaseRoot
{
    [SerializeField] float gatherTimeSec = 1.0f;
    [SerializeField] Resource consume;
    [SerializeField] Resource gain;
    [SerializeField] bool showGainToastUI;

    public override void OnPlacement()
    {
        Utility.FunctionTimer.CreateTimer( gatherTimeSec, GainResources, "RootNutrient" + gameObject.GetInstanceID(), true );
    }

    private void OnDisable()
    {
        Utility.FunctionTimer.StopTimer( "RootNutrient" + gameObject.GetInstanceID() );
    }

    private void GainResources()
    {
        EventSystem.Instance.TriggerEvent( new GainResourcesEvent()
        {
            res = gain - consume,
            originForUIDisplay = showGainToastUI ? transform.GetComponentInChildren<Renderer>().bounds.center : null
        } );
    }
}
