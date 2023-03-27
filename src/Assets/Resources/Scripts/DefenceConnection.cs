using UnityEngine;
using UnityEngine.UI;

[RequireComponent( typeof( EventDispatcherV2 ) )]
public class DefenceConnection : EventReceiverInstance
{
    [SerializeField] GameObject defenceSelectionUI;
    [SerializeField] GameObject defenceSelectionUIContent;
    [SerializeField] GameObject defenceEntryUIPrefab;

    private Camera mainCamera;

    private GameObject attachedDefence;

    protected override void Start()
    {
        base.Start();

        var eventDispatcher = GetComponent<EventDispatcherV2>();
        eventDispatcher.OnPointerDownEvent.RemoveAllListeners();
        eventDispatcher.OnPointerDownEvent.AddListener( x =>
        {
            OnClicked();
        } );

        mainCamera = Camera.main;
    }

    private void SetupRootTypeUIOptions()
    {
        foreach( Transform t in defenceSelectionUIContent.transform )
            t.DestroyGameObject();

        defenceSelectionUIContent.transform.DetachChildren();

        foreach( var type in GameController.Instance.Constants.defenceTypes )
        {
            if( GameController.Instance.player.Level < type.type.requiredLevel )
                continue;
            var entry = Instantiate( defenceEntryUIPrefab, defenceSelectionUIContent.transform );
            entry.GetComponent<DefenceEntryUI>().SetData( type.type );
            var typeLocal = type;
            entry.GetComponent<Button>().onClick.AddListener( () =>
            {
                DefenceUIOptionClicked( type );
            } );
        }
    }

    private void OnClicked()
    {
        // Show UI 
        SetupRootTypeUIOptions();
        defenceSelectionUI.SetActive( true );
        ( defenceSelectionUI.transform as RectTransform ).anchoredPosition = mainCamera.WorldToScreenPoint( transform.position );

        foreach( Transform t in defenceSelectionUIContent.transform )
        {
            t.GetComponent<DefenceEntryUI>().CheckEnabled( GameController.Instance.player.CurrentResource );
        }

        InputPriority.Instance.Cancel( "defenceSelectionUI" );
    }

    private void DefenceUIOptionClicked( BaseDefence type )
    {
        defenceSelectionUI.SetActive( false );

        attachedDefence = Instantiate( type ).gameObject;
        attachedDefence.transform.position = transform.position;
        attachedDefence.transform.rotation = transform.rotation;
    }

    public override void OnEventReceived( IBaseEvent e )
    {
        if( e is ResetGameEvent )
        {
            if( attachedDefence != null )
                attachedDefence.Destroy();
        }
    }
}