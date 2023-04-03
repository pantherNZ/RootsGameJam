using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] bool rotateTowardsMovement = true;
    [SerializeField] float maxRange = 0.0f;
    public event Action<Collider2D> onCollision;

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
            Destroy();

        if( maxRange > 0.0f && ( startPos - transform.position ).sqrMagnitude >= maxRange * maxRange )
            Destroy();
    }

    private void OnTriggerEnter2D( Collider2D col )
    {
        onCollision?.Invoke( col );
    }

    private void Destroy()
    {
        var ps = GetComponent<ParticleSystem>();
        if( ps != null )
            ps.Play();
        gameObject.Destroy();
    }
}