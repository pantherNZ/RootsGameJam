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
    [SerializeField] Resource currentResource;
    [SerializeField] Resource maxResource;
    int level;
    [SerializeField] GameObject rootSelectionUI;
    [SerializeField] GameObject gameUIRoot;
    [SerializeField] CanvasGroup mainMenuPanel;
    [SerializeField] GameObject rootSelectionUIContent;
    [SerializeField] RootEntryUI rootEntryUIPrefab;
    [SerializeField] ValueBarUI waterBarUI;
    [SerializeField] ValueBarUI foodBarUI;
    [SerializeField] ValueBarUI healthBarUI;
    [SerializeField] GameObject levelUpUI;
    [SerializeField] TMPro.TextMeshProUGUI levelUI;
    [SerializeField] float menuFadeOutTime = 1.0f;
    [SerializeField] float rootScale = 1.0f;
    public bool inMenu = true;
    [SerializeField] Color invalidPlacementColour = Color.red;
    [SerializeField] GameConstants gameConstants;

    private Camera mainCamera;
    private List<MainMenuLetter> mainMenuLetters = new List<MainMenuLetter>();
    private RootConnection currentConnection;

    private List<Root> roots = new List<Root>();
    public Root newRoot;
    private Color levelUpUIColour;

    protected override void Start()
    {
        base.Start();

        mainCamera = Camera.main;

        gameUIRoot.SetActive( !inMenu );
        rootSelectionUI.SetActive( !inMenu );
        mainMenuPanel.gameObject.SetActive( inMenu );
        ChangeLevel( 0 );

        currentResource = gameConstants.startingResource;
        maxResource = gameConstants.startingMaxResource;

        // Find root connections and list to their click event
        var rootConnections = FindObjectsOfType<RootConnection>();
        mainMenuLetters = FindObjectsOfType<MainMenuLetter>().ToList();

        ListenToConnections( rootConnections );
        SetupRootTypeUIOptions();

        waterBarUI.SetValue( currentResource.water, maxResource.water );
        foodBarUI.SetValue( currentResource.food, maxResource.food );

        levelUpUIColour = levelUpUI.GetComponentInChildren<Image>().color;
        ShowLevelUpPopup( false );
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
        if( inMenu || newRoot != null || currentConnection != null )
            return;

        // Show UI
        rootSelectionUI.SetActive( true );
        ( rootSelectionUI.transform as RectTransform ).anchoredPosition = mainCamera.WorldToScreenPoint( connection.transform.position );
        currentConnection = connection;

        foreach( Transform t in rootSelectionUIContent.transform )
        {
            t.GetComponent<RootEntryUI>().CheckEnabled( currentResource.water, currentResource.food );
        }

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

        var depth = currentConnection.parent == null ? 49 : currentConnection.parent.sprite.sortingOrder - 1;
        newRoot.baseCol = newRoot.sprite.color;
        newRoot.sprite.sortingOrder = depth;
        newRoot.obj.transform.localPosition = new Vector3();
        newRoot.obj.transform.localScale = new Vector3( rootScale, rootScale, rootScale );
    }

    public override void OnEventReceived( IBaseEvent e )
    {;
        if( e is GainResourcesEvent gain )
        {
            currentResource.water = Mathf.Min( maxResource.water, currentResource.water + gain.water );
            currentResource.food = Mathf.Min( maxResource.food, currentResource.food + gain.nutrients );
            UpdateBars();


            if( currentConnection != null )
            {
                foreach( Transform t in rootSelectionUIContent.transform )
                {
                    t.GetComponent<RootEntryUI>().CheckEnabled( currentResource.water, currentResource.food );
                }
            }
        }
        else if( e is ModifyStorageEvent modify )
        {
            maxResource.water += modify.water;
            maxResource.food += modify.nutrients;
            UpdateBars();
            ShowLevelUpPopup( levelUpUI.activeSelf );
        }
    }

    void UpdateBars()
    {
        waterBarUI.SetValue( currentResource.water, maxResource.water );
        foodBarUI.SetValue( currentResource.food, maxResource.food );
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
                //bool validAngle = ( Vector3.Angle( direction, currentConnection.transform.right ) <= currentConnection.rootMaxAngleDegrees / 2.0f ||
                //    ( currentConnection.allowBackwards && Vector3.Angle( direction, -currentConnection.transform.right ) > currentConnection.rootMaxAngleDegrees / 2.0f ) );
                bool validAngle = true;
                bool validCollision = colliderList.All( x =>
                    {
                        return x.gameObject == newRoot.obj ||
                            x.transform.IsChildOf( newRoot.obj.transform ) ||
                            x.GetComponent<RootConnection>() != null ||
                            ( isFirstConnection && x.GetComponent<Tree>() != null );
                    } );

                bool validPlacement = validCollision && validAngle;
                newRoot.sprite.color = validPlacement ? newRoot.baseCol : invalidPlacementColour;

                // Place
                if( validPlacement && Input.GetMouseButtonDown( 0 ) )
                {
                    ConfirmNewRoot();
                }
                // Cancel
                else if( Input.GetMouseButtonDown( 1 ) )
                {
                    newRoot.obj.Destroy();
                    newRoot = null;
                    currentConnection = null;
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

    void ConfirmNewRoot()
    {
        var rootType = newRoot.obj.GetComponent<BaseRoot>();
        currentResource.water -= rootType.waterCost;
        currentResource.food -= rootType.foodCost;

        waterBarUI.SetValue( currentResource.water, maxResource.food );
        foodBarUI.SetValue( currentResource.food, maxResource.food );

        rootType.isPlaced = true;
        rootType.OnPlacement();
        roots.Add( newRoot );
        var newConnections = newRoot.obj.GetComponentsInChildren<RootConnection>();
        ListenToConnections( newConnections );

        foreach( var connection in newConnections )
            connection.parent = newRoot;

        if( --currentConnection.numConnectionsAllowed == 0 )
            currentConnection.GetComponent<EventDispatcherV2>().OnPointerDownEvent.RemoveAllListeners();
        newRoot = null;
        currentConnection = null;
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
        levelUI.text = ( l + 1 ).ToString();
    }

    public void ShowLevelUpPopup( bool show )
    {
        levelUpUI.SetActive( show && !inMenu );
        var texts = levelUpUI.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
        texts[0].text = string.Format( "Next Tree Age: {0}\nCost: ", level + 2 );
        texts[1].text = GetLevelUpCost().food.ToString();
        texts[2].text = GetLevelUpCost().water.ToString();
        levelUpUI.GetComponentInChildren<Image>().color = CanLevelUp() ? levelUpUIColour : invalidPlacementColour;
    }

    private bool CanLevelUp()
    {
        return currentResource.water >= GetLevelUpCost().water && 
            currentResource.food >= GetLevelUpCost().food;
    }

    public void LevelUp()
    {
        if( !CanLevelUp() )
            return;

        currentResource.food -= GetLevelUpCost().food;
        currentResource.water -= GetLevelUpCost().water;
        UpdateBars();

        ChangeLevel( level + 1 );
        SetupRootTypeUIOptions();
        ShowLevelUpPopup( false );
    }

    public void SetupRootTypeUIOptions()
    {
        foreach( Transform t in rootSelectionUIContent.transform )
            t.DestroyGameObject();

        rootSelectionUIContent.transform.DetachChildren();

        foreach( var type in gameConstants.rootTypes )
        {
            if( level < type.requiredLevel )
                continue;

            var entry = Instantiate( rootEntryUIPrefab.gameObject, rootSelectionUIContent.transform );
            entry.GetComponent<RootEntryUI>().SetData( type );
            var typeLocal = type;
            entry.GetComponent<Button>().onClick.AddListener( () =>
            {
                RootUIOptionClicked( type );
            } );
        }
    }

    public Resource GetLevelUpCost()
    {
        return gameConstants.levelCosts[level];
    }
}
