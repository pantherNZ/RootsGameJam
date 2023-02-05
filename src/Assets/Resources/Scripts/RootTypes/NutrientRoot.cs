using UnityEngine;

public class NutrientRoot : BaseRoot
{
    [SerializeField] float gatherTimeSec = 1.0f;
    [SerializeField] int waterGain = 5;
    [SerializeField] int nutrientGain = 5;

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
