using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ValueBarUI : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI label;
    [SerializeField] Image image;
    [SerializeField] float lerpTimeSec;
    private float? currentLerp;
    private Coroutine lerpCoroutine;

    public void SetValue( float newValue, float max )
    {
        if( lerpCoroutine != null )
            StopCoroutine( lerpCoroutine );

        if( !currentLerp.HasValue || newValue < currentLerp.Value )
            SetValueInternal( newValue, max );
        else
            lerpCoroutine = StartCoroutine( LerpToNewValue( newValue, max ) );
    }

    IEnumerator LerpToNewValue( float newValue, float max )
    {
        float t = 0.0f;

        while( t < 1.0f )
        {
            t += Time.deltaTime / lerpTimeSec;
            SetValueInternal( Mathf.Lerp( currentLerp.Value, newValue, t ), max );
            yield return null;
        }
    }

    void SetValueInternal( float current, float max )
    {
        currentLerp = current;
        if( label != null )
            label.text = string.Format( "{0}/{1}", Mathf.RoundToInt( current ), Mathf.RoundToInt( max ) );
        if( image != null )
            image.fillAmount = current / max;
    }
}
