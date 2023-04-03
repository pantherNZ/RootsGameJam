using UnityEngine;

public class ShootingDefence : BaseDefence, IDamageDealer
{
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] Transform projectileSpawnPos;
    float cooldown;

    private void Start()
    {
        cooldown = type.attackTimeSec;
    }

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
            newProj.GetComponent<Rigidbody2D>().AddForce( projectileSpawnPos.up * type.projectileSpeed, ForceMode2D.Impulse );
            newProj.GetComponent<Projectile>().onCollision += Projectile_onCollision;
        }

        cooldown = type.attackTimeSec;
    }

    private void Projectile_onCollision( Collider2D obj )
    {
        if( obj == null )
            return;

        if( obj.GetComponent<Monster>() == null )
            return;

        var damageable = obj.GetComponent<IDamageable>();
        if( damageable != null )
            DispatchDamage( damageable, type.damage, type.damageType );
    }

    public void DispatchDamage( IDamageable to, int damage, DamageType type )
    {
        to.ReceiveDamage( this, damage, type );
    }
}
