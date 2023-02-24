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
    public PathCreation.Examples.RoadMeshCreator spline;
}

public enum GameState
{
    Menu,
    Game,
    GameOver,
}

public class PlayerController : EventReceiverInstance
{
    // Resources
    [SerializeField] Resource currentResource;
    [SerializeField] Resource maxResource;
    int level;
    [SerializeField] GameObject rootSelectionUI;
    [SerializeField] GameObject gameUIRoot;
    [SerializeField] GameObject worldUIRoot;
    [SerializeField] CanvasGroup mainMenuPanel;
    [SerializeField] GameObject rootSelectionUIContent;
    [SerializeField] RootEntryUI rootEntryUIPrefab;
    [SerializeField] ValueBarUI waterBarUI;
    [SerializeField] ValueBarUI foodBarUI;
    [SerializeField] ValueBarUI energyBarUI;
    [SerializeField] GameObject levelUpUI;
    [SerializeField] TMPro.TextMeshProUGUI levelUI;
    [SerializeField] GameObject gainResourceUIPrefab;
    [SerializeField] float menuFadeOutTime = 1.0f;
    [SerializeField] float rootScale = 1.0f;
    [SerializeField] Color invalidPlacementColour = Color.red;
    [SerializeField] GameConstants gameConstants;
    [SerializeField] LayerMask allowPlacementLayer;
    [SerializeField] GameObject gameOverScreen;
    public GameState gameState = GameState.Menu;

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

        UpdateScreens();
        ChangeLevel( 0 );

        currentResource = gameConstants.startingResource;
        maxResource = gameConstants.startingMaxResource;

        // Find root connections and list to their click event
        var rootConnections = FindObjectsOfType<RootConnection>();
        mainMenuLetters = FindObjectsOfType<MainMenuLetter>().ToList();

        ListenToConnections( rootConnections );
        SetupRootTypeUIOptions();

        UpdateBars();

