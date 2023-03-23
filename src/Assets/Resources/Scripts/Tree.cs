using System;
using UnityEngine;

public interface IDamageDealer
{
    abstract void DispatchDamage( IDamageable to, int damage, DamageType type );
}

public interface IDamageable
{
    abstract void ReceiveDamage( IDamageDealer from, int damage, DamageType type );
}

[Serializable]
public struct TreeStats
{
    public int health;
    public int maxHealth;
    public int armour;
    public int maxArmour;
}

public class Tree : EventReceiverInstance, IDamageable
{
    [SerializeField] float spinSpeed;
    [SerializeField] GameObject treeLevelUpObj;
    [SerializeField] Color treeHighlightColour;
    [SerializeField] PlayerController controller;
    private TreeStats stats;
    private Color baseColour;
    private bool hovered = false;
    // <Old health, new health, modified by> 
    public event Action<TreeStats, TreeStats, DamageType> OnHealthChanged;

    protected override void Start()
    {
        base.Start();

        stats = GameController.Instance.Constants.treeInitialStats;
        var sprite = treeLevelUpObj.GetComponent<SpriteRenderer>();
        baseColour = sprite.color;

        var ed = treeLevelUpObj.GetComponent<EventDispatcherV2>();
        ed.OnPointerEnterEvent.AddListener( x =>
        {
            if( controller.gameState != GameState.Game )
                return;
            hovered = true;
            sprite.color = treeHighlightColour;
            // Show level up cost/info popup
            controller.ShowLevelUpPopup( true );
        } );
        ed.OnPointerExitEvent.AddListener( x =>
        {
            hovered = false;
            sprite.color = baseColour;
            controller.ShowLevelUpPopup( false );
        } );
        ed.OnPointerDownEvent.AddListener( x =>
        {
            hovered = false;
            controller.LevelUp();
        } );

        //Utility.FunctionTimer.CreateTimer( 1.0f, () => ReceiveDamage( null, 10000, DamageType.Default ) );
    }

    private void Update()
    {
        if( hovered )
            treeLevelUpObj.transform.Rotate( 0.0f, 0.0f, spinSpeed * Time.deltaTime, Space.Self );
    }

    public void ReceiveDamage( IDamageDealer from, int damage, DamageType type )
    {
        var oldHp = stats;
        stats.health = Mathf.Max( 0, stats.health - damage );
        OnHealthChanged?.Invoke( oldHp, stats, type );

        if( stats.health <= 0 )
        {
            EventSystem.Instance.TriggerEvent( new GameOverEvent() );
        }
    }

    public override void OnEventReceived( IBaseEvent e )
    {
        if( e is ResetGameEvent )
        {
            stats = GameController.Instance.Constants.treeInitialStats;
            OnHealthChanged?.Invoke( stats, stats, DamageType.Cheat );
        }
    }
}
