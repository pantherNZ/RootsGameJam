using UnityEngine;

public class NutrientRoot : BaseRoot
{
    const float gatherTimeSec = 1.0f;
    [SerializeField] int waterGain = 5;
    [SerializeField] int nutrientGain = 5;

    private void Start()
    {
        Utility.FunctionTimer.CreateTimer( gatherTimeSec, GainResources, "RootNutrient" + gameObject.GetInstanceID(), true );
    }

    private void OnDestroy()
    {
        Utility.FunctionTimer.StopTimer( "RootNutrient" + gameObject.GetInstanceID() );
    }

    private void GainResources()
    {
        if( !isPlaced )
            return;

        EventSystem.Instance.TriggerEvent( new GainResourcesEvent()
        {
            water = WaterToGain(),
            nutrients = NutrientsToGain()
        } );
    }

    protected virtual int WaterToGain()
    {
        return waterGain;
    }

    protected virtual int NutrientsToGain()
    {
        return nutrientGain;
    }
}
