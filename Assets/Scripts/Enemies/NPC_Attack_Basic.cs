using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

public class NPC_Attack_Basic : MonoBehaviour {

    public UnityEngine.AI.NavMeshAgent agent;

    //public GameObject targetObj; //player
    private NPC_Sieni_Animations anim;
    private NPC_Sieni_CombatStats combat;
    //public GameObject dmgBox;
    private float t = 0;
    private float antiDuration, swingDuration, endDuration;
    private float antiAnimSpeed, swingAnimSpeed, endAnimSpeed;
    private float antiAnimStartPerc, swingAnimStartPerc, endAnimStartPerc;
    private float hitboxStartTime, hitboxDuration;
    private float rotateStartTime, rotateEndTime, rotateSpeed;
    private Transform target;


    void Start() {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        anim = GetComponent<NPC_Sieni_Animations>();
        combat = GetComponent<NPC_Sieni_CombatStats>();
    }

    public void BasicAttack(
            float rotateStartTime,
            float rotateEndTime,
            float rotateSpeed,
            object antiAnim,
            float antiDuration,
            float antiAnimSpeed,
            float antiAnimStartPerc,
            object swingAnim,
            float swingDuration,
            float swingAnimSpeed,
            float swingAnimStartPerc,
            object endAnim,
            float endDuration,
            float endAnimSpeed,
            float endAnimStartPerc,
            GameObject target,
            float hitboxStartTime,
            float hitboxDuration,
            out float timeToWait) 
        {
        if (target == null) {
            timeToWait = 0;
            return;
        }
        this.rotateStartTime = rotateStartTime;
        this.rotateEndTime = rotateEndTime;
        this.rotateSpeed = rotateSpeed;
        this.antiDuration = antiDuration;
        this.antiAnimSpeed = antiAnimSpeed;
        this.antiAnimStartPerc = Mathf.Clamp(antiAnimStartPerc, 0, 0.99f);
        this.swingDuration = swingDuration;
        this.swingAnimSpeed = swingAnimSpeed;
        this.swingAnimStartPerc = Mathf.Clamp(swingAnimStartPerc, 0, 0.99f);
        this.hitboxStartTime = hitboxStartTime;
        this.hitboxDuration = hitboxDuration;
        this.endDuration = endDuration;
        this.endAnimSpeed = endAnimSpeed;
        this.endAnimStartPerc = Mathf.Clamp(endAnimStartPerc, 0, 0.99f);
        this.target = target.transform;
        timeToWait = this.antiDuration + this.swingDuration + this.endDuration;
        t = 0;
        StartCoroutine(BasicSlap(antiAnim, swingAnim, endAnim));
        StartCoroutine(BasicAttackHitboxes());
    }

    IEnumerator BasicSlap(object antiAnim, object swingAnim, object endAnim) {
        agent.isStopped = true;
        //GeneralSFX.current.ShroomBasicAttack(transform.position);
        anim.PlayAnimation(antiAnim, antiAnimStartPerc, antiAnimSpeed);
        yield return Helpers.GetWait(antiDuration);
        anim.PlayAnimation(swingAnim, swingAnimStartPerc, swingAnimSpeed);
        yield return Helpers.GetWait(swingDuration);
        anim.PlayAnimation(endAnim, endAnimStartPerc, endAnimSpeed);
        yield return Helpers.GetWait(endDuration);
    }
    
    IEnumerator BasicAttackHitboxes() {
        yield return Helpers.GetWait(hitboxStartTime);
        combat.SetHurtboxersState(true);
        yield return Helpers.GetWait(hitboxDuration);
        combat.SetHurtboxersState(false);
    }

    public void AttackRotation() {
        t += Time.deltaTime;
        if (t >= rotateStartTime && t < rotateEndTime) {
            Vector3 dir = target.position - transform.position;
            Quaternion lookRot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z).normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * rotateSpeed);
        }
    }

    public void ExitBasicAttackState() {
        StopAllCoroutines();
        combat.SetHurtboxersState(false);
        agent.isStopped = false;
    }

    /*
    void InstBasicAttack() {
        Vector3 pos = this.targetPos;
        var dmgInstance = Instantiate(dmgBox, pos, new Quaternion(0,0,0,0));
        dmgInstance.transform.parent = gameObject.transform.parent;
    }

    public void InstBasicAttack(GameObject damageInstance, Vector3 position) {
        var dmgInstance = Instantiate(damageInstance, position, new Quaternion(0,0,0,0));
        dmgInstance.transform.parent = gameObject.transform.parent;
    }

    public void SetTargetObj(GameObject targetObj) {
        this.targetObj = targetObj;
    }*/
}
