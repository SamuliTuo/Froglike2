using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectReturnProjectile : MonoBehaviour {

    [SerializeField] private float minimumThrowDist = 5f;

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
    private float totalDist;
    private float overshoot;
    private Vector3 startPos;
    private Vector3 endPos;
    private float t;


    void FixedUpdate() {
        ProjectileMovement();
        Disappear();
        t += Time.deltaTime;
    }

    private void ProjectileMovement() {
        float perc = t / duration;
        if (!halfDone) {
            t += Time.deltaTime;
            perc = Mathf.Sin(perc * Mathf.PI * 0.5f);
            transform.root.position = Vector3.Lerp(startPos, endPos, perc);
            if (t >= duration) {
                ChangeDirection();
            }
        }
        else {
            perc = 1f - Mathf.Cos(perc * Mathf.PI * 0.5f);
            transform.root.position = Vector3.Lerp(startPos, endPos, perc);
        }
        
    }
    /*
    private void Returning() {
        if (shooter == null) {
            Destroy(gameObject);
            return;
        }
        transform.root.position = Vector3.Lerp(projEnd, shooter.transform.position, tParam / duration);
    }*/

    private void Disappear() {
        if (shooter == null) {
            Destroy(gameObject);
            return;
        }
        if (halfDone == true && t >= duration) {
            shooter.GetComponent<NPC_ReturnShoot>().CatchProjectile();
            Destroy(gameObject);
        }
    }
    /*
    private float bigF(float pct, float speed) {
        float res = speed * pct;
        return res;
    }

    private float bigFComplement(float pct, float speed) {
        float res = speed * (1f - pct);
        return res;
    }*/

    //target = pelaajapositio
    void ShootCalc() {
        Vector3 dir = (shooter.transform.position - this.target).normalized;
        projStart = transform.root.position;
        projEnd = target;
        projEnd += -dir * overshoot;
        //projHandle = projEnd - (dir * 3f);
        //Instantiate(debug, projEnd, Quaternion.identity);
        projEnd.y += 1f;
        startPos = projStart;
        endPos = projEnd;
    }

    private void DistanceBasedFlightDuration() {
        float dist = totalDist;
        float additive;
        if(dist <= 10f) {
            additive = -0.25f;
        } else if (dist <= 15f) {
            additive = dist * 0.05f - 0.75f;
        } else {
            additive = 0.25f;
        }
        duration += additive;
        //print("totaldist: "+totalDist +"dur: "+duration);
    }

    public void GiveTarget(Vector3 target, GameObject shooter, float overshoot, float projSpeed) {
        //this.projSpeed = projSpeed;
        this.overshoot = overshoot;
        this.target = target;
        this.shooter = shooter;
        ShootCalc();
        t = 0f;
        halfDone = false;
        //maxT = 2f;
        totalDist = Mathf.Max(Vector3.Distance(projEnd, transform.position), minimumThrowDist);
        DistanceBasedFlightDuration();
    }

    private void ChangeDirection() {
        halfDone = true;
        t = 0f;
        startPos = transform.root.position;
        endPos = shooter.transform.position;
        endPos.y += 2f;
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

    void OnTriggerEnter(Collider col) {
        /*if (col.CompareTag("Weapon")) {
            ChangeDirection();
        }*/
        if (col.transform.root.gameObject.CompareTag("Player") && !col.isTrigger) {
            Singleton.instance.PlayerHurt.Hurt(transform.position, 1);
            //ChangeDirection();
        }
        else if (!col.CompareTag("Enemy") && !col.CompareTag("Loot") && !col.isTrigger) {
            ChangeDirection();
        }
    }
}
