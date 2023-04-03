using System.Collections;
using System.Linq;
using System;
using UnityEngine;

public class Monster : MonoBehaviour, IDamageDealer, IDamageable
{
    [SerializeField] float heightOffset;
    [SerializeField] float attackDistance;
    [SerializeField] float attackDistanceRand;
    [SerializeField] float runSpeedThreshold;
    [SerializeField] float moveAnimationSpeed;
    [SerializeField] string runAnim;
    [SerializeField] string walkAnim;
    [SerializeField] string attackAnim;
    [SerializeField] string takeHitAnim;
    [SerializeField] string deathAnim;
    [SerializeField] LayerMask attackLayer;

    [SerializeField] float speed;
    [ReadOnly, SerializeField] int damage;
    [ReadOnly, SerializeField] bool attacking;
    [ReadOnly, SerializeField] int health;
    [ReadOnly, SerializeField] string currentAnim;
    [ReadOnly, SerializeField] float attackDistanceFinal;
    [ReadOnly, SerializeField] Resource? gainOnKill;
    private Animator animator;

    // Old health, new health, damage type
    public event Action<int, int, DamageType> OnHealthChanged;
    public bool IsDead => health <= 0;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Initialise( MonsterData data, Modifier mod )
    {
        speed = data.speed * mod.speedModifier;
        health = Mathf.RoundToInt( data.health * mod.lifeModifier );
        damage = Mathf.RoundToInt( data.damage * mod.damageModifier );
        transform.position += new Vector3( 0.0f, heightOffset, 0.0f );
        gainOnKill = data.gainOnKill;
        attackDistanceFinal = attackDistance + UnityEngine.Random.Range( -attackDistanceRand / 2.0f, attackDistanceRand / 2.0f );
        PlayMovementAnimation();
    }

    public void ReceiveDamage( int damage )
    {
        health -= damage;

        if( health <= 0 )
        {
            health = 0;
            PlayAnimation( deathAnim );
        }
        else
        {
            var prevAnim = currentAnim;
            var prevSpeed = animator.speed;
            PlayAnimation( takeHitAnim );
            QueueAnimation( prevAnim, prevSpeed );
        }
    }

    private void PlayMovementAnimation()
    {
        float animSpeed = speed / moveAnimationSpeed;
        PlayAnimation( speed >= runSpeedThreshold ? runAnim : walkAnim, animSpeed );
    }

    private void PlayAnimation( string anim, float speed = 1.0f )
    {
        currentAnim = anim;
        animator.Play( anim );
        animator.speed = speed;
    }

    private void QueueAnimation( string anim, float speed = 1.0f )
    {
        StartCoroutine( QueueAnimationInternal( anim, speed ) );
    }

    private IEnumerator QueueAnimationInternal( string anim, float speed = 1.0f )
    {
        var current = animator.GetCurrentAnimatorStateInfo( 0 ).shortNameHash;

        while( animator.GetCurrentAnimatorStateInfo( 0 ).shortNameHash == current )
        {
            yield return null;
        }

        PlayAnimation( anim, speed );
    }

    private void Update()
    {
        if( IsDead )
            return;

        var prevAttacking = attacking;
        var direction = transform.right * -transform.position.normalized.x;
        if( !attacking )
            transform.position += speed * Time.deltaTime * direction;
        var hits = Physics2D.RaycastAll( transform.position, direction, attackDistanceFinal, attackLayer );
        attacking = hits.Any( x => x.collider.GetComponent<IDamageable>() != null );

        if( attacking != prevAttacking )
        {
            if( attacking )
                PlayAnimation( attackAnim );
            else
                PlayMovementAnimation();
        }
    }

    public void DispatchDamage()
    {
        var direction = transform.right * -transform.position.normalized.x;
        var hits = Physics2D.RaycastAll( transform.position, direction, attackDistanceFinal, attackLayer );

        foreach( var hit in hits )
        {
            var target = hit.collider.GetComponent<IDamageable>();
            if( target != null )
                DispatchDamage( target, damage, DamageType.Default );
        }
    }

    public void DispatchDamage( IDamageable to, int damage, DamageType type )
    {
        to.ReceiveDamage( this, damage, type );
    }

    public void ReceiveDamage( IDamageDealer from, int damage, DamageType type )
    {
        if( IsDead )
            return;

        var oldHp = health;
        health = Mathf.Max( 0, health - damage );
        OnHealthChanged?.Invoke( oldHp, health, type );

        if( health <= 0 )
        {
            PlayAnimation( "Death" );
            Utility.FunctionTimer.CreateTimer( 2.0f, () => gameObject.Destroy() );
            // Play particles, increment kills counter
            if( gainOnKill != null )
            {
                EventSystem.Instance.TriggerEvent( new GainResourcesEvent()
                {
                    originForUIDisplay = transform.position,
                    res = gainOnKill.Value,
                } );
            }

            GetComponent<Collider2D>().enabled = false;
        }
    }
}
