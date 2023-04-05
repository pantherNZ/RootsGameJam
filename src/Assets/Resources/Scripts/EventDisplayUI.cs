using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventDisplayUI : EventReceiverInstance
{
    [SerializeField] GameObject pointerLeftIcon;
    [SerializeField] GameObject pointerRightIcon;
    [SerializeField] CanvasGroup leftUI;
    [SerializeField] CanvasGroup rightUI;
    [SerializeField] Transform leftSpawnPos;
    [SerializeField] Transform rightSpawnPos;
    [SerializeField] float displayTimeSec = 3.0f;
    [SerializeField] float fadeOutTimeSec = 1.0f;

    private Camera mainCam;

    protected override void Start()
    {
        base.Start();

        leftUI.gameObject.SetActive( false );
        rightUI.gameObject.SetActive( false );

        leftUI.alpha = 0.0f;
        rightUI.alpha = 0.0f;

        mainCam = Camera.main;
    }

    public override void OnEventReceived( IBaseEvent e )
    {
        if( e is SpawningStartedEvent spawningStarted )
        {
            switch( spawningStarted.spawnPos )
            {
                case SpawnPos.Left:
                    {
                        Left();
                        break;
                    }
                case SpawnPos.Right:
                    {
                        Right();
                        break;
                    }
                case SpawnPos.Alternating:
                case SpawnPos.Random:
                case SpawnPos.AlternatingPairs:
                    {
                        Left();
                        Right();
                        break;
                    }
            }
        }
    }

    private void Right()
    {
        Show( pointerRightIcon, rightUI, rightSpawnPos );
    }

    private void Left()
    {
        Show( pointerLeftIcon, leftUI, leftSpawnPos );
    }

    private void Show( GameObject pointerIcon, CanvasGroup ui, Transform spawnPos )
    {
        ui.gameObject.SetActive( true );
        this.FadeFromBlack( ui, fadeOutTimeSec );
        Utility.FunctionTimer.CreateTimer( fadeOutTimeSec + displayTimeSec, () =>
        {
            this.FadeToBlack( ui, fadeOutTimeSec );
        } );
        Utility.FunctionTimer.CreateTimer( displayTimeSec + fadeOutTimeSec * 2.0f, () =>
        {
            ui.gameObject.SetActive( false ); 
        } );

        Rotate( pointerIcon, spawnPos );
    }

    private void Rotate( GameObject pointerIcon, Transform spawnPos )
    {
        var iconPos = Camera.main.ScreenToWorldPoint( pointerIcon.transform.position );
        var direction = ( spawnPos.position - iconPos ).normalized.ToVector2();
        pointerIcon.transform.rotation = Quaternion.Euler( 0.0f, 0.0f, -direction.Angle() );
    }

    private void Update()
    {
        if( leftUI.isActiveAndEnabled )
            Rotate( pointerLeftIcon, leftSpawnPos );
        if( rightUI.isActiveAndEnabled )
            Rotate( pointerRightIcon, rightSpawnPos );
    }
}
