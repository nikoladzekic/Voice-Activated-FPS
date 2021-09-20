using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public static Dictionary<int, Projectile> projectiles = new Dictionary<int, Projectile>();
    private static int nextProjectileId = 1;

    //Assignables
    public Rigidbody rb;
    public LayerMask whatIsEnemies;
    public int shotByPlayer;
    public int id;

    //Stats
    [Range(0f, 1f)]
    public float bounciness;
    public bool useGravity;
    public Vector3 initialForce;

    //Damage
    public int explosionDamage;
    public float explosionRange;
    public float explosionForce;
    public int bulletDamage;

    //Lifetime
    public int maxCollisions;
    public float maxLifetime;
    public bool explodeOnTouch = false;

    int collisions;
    PhysicMaterial physics_mat;

    private void Start()
    {
        Setup();
        id = nextProjectileId;
        nextProjectileId++;
        projectiles.Add(id, this);

        rb.AddForce(initialForce, ForceMode.Impulse);
    }

    private void FixedUpdate()
    {
        //When to explode:
        if (collisions > maxCollisions && explodeOnTouch) Explode();

        //Count down lifetime
        maxLifetime -= Time.deltaTime;
        if (maxLifetime <= 0 && explodeOnTouch) Explode();
    }

    public void Initialize(Vector3 _initialMovementDirection, float _initialForceStrength, int _thrownByPlayer)
    {
        initialForce = _initialMovementDirection * _initialForceStrength;
        shotByPlayer = _thrownByPlayer;
    }
    private void Explode()
    {
        //Check for enemies 
        Collider[] enemies = Physics.OverlapSphere(transform.position, explosionRange, whatIsEnemies);
        for (int i = 0; i < enemies.Length; i++)
        {
            //Get component of enemy and call Take Damage

            enemies[i].GetComponent<Player>().TakeDamage(explosionDamage);
        }
        Invoke("Delay", 0.001f);
    }
    private void Delay()
    {
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Count up collisions
        collisions++;
        //Explode if bullet hits an enemy directly and explodeOnTouch is activated
        if (collision.collider.CompareTag("Player"))
        {
            collision.collider.GetComponent<Player>().TakeDamage(bulletDamage);
        }
    }

    private void Setup()
    {
        //Create a new Physic material
        physics_mat = new PhysicMaterial();
        physics_mat.bounciness = bounciness;
        physics_mat.frictionCombine = PhysicMaterialCombine.Minimum;
        physics_mat.bounceCombine = PhysicMaterialCombine.Maximum;
        //Assign material to collider
        GetComponent<SphereCollider>().material = physics_mat;

        //Set gravity
        rb.useGravity = useGravity;
    }
}
