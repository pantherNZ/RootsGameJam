using System.Collections;
using System.Linq;
using UnityEngine;

public class Monster : MonoBehaviour, IDamageDealer
{
    [SerializeField] float heightOffset;
    [SerializeField] float attackDistance;
    [SerializeField] float attackDistanceRand;
    [SerializeField] float runningSpeed;
    [SerializeField] string runAnim;
    [SerializeField] string walkAnim;
    [SerializeField] string attackAnim;
    [SerializeField] string takeHitAnim;
    [SerializeField] string deathAnim;
    [SerializeField] LayerMask attackLayer;

    private float speed;
    private int damage;
    private bool attacking;
    private int health;
    private Animator animator;
    private string currentAnim;
    private float attackDistanceFinal;

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
        attackDistanceFinal = attackDistance + Random.Range( -attackDistanceRand / 2.0f, attackDistanceRand / 2.0f );
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
            PlayAnimation( takeHitAnim );
            QueueAnimation( prevAnim );
        }
    }

    private void PlayMovementAnimation()
    {
        PlayAnimation( speed >= runningSpeed ? runAnim : walkAnim );
    }

    private void PlayAnimation( string anim )
    {
        currentAnim = anim;
        animator.Play( anim );
    }

    private void QueueAnimation( string anim )
    {
        StartCoroutine( QueueAnimationInternal( anim ) );
    }

    private IEnumerator QueueAnimationInternal( string anim )
    {
        var currentAnim = animator.GetCurrentAnimatorStateInfo( 0 ).shortNameHash;

        while( animator.GetCurrentAnimatorStateInfo( 0 ).shortNameHash == currentAnim )
        {
            yield return null;
        }

        animator.Play( anim );
    }

    private void Update()
    {
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
}
