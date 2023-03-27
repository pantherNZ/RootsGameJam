using UnityEngine;

public class ShootingDefence : BaseDefence
{
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Transform projectileSpawnPos;
    float cooldown;

    private void Update()
    {
        if( cooldown > 0.0f )
        {
            cooldown = Mathf.Max( cooldown - Time.deltaTime, 0.0f );
        }
        else if( CanFire() )
        {
            Fire();
        }
    }

    private bool CanFire()
    {
        return true;
    }

    private void Fire()
    {
        for( int i = 0; i < type.numProjectiles; ++i )
        {
            var newProj = Instantiate( projectilePrefab );
            newProj.transform.position = projectileSpawnPos.position;
            newProj.transform.right = projectileSpawnPos.right;
            newProj.GetComponent<Rigidbody2D>().AddForce( projectileSpawnPos.right * type.projectileSpeed, ForceMode2D.Impulse );
        }

        cooldown = type.attackTimeSec;
    }
}
