using UnityEngine;

public class Monster : MonoBehaviour
{
    [SerializeField] int maxHealth;
    private int health;
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void SetModifiers( Modifier mod )
    {

    }

    public void DealDamage( int damage )
    {
        health -= damage;

        if( health <= 0 )
        {
            health = 0;
            animator.SetBool( "isDead", true );
        }
    }
}
