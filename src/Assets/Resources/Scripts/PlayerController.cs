using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Root
{
    public GameObject obj;
    public SpriteRenderer sprite;
    public MeshRenderer mesh;
    public Collider2D collision;
    public List<Root> children = new List<Root>();
    public Root parent;
    public Color baseCol;
    public RootMeshCreator spline;
    public BaseRoot rootType;
    public Vector3 fromPos;
    public int depth;
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

        // Find root connections and listen to their click event
        var rootConnections = FindObjectsOfType<RootConnection>();
        mainMenuLetters = FindObjectsOfType<MainMenuLetter>().ToList();

        ListenToConnections( rootConnections );
        SetupRootTypeUIOptions();

        UpdateBars();

        levelUpUIColour = levelUpUI.GetComponentInChildren<Image>().color;
        ShowLevelUpPopup( false );

        // Hook up initial parent / link between connections and starting splines
        var splines = FindObjectsOfType<RootMeshCreator>();
        foreach( var spline in splines )
        {
            spline.pathCreator.InitializeEditorData( true );
            spline.transform.parent.GetComponent<RootConnection>().parent = new Root()
            {
                obj = spline.gameObject,
                collision = spline.GetComponentInChildren<Collider2D>(),
                spline = spline,
            };
        }
    }


    public override void OnEventReceived( IBaseEvent e )
    {
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

    private void UpdateScreens()
    {
        gameUIRoot.SetActive( gameState == GameState.Game );
        rootSelectionUI.SetActive( gameState == GameState.Game );
        mainMenuPanel.gameObject.SetActive( gameState == GameState.Menu );
        gameUIRoot.SetActive( gameState == GameState.Game );
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

        if( rootType.GetComponent<RootMeshCreator>() )
        {
            var spline = currentConnection.parent.obj.GetComponent<RootMeshCreator>();
            var prevTValue = spline.pathCreator.path.GetClosestTimeOnPath( currentConnection.transform.position );

            // If the connection is less than 90%, then branch off a new spline
            if( prevTValue < 0.9f && currentConnection.parent.depth > 0 )
            {
                var newSpline = Instantiate( rootType ).GetComponent<RootMeshCreator>();
                newSpline.transform.position = currentConnection.transform.position;
                newSpline.pathCreator.InitializeEditorData( true );
                newSpline.collisionDistOffset = 1.5f; // Prevent collision with tree
                newSpline.TriggerUpdate();

                newRoot = new Root()
                {
                    obj = newSpline.gameObject,
                    collision = newSpline.gameObject.GetComponentInChildren<Collider2D>(),
                    spline = newSpline,
                    parent = currentConnection.parent,
                    rootType = rootType,
                    depth = currentConnection.parent.depth + 1,
                    fromPos = currentConnection.transform.position,
                };

                var connections = newSpline.GetComponentsInChildren<RootConnection>( true );
                ListenToConnections( connections );
                foreach( var connection in connections )
                    connection.parent = newRoot;

                Utility.FunctionTimer.CreateTimer( 0.1f, () =>
                {
                    newRoot.mesh = newSpline.GetComponentInChildren<MeshRenderer>();
                    newRoot.baseCol = newRoot.mesh.material.GetColor( "_Colour" );
                } );
            }
            // Extend the existing spline
            else
            {
                var pos = currentConnection.transform.position + currentConnection.transform.forward * 1.0f;
                spline.pathCreator.bezierPath.AddSegmentToEnd( pos );
                spline.collisionDistOffset = 1.5f; // Prevent collision with tree
                spline.TriggerUpdate();

                newRoot = new Root()
                {
                    obj = spline.gameObject,
                    collision = spline.gameObject.GetComponentInChildren<Collider2D>(),
                    spline = spline,
                    parent = currentConnection.parent,
                    rootType = rootType,
                    depth = currentConnection.parent.depth + 1,
                    fromPos = spline.pathCreator.path.LocalToWorld( 
                        spline.pathCreator.bezierPath.GetPoint( spline.pathCreator.bezierPath.NumPoints - 4 ) ),
                };

                // Update old connections t values (set their distance along path, so they maintain their position)
                var oldConnections = spline.GetComponentsInChildren<RootConnection>( true );
                foreach( var connection in oldConnections )
                    connection.distAlongPath = spline.pathCreator.path.GetClosestDistanceAlongPath( connection.transform.position );

                // Construct new connections and set their t values
                var connectionsToConstruct = rootType.GetComponentsInChildren<RootConnection>( true );
                var numSegments = spline.pathCreator.bezierPath.NumSegments - 1;
                var tValueStart = ( 1.0f / numSegments ) * ( numSegments - 1 );
                foreach( var connection in connectionsToConstruct )
                {
                    var newConnection = Instantiate( connection, newRoot.obj.transform );
                    newConnection.parent = newRoot;
                    newConnection.tValue = tValueStart + ( 1.0f - tValueStart ) * newConnection.tValue;
                    ListenToConnections( new RootConnection[] { newConnection } );
                }

                newRoot.mesh = spline.GetComponentInChildren<MeshRenderer>();
                newRoot.baseCol = newRoot.mesh.material.GetColor( "_Colour" );
            }
        }
        else
        {
            var rootObj = Instantiate( rootType );

            newRoot = new Root()
            {
                obj = rootObj.gameObject,
                sprite = rootObj.GetComponentInChildren<SpriteRenderer>(),
                collision = rootObj.GetComponentInChildren<Collider2D>(),
                parent = currentConnection.parent,
                rootType = rootObj.GetComponent<BaseRoot>(),
                depth = currentConnection.parent.depth + 1,
            };

            var depth = ( currentConnection.parent == null ||
                currentConnection.parent.sprite == null ) ? 49 : currentConnection.parent.sprite.sortingOrder - 1;
            newRoot.baseCol = newRoot.sprite.color;
            newRoot.sprite.sortingOrder = depth;
            newRoot.obj.transform.position = currentConnection.transform.position;
        }
    }

    public static bool LineLineIntersection( Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2 )
    {
        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross( lineVec1, lineVec2 );
        Vector3 crossVec3and2 = Vector3.Cross( lineVec3, lineVec2 );

        float planarFactor = Vector3.Dot( lineVec3, crossVec1and2 );

        //is coplanar, and not parallel
        if( Mathf.Abs( planarFactor ) < 0.0001f
                && crossVec1and2.sqrMagnitude > 0.0001f )
        {
            float s = Vector3.Dot( crossVec3and2, crossVec1and2 )
                    / crossVec1and2.sqrMagnitude;
            var intersection = linePoint1 + ( lineVec1 * s );
            float aSqrMagnitude = lineVec1.sqrMagnitude;
            float bSqrMagnitude = lineVec2.sqrMagnitude;

            if( ( intersection - linePoint1 ).sqrMagnitude <= aSqrMagnitude
                 && ( intersection - ( linePoint1 + lineVec1 ) ).sqrMagnitude <= aSqrMagnitude
                 && ( intersection - linePoint2 ).sqrMagnitude <= bSqrMagnitude
                 && ( intersection - ( linePoint2 + lineVec2 ) ).sqrMagnitude <= bSqrMagnitude )
            {
                return true;
            }
            return false;
        }
        else
        {
            return false;
        }
    }

    private void ProcessRootPlacement()
    {
        var mousePos = mainCamera.ScreenToWorldPoint( Utility.GetMouseOrTouchPos() );

        // Positioning
        if( newRoot.spline != null )
        {
            var path = newRoot.spline.pathCreator.bezierPath;
            var offset = ( mousePos - newRoot.fromPos ).SetZ( 0.0f );
            if( newRoot.rootType.lengthMax > 0.0 )
                offset = offset.Clamp( newRoot.rootType.lengthMin, newRoot.rootType.lengthMax );
            var result = newRoot.fromPos + offset;
            if( result.y > gameConstants.maxPlaceYValue )
                result.y = gameConstants.maxPlaceYValue;
            var localPos = newRoot.spline.pathCreator.path.WorldToLocal( result );
            path.MovePoint( path.NumPoints - 1, localPos );
            newRoot.spline.TriggerUpdate();
        }
        else
        {
            var direction = ( mousePos - currentConnection.transform.position ).normalized.ToVector2();
            newRoot.obj.transform.rotation = Quaternion.Euler( 0.0f, 0.0f, -direction.Angle() + 90.0f );
        }

        // Collision and angle check
        List<Collider2D> colliderList = new List<Collider2D>();
        bool isFirstConnection = newRoot.depth <= 1;
        newRoot.collision.OverlapCollider( new ContactFilter2D().NoFilter(), colliderList );
        //bool validAngle = ( Vector3.Angle( direction, currentConnection.transform.right ) <= currentConnection.rootMaxAngleDegrees / 2.0f ||
        //    ( currentConnection.allowBackwards && Vector3.Angle( direction, -currentConnection.transform.right ) > currentConnection.rootMaxAngleDegrees / 2.0f ) );
        bool validAngle = true;
        bool validCollision = colliderList.All( x =>
        {
            return x.gameObject == newRoot.obj ||
                x.transform.IsChildOf( newRoot.obj.transform ) ||
                ( ( 1 << x.gameObject.layer ) & allowPlacementLayer.value ) != 0 ||
                ( isFirstConnection && x.CompareTag("Tree") );
        } );

        if( newRoot.spline != null && validCollision )
        {
            for( int i = 0; i < newRoot.spline.pathCreator.bezierPath.NumPoints - 2; ++i )
            {
                var a1 = newRoot.spline.pathCreator.bezierPath.GetPoint( i );
                var a2 = newRoot.spline.pathCreator.bezierPath.GetPoint( i + 1 );
                for( int j = 0; j < newRoot.spline.pathCreator.bezierPath.NumPoints - 2; ++j )
                {
                    if( Math.Abs( i - j ) < 3 )
                        continue;

                    var b1 = newRoot.spline.pathCreator.bezierPath.GetPoint( j );
                    var b2 = newRoot.spline.pathCreator.bezierPath.GetPoint( j + 1 );
                    if( LineLineIntersection( a1, a2 - a1, b2, b2 - b1 ) )
                    {
                        validCollision = false;
                        break;
                    }
                }
            }
        }

        var rootType = newRoot.obj.GetComponent<BaseRoot>();

        if( validCollision && rootType != null && rootType.placementTagRequirement.Length > 0 && rootType.placementRequirement != null )
        {
            colliderList.Clear();
            rootType.placementRequirement.OverlapCollider( new ContactFilter2D().NoFilter(), colliderList );
            validCollision = colliderList.Any( x => x.CompareTag( rootType.placementTagRequirement ) );
        }

        bool validPlacement = validCollision && validAngle;

        // Colour tint
        if( newRoot.sprite != null )
            newRoot.sprite.color = validPlacement ? newRoot.baseCol : invalidPlacementColour;
        if( newRoot.mesh != null )
            newRoot.mesh.material.SetColor( "_Colour", validPlacement ? newRoot.baseCol : invalidPlacementColour );

        // Place
        if( validPlacement && Input.GetMouseButtonDown( 0 ) )
        {
            ConfirmNewRoot();
        }
        // Cancel
        else if( Input.GetMouseButtonDown( 1 ) )
        {
            var numNewConnections = newRoot.rootType.GetComponentsInChildren<RootConnection>( true ).Length;
            var allConnections = newRoot.obj.GetComponentsInChildren<RootConnection>( true );
            for( int i = 0; i < numNewConnections; ++i )
            {
                allConnections[allConnections.Length - numNewConnections + i].DestroyGameObject();
            }

            if( newRoot.spline == null || newRoot.spline.pathCreator.bezierPath.NumPoints <= 4 )
            {
                newRoot.obj.Destroy();
            }
            else
            {
                newRoot.spline.pathCreator.bezierPath.DeleteSegment( newRoot.spline.pathCreator.bezierPath.NumPoints - 1 );
                newRoot.spline.TriggerUpdate();
            }

            newRoot = null;
            currentConnection = null;
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
        var rootType = newRoot.rootType;
        currentResource -= rootType.cost;

        UpdateBars();

        rootType.isPlaced = true;
        rootType.OnPlacement();
        roots.Add( newRoot );

        if( newRoot.spline == null )
        {
            var newConnections = newRoot.obj.GetComponentsInChildren<RootConnection>( true );
            ListenToConnections( newConnections );

            foreach( var connection in newConnections )
            {
                connection.gameObject.SetActive( true );
                connection.parent = newRoot;
            }
        }

        if( --currentConnection.currentConnections == 0 )
        {
            currentConnection.GetComponent<EventDispatcherV2>().OnPointerDownEvent.RemoveAllListeners();
        }

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
