using System.Collections;
using UnityEngine;

public class Monster : MonoBehaviour, IDamageDealer
{
    [SerializeField] float heightOffset;
    [SerializeField] float attackDistance;
    [SerializeField] float runningSpeed;
    [SerializeField] int maxHealth;
    [SerializeField] float speed;
    [SerializeField] int damage;
    [SerializeField] bool attacking;
    [SerializeField] string runAnim;
    [SerializeField] string walkAnim;
    [SerializeField] string attackAnim;
    [SerializeField] string takeHitAnim;
    [SerializeField] string deathAnim;

    private int health;
    private Animator animator;
    private string currentAnim;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Initialise( Modifier mod )
    {
        speed *= mod.speedModifier;
        maxHealth = Mathf.RoundToInt( maxHealth * mod.lifeModifier );
        health = maxHealth;
        damage = Mathf.RoundToInt( damage * mod.damageModifier );
        transform.position += new Vector3( 0.0f, heightOffset, 0.0f );
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
        transform.position += new Vector3( -transform.position.normalized.x * speed * Time.deltaTime, 0.0f, 0.0f );
        attacking = Mathf.Abs( transform.position.x ) <= attackDistance;

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
        DispatchDamage( null, damage, DamageType.Default );
    }

    public void DispatchDamage( IDamageable to, int damage, DamageType type )
    {
        to.ReceiveDamage( this, damage, type );
    }
}
