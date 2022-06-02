using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_Attack_Stomp : MonoBehaviour {

    public UnityEngine.AI.NavMeshAgent agent;
    NPC_Movement _move;
    NPC_Sieni_CombatStats _combat;
    NPC_Sieni_Animations _anim;
    Coroutine attackCoroutine;
    [SerializeField] float stompCooldown = 5;
    Transform target;
    bool canStomp = true;
    bool canRotate = false;
    float rotateSpeed = 1000;
    float jumpMoveSpeed;
    float antiDuration, jumpDuration, endDuration;
    float antiAnimSpeed, jumpAnimSpeed, endAnimSpeed;
    float antiAnimStartPerc, jumpAnimStartPerc, endAnimStartPerc;
    float hitboxStartTime, hitboxDuration;

    void Start() {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        _move = GetComponent<NPC_Movement>();
        _combat = GetComponent<NPC_Sieni_CombatStats>();
        _anim = GetComponent<NPC_Sieni_Animations>();
    }

    public bool ToStompOrNotToStomp(float probability) {
        if (canStomp) {
            if (Random.Range(0.000f, 1.000f) <= probability) {
                return true;
            }
        }
        return false;
    }

    public void StompAttack(
            object antiAnim,
            float antiDuration,
            float antiAnimSpeed,
            float antiAnimStartPerc,
            object jumpAnim,
            float jumpMoveSpeed,
            float jumpDuration,
            float jumpAnimSpeed,
            float jumpAnimStartPerc,
            object endAnim,
            float endDuration,
            float endAnimSpeed,
            float endAnimStartPerc,
            GameObject target,
            float hitboxStartTime,
            float hitboxDuration,
            out float timeToWait) 
        {
        this.antiDuration = antiDuration;
        this.antiAnimSpeed = antiAnimSpeed;
        this.antiAnimStartPerc = Mathf.Clamp(antiAnimStartPerc, 0, 0.99f);
        this.jumpMoveSpeed = jumpMoveSpeed;
        this.jumpDuration = jumpDuration;
        this.jumpAnimSpeed = jumpAnimSpeed;
        this.jumpAnimStartPerc = Mathf.Clamp(jumpAnimStartPerc, 0, 0.99f);
        this.hitboxStartTime = hitboxStartTime;
        this.hitboxDuration = hitboxDuration;
        this.endDuration = endDuration;
        this.endAnimSpeed = endAnimSpeed;
        this.endAnimStartPerc = Mathf.Clamp(endAnimStartPerc, 0, 0.99f);
        this.target = target.transform;
        timeToWait = this.antiDuration + this.jumpDuration + this.endDuration;
        attackCoroutine = StartCoroutine(StompAttack_(antiAnim, jumpAnim, endAnim));
        StartCoroutine(StompCooldown());
        StartCoroutine(StompAttackHitboxes());
    }

    IEnumerator StompAttack_ (object antiAnim, object jumpAnim, object endAnim) {
        // ANTICIPATION
        _combat.SetCanBeInterrupted(true);
        canRotate = true;
        agent.isStopped = true;
        //GeneralSFX.current.ShroomBasicJump(transform.position);
        _anim.PlayAnimation(antiAnim, antiAnimStartPerc, antiAnimSpeed);
        yield return Helpers.GetWait(antiDuration);
        // JUMP
        _combat.SetCanBeInterrupted(false);
        canRotate = false;
        agent.isStopped = false;
        _move.SetAcceleration(100f);
        float spd = Mathf.Min((target.transform.position - transform.position).magnitude * jumpDuration * 1.4f, jumpMoveSpeed);
        _move.MoveTo(target.transform.position, spd);
        _anim.PlayAnimation(jumpAnim, jumpAnimStartPerc, jumpAnimSpeed);
        yield return Helpers.GetWait(jumpDuration * 0.6f);
        agent.isStopped = true;
        //GeneralSFX.current.ShroomBasicGrunt(transform.position);
        yield return Helpers.GetWait(jumpDuration * 0.4f);
        // END
        _combat.SetCanBeInterrupted(true);
        _anim.PlayAnimation(endAnim, endAnimStartPerc, endAnimSpeed);
        yield return Helpers.GetWait(endDuration);
    }

    public void StompRotation() {
        if (canRotate) {
            Vector3 dir = target.position - transform.position;
            Quaternion lookRot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z).normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * rotateSpeed);
        }
    }

    IEnumerator StompAttackHitboxes() {
        yield return Helpers.GetWait(hitboxStartTime);
        _combat.SetHurtboxersState(true);
        yield return Helpers.GetWait(hitboxDuration);
        _combat.SetHurtboxersState(false);
    }

    IEnumerator StompCooldown() {
        canStomp = false;
        yield return Helpers.GetWait(stompCooldown);
        canStomp = true;
    }

    public void ExitStompState() {
        StopCoroutine(StompAttackHitboxes());
        StopCoroutine(attackCoroutine);
        _combat.SetHurtboxersState(false);
        canRotate = false;
        _combat.SetCanBeInterrupted(true);
        agent.isStopped = false;
        _move.SetAcceleration(_move.GetStartAcceleration());
        agent.speed = _move.GetStartSpeed();
    }
}
