using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_Shoot : MonoBehaviour {

    public UnityEngine.AI.NavMeshAgent agent;

    private NPC_Sieni_Animations animate;
    private NPC_Sieni_CombatStats combat;
    private float antiDuration, shootDuration, endDuration;
    private float antiAnimSpeed, shootAnimSpeed, endAnimSpeed;
    private float antiAnimStartPerc, shootAnimStartPerc, endAnimStartPerc;
    private float rotateStartTime, rotateEndTime, rotateSpeed;
    private float t = 0;
    private float projSpeed, projSpeedUp, projOvershootDist;
    private Transform target;
    private GameObject projectile;
    private float accuracy;


    void Start() {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        animate = GetComponent<NPC_Sieni_Animations>();
        combat = GetComponent<NPC_Sieni_CombatStats>();
    }

    public void Shoot(
            bool useGravity,
            float rotateStartTime,
            float rotateEndTime,
            float rotateSpeed,
            object antiAnim,
            float antiDuration,
            float antiAnimSpeed,
            float antiAnimStartPerc,
            object shootAnim,
            float shootDuration,
            float shootAnimSpeed,
            float shootAnimStartPerc,
            object endAnim,
            float endDuration,
            float endAnimSpeed,
            float endAnimStartPerc,
            GameObject target, 
            GameObject projectile,
            float projSpeed,
            float projSpeedUp,
            float projOvershootDist,
            float accuracy,
            out float timeToWait) 
        {
        this.endAnimSpeed = endAnimSpeed;
        this.endAnimStartPerc = endAnimStartPerc;
        this.rotateStartTime = rotateStartTime;
        this.rotateEndTime = rotateEndTime;
        this.rotateSpeed = rotateSpeed;
        this.antiDuration = antiDuration;
        this.antiAnimSpeed = antiAnimSpeed;
        this.antiAnimStartPerc = Mathf.Clamp(antiAnimStartPerc, 0, 0.99f);
        this.shootDuration = shootDuration;
        this.shootAnimSpeed = shootAnimSpeed;
        this.shootAnimStartPerc = Mathf.Clamp(shootAnimStartPerc, 0, 0.99f);
        this.endDuration = endDuration;
        this.endAnimSpeed = endAnimSpeed;
        this.endAnimStartPerc = Mathf.Clamp(endAnimStartPerc, 0, 0.99f);
        this.target = target.transform;
        this.projectile = projectile;
        this.projSpeed = projSpeed;
        this.projSpeedUp = projSpeedUp;
        this.projOvershootDist = projOvershootDist;
        timeToWait = this.antiDuration + this.shootDuration + this.endDuration;
        t = 0;
        StartCoroutine(Shoot(antiAnim, shootAnim, endAnim));
        //joku timeri, jonka jälkeen sieni alkaa tekemään seuraavaa actionia odottamatta hatun paluuta?
    }

    private IEnumerator Shoot(object antiAnimation, object shootAnim, object endAnim) {
        agent.isStopped = true;
        animate.PlayAnimation(antiAnimation, antiAnimStartPerc, antiAnimSpeed);
        yield return Helpers.GetWait(antiDuration);
        combat.SetCanBeInterrupted(false);
        animate.PlayAnimation(shootAnim, shootAnimStartPerc, shootAnimSpeed);
        InstProjectile();
        yield return Helpers.GetWait(shootDuration);
        combat.SetCanBeInterrupted(true);
        animate.PlayAnimation(endAnim, endAnimStartPerc, endAnimSpeed);
    }

    private void InstProjectile() {
        Vector3 spawnPos = transform.position;
        //spawnPos += transform.root.forward * 3f;
        spawnPos.y += 2f;
        var cloneScript = Instantiate(projectile, spawnPos, Quaternion.identity).
            GetComponent<EnemyProjectile>();
        cloneScript.InitProjectile(target.position, gameObject, false, 3);
        cloneScript.ShootProjectile(accuracy, projSpeed, projSpeedUp);
            /*
            target.position, 
            gameObject, 
            projOvershootDist, 
            projSpeed);*/
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

    public void ShootRotation() {
        t += Time.deltaTime;
        if (t >= rotateStartTime && t < rotateEndTime) {
            Vector3 dir = target.position - transform.position;
            Quaternion lookRot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z).normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * rotateSpeed);
        }
    }

    public void ExitShootState() {
        StopAllCoroutines();
        agent.isStopped = false;
    }
}
