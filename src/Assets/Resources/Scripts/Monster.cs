using UnityEngine;

public class Monster : MonoBehaviour
{
    [SerializeField] float heightOffset;
    [SerializeField] float attackDistance;
    [SerializeField] float runningSpeed;
    [SerializeField] int maxHealth;
    [SerializeField] float speed;
    [SerializeField] int damage;
    [SerializeField] bool attacking;
    private int health;
    private Animator animator;
    private Tree tree;

    const string isDeadParam = "isDead";
    const string isAttackingParam = "isAttacking";
    const string isRunningParam = "isRunning";
    const string isHurtParam = "isHurt";

    private void Awake()
    {
        animator = GetComponent<Animator>();
        tree = FindObjectOfType<Tree>();
    }

    public void Initialise( Modifier mod )
    {
        speed *= mod.speedModifier;
        maxHealth = Mathf.RoundToInt( maxHealth * mod.lifeModifier );
        health = maxHealth;
        damage = Mathf.RoundToInt( damage * mod.damageModifier );
        animator.SetBool( isRunningParam, speed >= runningSpeed );
        transform.position += new Vector3( 0.0f, heightOffset, 0.0f );
    }

    public void ReceiveDamage( int damage )
    {
        health -= damage;

        animator.SetTrigger( isHurtParam );

        if( health <= 0 )
        {
            health = 0;
            animator.SetBool( isDeadParam, true );
        }
    }

    public void DispatchDamage()
    {
        tree.ReceiveDamage( damage, DamageType.Default );
    }
    
    private void Update()
    {
        animator.SetBool( isAttackingParam, attacking );

        if( !attacking )
        {
            transform.position += new Vector3( -transform.position.normalized.x * speed * Time.deltaTime, 0.0f, 0.0f );
            attacking = Mathf.Abs( transform.position.x ) <= attackDistance;
        }
    }
}
