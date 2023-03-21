using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameOverUI : EventReceiverInstance
{
    [SerializeField] CanvasGroup ui;
    private List<MainMenuLetter> letters = new List<MainMenuLetter>();

    protected override void Start()
    {
        base.Start();

        letters = GetComponentsInChildren<MainMenuLetter>( true ).ToList();
    }

    public override void OnEventReceived( IBaseEvent e )
    {
        if( e is GameOverEvent )
        {
            ui.SetVisibility( true );
            ui.gameObject.SetActive( true );

            foreach( var letter in letters )
                letter.Show();
        }
    }

    public void Hide()
    {
        var duration = GameController.Instance.Constants.menuFadeOutTime;

        foreach( var letter in letters )
            letter.Hide();

        StartCoroutine( Utility.FadeToBlack( ui, duration ) );

        Utility.FunctionTimer.CreateTimer( duration, () =>
        {
            ui.gameObject.SetActive( false );
        } );

        EventSystem.Instance.TriggerEvent( new ResetGameEvent() );
    }
}