        levelUpUIColour = levelUpUI.GetComponentInChildren<Image>().color;
        ShowLevelUpPopup( false );
    }

    private void UpdateScreens()
    {
        gameUIRoot.SetActive( gameState == GameState.Game );
        rootSelectionUI.SetActive( gameState == GameState.Game );
        mainMenuPanel.gameObject.SetActive( gameState == GameState.Menu );
        gameUIRoot.SetActive( gameState == GameState.GameOver );
        levelUpUI.SetActive( false );
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
        if( gameState != GameState.Game || newRoot != null || currentConnection != null )
            return;

        // Show UI
        rootSelectionUI.SetActive( true );
        ( rootSelectionUI.transform as RectTransform ).anchoredPosition = mainCamera.WorldToScreenPoint( connection.transform.position );
        currentConnection = connection;

        foreach( Transform t in rootSelectionUIContent.transform )
        {
            t.GetComponent<RootEntryUI>().CheckEnabled( currentResource );
        }

        InputPriority.Instance.Cancel( "rootSelectionUI" );
    }

    void RootUIOptionClicked( BaseRoot rootType )
    {
        rootSelectionUI.SetActive( false );

        if( rootType.GetComponent<PathCreation.Examples.RoadMeshCreator>() )
        {
            PathCreation.Examples.RoadMeshCreator spline;

            if( currentConnection.parent == null )
            {
                var rootObj = Instantiate( rootType, currentConnection.transform );
                spline = rootObj.GetComponent<PathCreation.Examples.RoadMeshCreator>();
            }
            else
            {
                spline = currentConnection.parent.obj.GetComponent<PathCreation.Examples.RoadMeshCreator>();
                var pos = currentConnection.transform.position + currentConnection.transform.forward * 2.0f;
                spline.pathCreator.bezierPath.AddSegmentToEnd( pos );
            }

            newRoot = new Root()
            {
                obj = spline.gameObject,
                collision = spline.gameObject.GetComponentInChildren<Collider2D>(),
                spline = spline,
                parent = currentConnection.parent,
            };
        }
        else
        {
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
    }

    public override void OnEventReceived( IBaseEvent e )
    {;
        if( e is GainResourcesEvent gain )
        {
            if( ( maxResource + gain.res ).IsValid() )
            {
                currentResource.water = Mathf.Min( maxResource.water, currentResource.water + gain.res.water );
                currentResource.food = Mathf.Min( maxResource.food, currentResource.food + gain.res.food );
                currentResource.energy = Mathf.Min( maxResource.energy, currentResource.energy + gain.res.energy );
                UpdateBars();

                if( currentConnection != null )
                {
                    foreach( Transform t in rootSelectionUIContent.transform )
                    {
                        t.GetComponent<RootEntryUI>().CheckEnabled( currentResource );
                    }
                }

                if( gain.originForUIDisplay.HasValue )
                {
                    var newToast = Instantiate( gainResourceUIPrefab, worldUIRoot.transform );
                    newToast.transform.position = gain.originForUIDisplay.Value;

                    this.InterpolatePosition( newToast.transform, newToast.transform.position + new Vector3( 0.0f, 1.0f, 0.0f ), 1.0f );
                    this.FadeToBlack( newToast.GetComponent<CanvasGroup>(), 1.0f );
                }
            }
        }
        else if( e is ModifyStorageEvent modify )
        {
            if( ( maxResource + modify.res ).IsValid() )
            {
                maxResource += modify.res;
                UpdateBars();
                ShowLevelUpPopup( levelUpUI.activeSelf );
            }
        }
        else if( e is GameOverEvent )
        {
            gameState = GameState.GameOver;
            UpdateScreens();
        }
    }

    void UpdateBars()
    {
        waterBarUI.SetValue( currentResource.water, maxResource.water );
        foodBarUI.SetValue( currentResource.food, maxResource.food );
        energyBarUI.SetValue( currentResource.energy, maxResource.energy );
    }

    void Update()
    {
        if( gameState == GameState.Menu )
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
                ProcessRootPlacement();
            }
            else if( currentConnection != null )
            {
                ProcessSelectionUI();
            }
        }
    }

    private void ProcessRootPlacement()
    {
        var mousePos = mainCamera.ScreenToWorldPoint( Utility.GetMouseOrTouchPos() );

        if( newRoot.spline != null )
        {
            var path = newRoot.spline.pathCreator.bezierPath;
            path.MovePoint( path.NumPoints - 1, mousePos );
        }
        else
        {
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
                    ( ( 1 << x.gameObject.layer ) & allowPlacementLayer.value ) != 0 ||
                    ( isFirstConnection && x.GetComponent<Tree>() != null );
            } );

            var rootType = newRoot.obj.GetComponent<BaseRoot>();

            if( validCollision && rootType.placementTagRequirement.Length > 0 && rootType.placementRequirement != null )
            {
                colliderList.Clear();
                rootType.placementRequirement.OverlapCollider( new ContactFilter2D().NoFilter(), colliderList );
                validCollision = colliderList.Any( x => x.CompareTag( rootType.placementTagRequirement ) );
            }

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
    }

    private void ProcessSelectionUI()
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

    private void ConfirmNewRoot()
    {
        var rootType = newRoot.obj.GetComponent<BaseRoot>();
        currentResource -= rootType.cost;

        UpdateBars();

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
        gameState = GameState.Game;
        StartCoroutine( Utility.FadeToBlack( mainMenuPanel, menuFadeOutTime ) );

        foreach( var letter in mainMenuLetters )
            letter.Hide();

        gameUIRoot.SetActive( true );
    }

    void ShowMenu()
    {
        gameState = GameState.Menu;
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
        levelUpUI.SetActive( show && gameState == GameState.Game );
        var texts = levelUpUI.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
        texts[0].text = string.Format( "Next Tree Age: {0}\nCost: ", level + 2 );
        texts[1].text = GetLevelUpCost().food.ToString();
        texts[2].text = GetLevelUpCost().water.ToString();
        levelUpUI.GetComponentInChildren<Image>().color = CanLevelUp() ? levelUpUIColour : invalidPlacementColour;
    }

    private bool CanLevelUp()
    {
        return currentResource.water >= GetLevelUpCost().water &&
            currentResource.food >= GetLevelUpCost().food &&
            currentResource.energy >= GetLevelUpCost().energy;
    }

    public void LevelUp()
    {
        if( !CanLevelUp() )
            return;

        currentResource -= GetLevelUpCost();
        UpdateBars();

        ChangeLevel( level + 1 );
        SetupRootTypeUIOptions();
        ShowLevelUpPopup( false );
        maxResource.energy = GetLevelUpCost().energy;
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
