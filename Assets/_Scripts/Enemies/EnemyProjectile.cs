using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float minimumThrowDist = 5f;
    [SerializeField] private float lifeTime = 10;

    private float duration = 1f;
    //private float flightTimeMod;
    //private float projSpeed = 0.2f;
    //private float acceptableDisappearDist = 2.5f;
    private Vector3 target;
    private GameObject shooter;
    private Vector3 projEnd;
    private Vector3 projStart;
    //private Vector3 projHandle;
    private bool halfDone = false;

    private Rigidbody rb;
    private float totalDist;
    private float overshoot;
    private float projSpeed;
    private float projSpeedUp;
    private float accuracyRandomness;
    private float shootForce;
    private Vector3 shootDir;
    private Vector3 startPos;
    private Vector3 endPos;
    private bool applyForce, explodeOnImpact, projectileShot;
    private float t;


    void FixedUpdate()
    {
        ProjectileMovement();
        Disappear();
        if (projectileShot)
        {
            t += Time.deltaTime;
        }
    }

    private void ProjectileMovement()
    {
        if (applyForce)
        {
            rb.useGravity = true;
            rb.AddForce(shootDir * shootForce * rb.mass, ForceMode.Impulse);
            projectileShot = true;
            applyForce = false;
        }
        /*
        float perc = t / duration;
        if (!halfDone)
        {
            t += Time.deltaTime;
            perc = Mathf.Sin(perc * Mathf.PI * 0.5f);
            transform.root.position = Vector3.Lerp(startPos, endPos, perc);
            if (t >= duration)
            {
                ChangeDirection();
            }
        }
        else
        {
            perc = 1f - Mathf.Cos(perc * Mathf.PI * 0.5f);
            transform.root.position = Vector3.Lerp(startPos, endPos, perc);
        }*/

    }

    private void Disappear()
    {
        if (t > lifeTime)
        {
            Destroy(gameObject);
        }
    }

    void ShootCalc()
    {
        Vector3 F = this.target - shooter.transform.position;
        float Fx = new Vector3(F.x, 0, F.z).magnitude;
        shootDir = (new Vector3(F.x, 0, F.z).normalized + Vector3.up).normalized;
        shootForce = 20 / Mathf.Sqrt(2) * Mathf.Sqrt(9.81f / 2 * (Fx - F.y));
            /*
        projStart = transform.root.position;
        projEnd = target;
        projEnd += -shootForce * overshoot;
        //projHandle = projEnd - (dir * 3f);
        projEnd.y += 1f;
        startPos = projStart;
        endPos = projEnd;'*/
    }

    public void InitProjectile(Vector3 target, GameObject shooter, bool explodeOnImpact, float upwardsSpeed)
    {
        this.target = target;
        this.shooter = shooter;
        this.explodeOnImpact = explodeOnImpact;
        rb = GetComponent<Rigidbody>();
        ShootCalc();
        t = 0f;
        halfDone = false;
        applyForce = false;
        rb.useGravity = true;
    }
    public void ShootProjectile(float accuracyRandomness, float projSpeed, float projSpeedUp)
    {
        this.projSpeed = projSpeed;
        this.accuracyRandomness = accuracyRandomness;
        this.projSpeedUp = projSpeedUp;
        t = 0;
        applyForce = true;
    }


    /*
    private Vector3 BezierCalc(float t, Vector3 p0, Vector3 p1, Vector3 p2) {
        float u = 1-t;
        float tt = t*t;
        float uu = u * u;
        Vector3 p = uu * p0; 
        p += 2 * u * t * p1;
        p += tt * p2;
        return p;
    }*/

    void Explode()
    {

    }

    void OnTriggerEnter(Collider col)
    {
        /*if (col.CompareTag("Weapon")) {
            ChangeDirection();
        }*/
        if (col.transform.root.gameObject.CompareTag("Player") && !col.isTrigger)
        {
            Singleton.instance.PlayerHurt.Hurt(transform.position, 1);
            //ChangeDirection();
        }
        else if (!col.CompareTag("Enemy") && !col.CompareTag("Loot") && !col.isTrigger)
        {
            if (explodeOnImpact) 
            { 
                Explode(); 
            }
            StopAllCoroutines();
            Destroy(gameObject);
        }
    }
}