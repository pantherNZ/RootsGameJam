using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameOverUI : EventReceiverInstance
{
    [SerializeField] GameObject ui;
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
            ui.SetActive( true );

            foreach( var letter in letters )
                letter.Show();
        }
    }

    public void Hide()
    {
        float maxTime = 0.0f;
        foreach( var letter in letters )
        {
            float time = letter.GenerateTime();
            maxTime = Mathf.Max( time, maxTime );
            letter.Hide( time );
        }

        Utility.FunctionTimer.CreateTimer( maxTime, () =>
        {
            ui.SetActive( false );
        } );

        EventSystem.Instance.TriggerEvent( new ResetGameEvent() );
    }
}
