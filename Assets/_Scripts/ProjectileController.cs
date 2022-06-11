using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{

    private ProjectileScriptable projectile = null;
    private List<Transform> objectsHit = new List<Transform>();
    private float charge;
    private Rigidbody rb;
    private float t;
    private int collisionsLeft;

    public void InitProjectile(ProjectileScriptable projectile, float charge, Vector3 velocity)
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        rb.useGravity = projectile.usesGravity;
        rb.AddForce(velocity, ForceMode.Impulse);
        this.projectile = projectile;
        this.charge = charge;
        collisionsLeft = projectile.collisionsBeforeDestroyed;
        objectsHit.Clear();
        StartCoroutine(LifeTime());
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger || Helpers.IsInLayerMask(other.gameObject, projectile.hitLayers) == false)
        {
            return;
        }
        if (projectile != null)
        {
            if (projectile.explodesOnImpact)
            {
                Explode();
            }
            if (projectile.leaveAOEOnImpact)
            {
                SpawnAOE();
            }
        }
        if (projectile.destroyedOnImpact)
        {
            collisionsLeft--;
            if (collisionsLeft <= 0)
            {
                StopAllCoroutines();
                Destroy(gameObject);
            }
        }
    }

    void Explode()
    {
        Collider[] effected = Physics.OverlapSphere(transform.position, projectile.explosionRadius);
        for (int i = 0; i < effected.Length; i++)
        {
            if (effected[i].CompareTag("Enemy"))
            {
                if (objectsHit.Contains(effected[i].transform.root))
                {
                    continue;
                }

                print(effected[i].transform.root.name);

                objectsHit.Add(effected[i].transform.root);
                var dist = (effected[i].transform.position - transform.position).magnitude;
                effected[i].transform.root.GetComponent<Enemy_LeekHurt>().TakeDmg(
                    transform.position,
                    null,
                    Mathf.Lerp(projectile.damageMinMax.x, projectile.damageMinMax.y, charge),
                    Mathf.Lerp(projectile.poiseDmgMinMax.x, projectile.poiseDmgMinMax.y, charge),
                    Mathf.Lerp(projectile.kbForceMinMax.x, projectile.kbForceMinMax.y,
                        dist / projectile.explosionRadius * charge));
            }
        }
    }
    void SpawnAOE()
    {
        Singleton.instance.DamageInstanceSpawner.SpawnDamageInstance_Sphere(
            transform.position,
            Quaternion.LookRotation(Vector3.up),
            projectile.aoeRadius,
            projectile.aoeLifetime,
            projectile.tickInterval,
            projectile.tickDmg,
            projectile.poiseDmgPerTick);
    }

    IEnumerator LifeTime()
    {
        t = 0;
        while (t < projectile.projectileLifeTime)
        {
            t += Time.deltaTime;
            yield return null;
        }
        if (projectile.explodeOnLifetimeEnd)
        {
            Explode();
        }
        if (projectile.leaveAOEOnLifetimeEnd)
        {
            SpawnAOE();
        }
        Destroy(gameObject);
    }
}
