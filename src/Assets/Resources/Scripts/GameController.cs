using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] WaveData waveData;
    [SerializeField] GameConstants constants;
    [SerializeField] DayNightCycleUI dayNightUI;
    [SerializeField] int currentDay;
    [SerializeField] Transform spawnPosLeft;
    [SerializeField] Transform spawnPosRight;

    private bool dayTime = true;
    private bool endlessMode;
    private List<GameObject> monsters = new();
    private PlayerController player;
    private float currentTimeHours = 6.0f;

    private void Start()
    {
        player = FindObjectOfType<PlayerController>();

        currentTimeHours = waveData.startHour;
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
                newMonster.GetComponent<Monster>().Initialise( modifiers );
                monsters.Add( newMonster );

                yield return new WaitForSeconds( next.delayPerMonsterSec );
            }

            yield return new WaitForSeconds( curentDayData.delaySec );
        }

        yield return null;
    }
}