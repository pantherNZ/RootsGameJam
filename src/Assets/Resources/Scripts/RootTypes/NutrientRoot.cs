public class NutrientRoot : BaseRoot
{
    const float gatherTimeSec = 1.0f;
    const int waterGain = 5;
    const int nutrientGain = 5;

    private void Start()
    {
        Utility.FunctionTimer.CreateTimer( gatherTimeSec, GainResources, "RootNutrient" + gameObject.GetInstanceID(), true );
    }

    private void OnDestroy()
    {
        Utility.FunctionTimer.StopTimer( "RootNutrient" + gameObject.GetInstanceID() );
    }

    public override string GetName()
    {
        return "Nutrient Root";
    }

    public override string GetDescription()
    {
        return "Slowly gathers water and nutrients from the ground";
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
