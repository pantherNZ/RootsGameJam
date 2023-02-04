using UnityEngine;
using UnityEngine.UI;

public class ValueBarUI : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI label;
    [SerializeField] Image image;

    public void SetValue( float current, float max )
    {
        if( label != null )
            label.text = string.Format( "{0}/{1}", Mathf.RoundToInt( current ), Mathf.RoundToInt( max ) );
        if( image != null ) 
            image.fillAmount = current / max;
    }
}
