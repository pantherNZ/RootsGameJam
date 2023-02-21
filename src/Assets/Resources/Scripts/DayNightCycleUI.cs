using UnityEngine;
using UnityEngine.UI;

public class DayNightCycleUI : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI label;
    [SerializeField] Image image;

    public void SetValue( float newValue, float max )
    {
        float percent = newValue / max;
        int hours = ( int )newValue;
        int minutes = ( int )( ( newValue - hours ) * 60.0f );
        bool isPm = hours >= 12.0f;
        hours = Utility.Mod( hours - 1, 12 ) + 1;

        label.text = string.Format( "{0}{1}:{2}{3}{4}",
            hours < 10 ? "0" : string.Empty,
            hours,
            minutes < 10 ? "0" : string.Empty,
            minutes,
            isPm ? "PM" : "AM" 
        );
    }
}
