using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] bool rotateTowardsMovement = true;
    [SerializeField] float maxRange = 0.0f;
    public event Func<Collider2D, bool> onCollision;

    private Rigidbody2D rigidBody;
    private Vector3 startPos;

    private void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        startPos = transform.position;
    }

    private void Update()
    {
        if( rotateTowardsMovement )
            transform.right = rigidBody.velocity;

        if( transform.position.y <= 0.0f )
            DestroyProjectile();

        if( maxRange > 0.0f && ( startPos - transform.position ).sqrMagnitude >= maxRange * maxRange )
            DestroyProjectile();
    }

    private void OnTriggerEnter2D( Collider2D col )
    {
        if( onCollision != null )
            if( onCollision.Invoke( col ) )
                DestroyProjectile();
    }

    private void DestroyProjectile()
    {
        var ps = GetComponent<ParticleSystem>();
        if( ps != null )
            ps.Play();
        gameObject.Destroy( false );
    }
}