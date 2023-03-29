using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] bool rotateTowardsMovement = true;

    private Rigidbody2D rigidBody;

    private void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if( rotateTowardsMovement )
            transform.right = rigidBody.velocity;
    }
}