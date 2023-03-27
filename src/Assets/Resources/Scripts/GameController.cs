using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : EventReceiverInstance
{
    [SerializeField] WaveData waveData;
    [SerializeField] GameConstants constants;
    [SerializeField] DayNightCycleUI dayNightUI;
    [SerializeField] int currentDay;
    [SerializeField] Transform spawnPosLeft;
    [SerializeField] Transform spawnPosRight;
    [SerializeField] int monsterLayerMin = 7;
    [SerializeField] int monsterLayerMax = 100;

    private bool dayTime = true;
    private bool endlessMode;
    private List<GameObject> monsters = new();
    private float currentTimeHours = 6.0f;
    private List<int> validLayers = new List<int>();

    [HideInInspector] public PlayerController player;
    public static GameController Instance { get; private set; }
    public GameConstants Constants { get => constants; private set { } }

    private void Awake()
    {
        Instance = this;
    }

    protected override void Start()
    {
        base.Start();

        Instance = this;
        player = FindObjectOfType<PlayerController>();

        currentTimeHours = waveData.startHour;

        validLayers.Capacity = monsterLayerMax - monsterLayerMin;
        for( int i = monsterLayerMin; i < monsterLayerMax; ++i )
            validLayers.Add( i );
    }

    private void Update()
    {
        if( player.gameState != GameState.Game )
            return;

        const float hoursInDay = 24.0f;

        bool wasDayTime = dayTime;
        currentTimeHours += ( Time.deltaTime / waveData.dayLengthSec ) * hoursInDay;

        // Day wrap / restart
        if( currentTimeHours >= hoursInDay )
        {
            ++currentDay;
            currentTimeHours = Utility.Mod( currentTimeHours, hoursInDay );
            endlessMode = currentDay >= waveData.waves.Count;
        }

        dayNightUI.SetValue( currentTimeHours, hoursInDay );

        dayTime = currentTimeHours >= waveData.nightEndHour && currentTimeHours < waveData.nightStartHour;

        // Start waves
        if( wasDayTime && !dayTime )
        {
            StartCoroutine( SpawnMonsters() );
        }
    }

    private IEnumerator SpawnMonsters()
    {
        var curentDayData = endlessMode ? 
            waveData.endlessWaves[Utility.Mod( currentDay, waveData.endlessWaves.Count )] :
            waveData.waves[currentDay];

        var layers = new List<int>( validLayers );

        for( int waveIdx = 0; waveIdx < curentDayData.monsters.Count; ++waveIdx )
        {
            Debug.Assert( !dayTime );
            var next = curentDayData.monsters[waveIdx];

            for( int spawnIdx = 0; spawnIdx < next.count; ++spawnIdx )
            {
                var finalIdx = waveIdx + spawnIdx;
                var spawnPos = curentDayData.spawnSide switch
                {
                    SpawnPos.Left => spawnPosLeft.position,
                    SpawnPos.Right => spawnPosRight.position,
                    SpawnPos.Random => Utility.RandomBool() ? spawnPosLeft.position : spawnPosRight.position,
                    SpawnPos.Alternating => ( finalIdx % 2 == 1 ) ? spawnPosLeft.position : spawnPosRight.position,
                    SpawnPos.AlternatingPairs => ( Mathf.Ceil( finalIdx / 2 ) - 1 ) % 2 == 1 ? spawnPosLeft.position : spawnPosRight.position,
                    _ => spawnPosLeft.position
                };

                var newMonster = Instantiate( next.monsterPrefab, spawnPos, Quaternion.identity );
                var modifiers = curentDayData.modifier;
                if( endlessMode )
                    modifiers *= waveData.endlessPerDayModifier;
                newMonster.GetComponent<Monster>().Initialise( next, modifiers );
                
                // Random layer
                var idx = Random.Range( 0, layers.Count - 1 );
                newMonster.GetComponent<SpriteRenderer>().sortingOrder = layers[idx];
                layers.RemoveAt( idx );

                if( layers.IsEmpty() )
                    layers = new List<int>( validLayers );

                monsters.Add( newMonster );

                yield return new WaitForSeconds( next.delayPerMonsterSec );
            }

            yield return new WaitForSeconds( curentDayData.delaySec );
        }

        yield return null;
    }

    public override void OnEventReceived( IBaseEvent e )
    {
        if( e is ResetGameEvent )
        {
            endlessMode = false;
            dayTime = true;
            currentTimeHours = waveData.startHour;
            currentDay = 0;

            foreach( var monster in monsters )
                monster.Destroy();
            monsters.Clear();
        }
    }
}