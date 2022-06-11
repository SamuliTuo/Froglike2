using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_ReturnShoot : MonoBehaviour {

    public UnityEngine.AI.NavMeshAgent agent;

    private NPC_Sieni_Animations animate;
    private NPC_Sieni_CombatStats combat;
    private float antiDuration, throwDuration, catchDuration;
    private float antiAnimSpeed, throwAnimSpeed, catchAnimSpeed;
    private float antiAnimStartPerc, throwAnimStartPerc, catchAnimStartPerc;
    private float rotateStartTime, rotateEndTime, rotateSpeed;
    private float t = 0;
    private float projSpeed, projOvershootDist;
    private Transform target;
    private GameObject projectile;
    private object catchAnim;
    private bool hatCatched = false;


    void Start() {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        animate = GetComponent<NPC_Sieni_Animations>();
        combat = GetComponent<NPC_Sieni_CombatStats>();
    }

    public void BoomerangAttack(
            float rotateStartTime,
            float rotateEndTime,
            float rotateSpeed,
            object antiAnim,
            float antiDuration,
            float antiAnimSpeed,
            float antiAnimStartPerc,
            object throwAnim,
            float throwDuration,
            float throwAnimSpeed,
            float throwAnimStartPerc,
            object catchAnim,
            float catchDuration,
            float catchAnimSpeed,
            float catchAnimStartPerc,
            GameObject target, 
            GameObject projectile,
            float projSpeed,
            float projOvershootDist,
            out float timeToWait) 
        {
        this.catchAnim = catchAnim;
        this.catchAnimSpeed = catchAnimSpeed;
        this.catchAnimStartPerc = catchAnimStartPerc;
        this.rotateStartTime = rotateStartTime;
        this.rotateEndTime = rotateEndTime;
        this.rotateSpeed = rotateSpeed;
        this.antiDuration = antiDuration;
        this.antiAnimSpeed = antiAnimSpeed;
        this.antiAnimStartPerc = Mathf.Clamp(antiAnimStartPerc, 0, 0.99f);
        this.throwDuration = throwDuration;
        this.throwAnimSpeed = throwAnimSpeed;
        this.throwAnimStartPerc = Mathf.Clamp(throwAnimStartPerc, 0, 0.99f);
        this.catchDuration = catchDuration;
        this.catchAnimSpeed = catchAnimSpeed;
        this.catchAnimStartPerc = Mathf.Clamp(catchAnimStartPerc, 0, 0.99f);
        this.target = target.transform;
        this.projectile = projectile;
        this.projSpeed = projSpeed;
        this.projOvershootDist = projOvershootDist;
        timeToWait = this.antiDuration + this.throwDuration + this.catchDuration;
        t = 0;
        hatCatched = false;
        StartCoroutine(ThrowBoomerang(antiAnim, throwAnim));
        //bool waitForReturnOrNo???
        //joku timeri, jonka jälkeen sieni alkaa tekemään seuraavaa actionia odottamatta hatun paluuta?
    }

    private IEnumerator ThrowBoomerang(object antiAnimation, object throwAnimation) {
        agent.isStopped = true;
        animate.PlayAnimation(antiAnimation, antiAnimStartPerc, antiAnimSpeed);
        yield return Helpers.GetWait(antiDuration);
        combat.SetCanBeInterrupted(false);
        animate.PlayAnimation(throwAnimation, throwAnimStartPerc, throwAnimSpeed);
        InstProjectile();
    }

    private void InstProjectile() {
        Vector3 spawnPos = transform.position;
        //spawnPos += transform.root.forward * 3f;
        spawnPos.y += 2f;
        var pr = Instantiate(projectile, spawnPos, Quaternion.identity);
        pr.GetComponent<DirectReturnProjectile>().GiveTarget(
            target.position, 
            gameObject, 
            projOvershootDist, 
            projSpeed);
    }

    public void CatchProjectile() {
        animate.PlayAnimation(catchAnim, catchAnimStartPerc, catchAnimSpeed);
        agent.isStopped = false;
        agent.ResetPath();
        StartCoroutine(WaitForCatchAnimationDuration());
    }
    IEnumerator WaitForCatchAnimationDuration() {
        yield return Helpers.GetWait(catchDuration);
        combat.SetCanBeInterrupted(true);
        hatCatched = true;
    }
    public bool HatCatched() {
        return hatCatched;
    }

    public bool Obstructed(Vector3 target) {
        Vector3 targetUp = target;
        targetUp.y += 0.6f;
        Vector3 dir = (targetUp - transform.position).normalized;
        float dist = Vector3.Distance(target, transform.position);
        Vector3 pos = transform.position;
        pos.y += 2f;
        if (Physics.Raycast(pos, dir, out _, dist, GetLayerMask())) {
            return true;
        }
        return false;
        /*
        if(hitBool) {
            Debug.Log(hitBool + "  dist:" + dist + "  nimi: " + hit.transform.gameObject.name);
        }
        Vector3 asdf = dir*dist;
        Debug.DrawLine(pos, target, Color.red,1f);*/
    }

    private int GetLayerMask() {
        int layerMask = (1 << 0); // | (1 << 2) | (1 << 4) | (1 << 5) | (1 << 6) | (1 << 8) | (1 << 9);
        //layerMask = ~layerMask;
        return layerMask;
    }

    public void BoomerangThrowRotation() {
        t += Time.deltaTime;
        if (t >= rotateStartTime && t < rotateEndTime) {
            Vector3 dir = target.position - transform.position;
            Quaternion lookRot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z).normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * rotateSpeed);
        }
    }

    public void ExitBoomerangThrowState() {
        StopAllCoroutines();
        agent.isStopped = false;
    }
}
