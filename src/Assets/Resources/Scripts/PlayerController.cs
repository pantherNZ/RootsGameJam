using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Root
{
    public GameObject obj;
    public SpriteRenderer sprite;
    public Collider2D collision;
    public List<Root> children = new List<Root>();
    public Root parent;
    public Color baseCol;
}

public class PlayerController : EventReceiverInstance
{
    // Resources
    [SerializeField] int nutrients;
    [SerializeField] int nutrientsMax;
    [SerializeField] int water;
    [SerializeField] int waterMax;
    [SerializeField] int level;
    [SerializeField] bool canPlaceRoots;
    [SerializeField] List<BaseRoot> rootTypes;
    [SerializeField] GameObject rootSelectionUI;
    [SerializeField] GameObject gameUIRoot;
    [SerializeField] CanvasGroup mainMenuPanel;
    [SerializeField] GameObject rootSelectionUIContent;
    [SerializeField] GameObject rootEntryUIPrefab;
    [SerializeField] ValueBarUI waterBarUI;
    [SerializeField] ValueBarUI foodBarUI;
    [SerializeField] ValueBarUI healthBarUI;
    [SerializeField] TMPro.TextMeshProUGUI levelUI;
    [SerializeField] float menuFadeOutTime = 1.0f;
    [SerializeField] float rootScale = 1.0f;
    [SerializeField] bool inMenu = true;
    [SerializeField] Color invalidPlacementColour = Color.red;
    [SerializeField] float rootMaxAngleDegrees = 150.0f;

    private Camera mainCamera;
    private List<MainMenuLetter> mainMenuLetters = new List<MainMenuLetter>();
    private RootConnection currentConnection;

    private List<Root> roots = new List<Root>();
    public Root newRoot;

    protected override void Start()
    {
        base.Start();

        mainCamera = Camera.main;

        gameUIRoot.SetActive( !inMenu );
        rootSelectionUI.SetActive( !inMenu );
        mainMenuPanel.gameObject.SetActive( inMenu );
        ChangeLevel( 1 );

        // Find root connections and list to their click event
        var rootConnections = FindObjectsOfType<RootConnection>();
        mainMenuLetters = FindObjectsOfType<MainMenuLetter>().ToList();

        ListenToConnections( rootConnections );

        foreach( var type in rootTypes )
        {
            var entry = Instantiate( rootEntryUIPrefab, rootSelectionUIContent.transform );
            var typeLocal = type;
            entry.GetComponent<Button>().onClick.AddListener( () =>
            {
                RootUIOptionClicked( type );
            } );
        }

        waterBarUI.SetValue( water, waterMax );
        foodBarUI.SetValue( nutrients, nutrientsMax );
    }

    void ListenToConnections( RootConnection[] connections )
    { 
        foreach( var connection in connections )
        {
            var connectionLocal = connection;
            connection.GetComponent<EventDispatcherV2>().OnPointerDownEvent.AddListener( x =>
                     {
                         ConnectionClicked( connectionLocal );
                     } );
        }
    }

    void ConnectionClicked( RootConnection connection )
    {
        if( inMenu )
            return;

        // Show UI
        rootSelectionUI.SetActive( true );
        ( rootSelectionUI.transform as RectTransform ).anchoredPosition = mainCamera.WorldToScreenPoint( connection.transform.position );
        currentConnection = connection;

        InputPriority.Instance.Cancel( "rootSelectionUI" );
    }

    void RootUIOptionClicked( BaseRoot rootType )
    {
        rootSelectionUI.SetActive( false );

        var rootObj = Instantiate( rootType, currentConnection.transform );

        newRoot = new Root()
        {
            obj = rootObj.gameObject,
            sprite = rootObj.GetComponentInChildren<SpriteRenderer>(),
            collision = rootObj.GetComponentInChildren<Collider2D>(),
            parent = currentConnection.parent,
        };

        newRoot.baseCol = newRoot.sprite.color;

        newRoot.obj.transform.localPosition = new Vector3();
        newRoot.obj.transform.localScale = new Vector3( rootScale, rootScale, rootScale );
    }

    public override void OnEventReceived( IBaseEvent e )
    {;
        if( e is GainResourcesEvent gain )
        {
            water += gain.water;
            nutrients += gain.nutrients;

            waterBarUI.SetValue( water, waterMax );
            foodBarUI.SetValue( nutrients, nutrientsMax );
        }
    }

    void Update()
    {
        if( inMenu )
        {
            if( Input.anyKeyDown )
            {
                HideMenu();
            }
        }
        else
        {
            if( newRoot != null )
            {
                var mousePos = mainCamera.ScreenToWorldPoint( Utility.GetMouseOrTouchPos() );
                var direction = ( mousePos - currentConnection.transform.position ).normalized.ToVector2();
                newRoot.obj.transform.rotation = Quaternion.Euler( 0.0f, 0.0f, -direction.Angle() + 90.0f );

                // Collision and angle check
                List<Collider2D> colliderList = new List<Collider2D>();
                bool isFirstConnection = currentConnection.parent == null;
                newRoot.collision.OverlapCollider( new ContactFilter2D().NoFilter(), colliderList );
                bool validPlacement = Vector3.Angle( direction, currentConnection.transform.right ) <= rootMaxAngleDegrees / 2.0f
                    && colliderList.All( x =>
                    {
                        return x.GetComponent<RootConnection>() != null ||
                            ( isFirstConnection && x.GetComponent<Tree>() != null );
                    } );

                newRoot.sprite.color = validPlacement ? newRoot.baseCol : invalidPlacementColour;

                // Place
                if( validPlacement && Input.GetMouseButtonDown( 0 ) )
                {
                    roots.Add( newRoot );
                    ListenToConnections( newRoot.obj.GetComponentsInChildren<RootConnection>() );
                    currentConnection.GetComponent<EventDispatcherV2>().OnPointerDownEvent.RemoveAllListeners();
                    newRoot = null;
                }
                // Cancel
                else if( Input.GetMouseButtonDown( 1 ) )
                {
                    newRoot.obj.Destroy();
                    newRoot = null;
                }
            }
            else if( currentConnection != null )
            {
                InputPriority.Instance.Request( () => Input.GetMouseButtonDown( 0 ) || Input.GetMouseButtonDown( 1 ), "rootSelectionUI", 0, () =>
                {
                    if( Input.GetMouseButtonDown( 1 ) || !Utility.IsPointerOverGameObject( rootSelectionUI ) )
                    {
                        currentConnection = null;
                        rootSelectionUI.SetActive( false );
                    }
                } );
            }
        }
    }

    void HideMenu()
    {
        inMenu = false;
        StartCoroutine( Utility.FadeToBlack( mainMenuPanel, menuFadeOutTime ) );

        foreach( var letter in mainMenuLetters )
            letter.Hide();

        gameUIRoot.SetActive( true );
    }

    void ShowMenu()
    {
        foreach( var letter in mainMenuLetters )
            letter.Show();

        gameUIRoot.SetActive( false );
        mainMenuPanel.gameObject.SetActive( true );
    }

    void ChangeLevel( int l )
    {
        level = l;
        levelUI.text = l.ToString();
    }
}
