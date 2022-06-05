using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttacks : MonoBehaviour {

    [HideInInspector] public bool hijackControls = false;
    [HideInInspector] public Transform target = null;

    [SerializeField] private PlayerAbilityUpgradeHolder attackEffects = null;
    [SerializeField] private PlayerAttackScriptable groundAttack = null;
    [SerializeField] private PlayerAttackScriptable airAttack = null;
    [SerializeField] private PlayerAttackScriptable rollAttack = null;  
    [SerializeField] private float targetPosOffsetFromPlayer = 1.6f;
    [SerializeField] private float aimerRadius = 2;
    [SerializeField] private float aimerDistance = 2;

    private PlayerController control;
    private PlayerSpecialAttack special;
    private PlayerJumping jump;
    private PlayerRotate rotate;
    private PlayerRolling roll;
    private PlayerColliderChanges colliders;
    private AttackInstance currentAttackInstance;
    private PlayerAttackScriptable currentAttack = null;
    private PlayerAttackScriptable nextAttack = null;
    private SwordTrigger weaponTrigger;
    private AttackHitEffects attackHitEffects;
    private Animator anim;
    private Dictionary<string, float> animStateList = new Dictionary<string, float>();
    private Rigidbody rb;
    private Transform model;
    private bool startedMoveReturn = false;
    private bool startedRotReturn = false;
    private bool rollQueued, attackQueued, specialQueued, jumpQueued;
    private float attSpdModifier = 1;
    private float attackDuration;
    private float t;


    void Start() {
        control = GetComponent<PlayerController>();
        special = GetComponent<PlayerSpecialAttack>();
        jump = GetComponent<PlayerJumping>();
        rotate = GetComponentInChildren<PlayerRotate>();
        roll = GetComponent<PlayerRolling>();
        colliders = GetComponent<PlayerColliderChanges>();
        weaponTrigger = GetComponentInChildren<SwordTrigger>();
        attackHitEffects = GetComponent<AttackHitEffects>();
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        model = transform.GetChild(0);
        ListAttackAnimationStates();
    }

    void OnAttack() {
        if (control.state == PlayerStates.ATTACK) {
            if (currentAttack.nextAttack != null) {
                this.nextAttack = currentAttack.nextAttack;
                rollQueued = specialQueued = jumpQueued = false;
                attackQueued = true;
            }
        }
        if (control.state == PlayerStates.NORMAL) {
            InitAttack();
        }
        else if (control.state == PlayerStates.ROLL) {
            if (colliders.TryToStandUp()) {
                colliders.ChangeToStandUpColliders();
                InitAttack(rollAttack);
            }
        }
    }
    void OnRoll() {
        if (control.state == PlayerStates.ATTACK) {
            rollQueued = true;
            attackQueued = specialQueued = jumpQueued = false;
            nextAttack = null;
        }
    }
    void OnSpecial(InputValue value) {
        if (control.state == PlayerStates.ATTACK) {
            specialQueued = true;
            attackQueued = rollQueued = jumpQueued = false;
            nextAttack = null;
        }
    }
    void OnJump(InputValue value) {
        var pressed = value.isPressed;
        if (!pressed)
            return;

        if (control.state == PlayerStates.ATTACK) {
            jumpQueued = true;
            attackQueued = specialQueued = rollQueued = false;
            nextAttack = null;
        }
    }

    public void InitAttack() {
        if (control.PlayerGrounded) {
            InitAttack(groundAttack);
        }
        else if (control.PlayerGrounded == false) {
            InitAttack(airAttack);
        }
    }
    void InitAttack(PlayerAttackScriptable attack) {
        control.state = PlayerStates.ATTACK;
        currentAttack = attack;
        nextAttack = null;
        attackQueued = rollQueued = specialQueued = jumpQueued = false;
        SetAttSpdModifier();
        attackHitEffects.SetActiveUpgrades(attackEffects.activeUpgrades);
        if (attack.attackDuration == 0) {
            foreach (var clip in animStateList) {
                if (clip.Key == attack.animatorStateName) {
                    this.attackDuration = clip.Value;
                    break;
                }
            }
        }
        else {
            this.attackDuration = attack.attackDuration;
        }
        t = 0;
        target = null;
        //startPos = transform.position;
        model.rotation = Quaternion.LookRotation(AimAndChooseTarget(aimerRadius, aimerDistance));
        if (target != null) {
            hijackControls = true;
            control.SetVelocity(Vector3.zero);
            rb.velocity = Vector3.zero;
        }
        else if (control.GetInput().sqrMagnitude > control.deadzoneSquared) {
            control.SetVelocity(model.forward * currentAttack.stepForce);
        }
        startedMoveReturn = false;
        anim.Play(attack.animatorStateName, 0, attack.animationStartPerc);

        // Slash banana:
        currentAttackInstance = Singleton.instance.PlayerAttackSpawner.SpawnAttack(
            currentAttack.spawnOffset,
            Quaternion.LookRotation(model.forward, Vector3.up) * Quaternion.Euler(currentAttack.rotation),
            currentAttack.width,
            currentAttack.length,
            currentAttack.growSpeed,
            currentAttack.flySpeed,
            currentAttack.lifeTime, 
            currentAttack.spawnDelay, 
            1);
        StartCoroutine(UpdateAttack(attack));
    }

    public void AttackWithTarget() {
        if (t < attackDuration * currentAttack.attackHomingEndPerc * attSpdModifier)
        {
            Vector3 targetPosWithOffset =
                target.position - (target.position - new Vector3(
                    transform.position.x, target.position.y, transform.position.z)).normalized *
                targetPosOffsetFromPlayer;
            Vector3 dirLen = targetPosWithOffset - transform.position;
            control.SetVelocity(dirLen * 1000 * Time.deltaTime);
            model.rotation = Quaternion.LookRotation(new Vector3(
                target.position.x - transform.position.x,
                0,
                target.position.z - transform.position.z).normalized);
        }
        else
        {
            control.SetVelocity(Vector3.zero);
        }
    }

    public Vector3 AttackWithoutTargetVector3(
        Vector3 xAxis, float currX,
        Vector3 zAxis, float currZ) 
    {
        float perc = 0;
        if (t < currentAttack.stepDuration * attSpdModifier) {
            perc = 1 - t / (currentAttack.stepDuration * attSpdModifier);
        }
        float desiredX = (model.forward * currentAttack.stepForce).x * perc * (2 - attSpdModifier);
        float desiredZ = (model.forward * currentAttack.stepForce).z * perc * (2 - attSpdModifier);
        Vector2 currentVelo = new Vector2(currX, currZ);
        Vector2 desiredVelo = new Vector2(desiredX, desiredZ);
        Vector2 newVelo = Vector2.MoveTowards(currentVelo, desiredVelo, 100);
        return xAxis * (newVelo.x - currX) + zAxis * (newVelo.y - currZ);
    }

    IEnumerator UpdateAttack(PlayerAttackScriptable attack) {
        while (t < attackDuration * attSpdModifier) {
            t += Time.deltaTime;
            anim.speed = 2 - attSpdModifier;

            // Weapon colliders:
            if (t >= attack.weaponColActivatePerc * attackDuration * attSpdModifier && 
                t < attack.weaponColDeactivatePerc * attackDuration * attSpdModifier &&
                weaponTrigger.triggerEnabled == false) 
            {
                weaponTrigger.ColliderOn(currentAttackInstance);
            }
            else if (t >= attack.weaponColDeactivatePerc * attackDuration * attSpdModifier &&
                weaponTrigger.triggerEnabled == true) 
            {
                weaponTrigger.ColliderOff();
            }

            // Movement:
            if (t < attack.moveReturnPerc * attackDuration * attSpdModifier) {
                control.SetAccelerationMod(0);
            }
            else if (!startedMoveReturn) {
                control.InitAccelerationModReturn(attack.moveReturnDuration, false);
                hijackControls = false;
                startedMoveReturn = true;
            }

            // Rotation:
            if (t < attack.rotReturnDuration * attackDuration * attSpdModifier) {
                rotate.SetRotateSpdMod(0);
            }
            else if (!startedRotReturn) {
                rotate.InitRotateSpdModReturn(attack.rotReturnDuration);
            }

            // If action queued, start it:
            if (t > attack.nextAttackInitPerc * attackDuration * attSpdModifier) {
                if (attackQueued) {
                    if (nextAttack != null) {
                        anim.speed = 1;
                        weaponTrigger.ColliderOff();
                        InitAttack(nextAttack);
                        yield break;
                    }
                }
                else if (specialQueued) {
                    anim.speed = 1;
                    weaponTrigger.ColliderOff();
                    special.StartBaseballSamurai();
                    yield break;
                }
                else if (rollQueued) {
                    anim.speed = 1;
                    weaponTrigger.ColliderOff();
                    hijackControls = false;
                    roll.InitRoll();
                    yield break;
                }
                else if (jumpQueued)
                {
                    anim.speed = 1;
                    weaponTrigger.ColliderOff();
                    hijackControls = false;
                    control.state = PlayerStates.NORMAL;
                    control.InitAccelerationModReturn(0.3f, false);
                    rotate.InitRotateSpdModReturn(0.3f);
                    jump.InitNormalJump();
                    yield break;
                }
            }
            yield return null;
        }
        anim.speed = 1;
        control.state = PlayerStates.NORMAL;
        StopAttacks();
    }

    public void StopAttacks() {
        StopAllCoroutines();
        t = 0;
        weaponTrigger.ColliderOff();
        hijackControls = false;
        currentAttack = null;
        nextAttack = null;
        attackQueued = specialQueued = rollQueued = false;
    }

    /// <summary>
    /// kun edellinen lyönti on loppunut ja ollaan palauttamassa rotaatiota pelaajalle, toi auto-aim ei toimi vaan se preferoi sitä "rotation-palautusta"
    /// THIS IS UNACCEPTABLE. CHANGE IT CHANGE IT!!!
    /// </summary>
    /// <returns></returns>

    public Vector3 AimAndChooseTarget(float overlapSphereRadius, float overlapSphereDistance) {
        Vector3 inputDir = control.GetInput();
        Vector3 dir = inputDir.sqrMagnitude > control.deadzoneSquared ? inputDir : model.forward;
        Vector3 endPoint = transform.position + dir * overlapSphereDistance;
        Collider[] targets = Physics.OverlapSphere(endPoint, overlapSphereRadius);
        target = null;
        float targetDistToPlr = 10;
        //if (inputDir.sqrMagnitude > control.deadzoneSquared) {
        float targetDistToLine = 10;
        foreach (var c in targets) {
            if (c.CompareTag("Enemy")) {
                float cDistToLine = MathUtils.DistanceLineSegmentPoint(
                    transform.position, endPoint, c.transform.position);
                float cDistToPlr = (transform.position - c.transform.position).magnitude;
                if (cDistToLine < targetDistToLine || cDistToLine < 0.7f) {
                    if (targetDistToLine < 0.7f) {
                        if (cDistToPlr > targetDistToPlr) {
                            continue;
                        }
                    }
                    target = c.transform;
                    targetDistToLine = cDistToLine;
                    targetDistToPlr = cDistToPlr;
                }
            }
        }
        if (target != null) {
            dir = new Vector3(
                target.position.x - transform.position.x, 
                0, 
                target.position.z - transform.position.z).normalized;
        }
        return dir;
    }

    void ListAttackAnimationStates() {
        animStateList.Add("attack_rose01", 0.792f);
        animStateList.Add("attack_rose02", 0.625f);
        animStateList.Add("attack_rose03", 0.583f);
        /*AnimatorController ac = anim.runtimeAnimatorController as AnimatorController;
        AnimatorControllerLayer[] acLayers = ac.layers;
        foreach (AnimatorControllerLayer layer in acLayers) {
            ChildAnimatorState[] animStates = layer.stateMachine.states;
            for (int i = 0; i < animStates.Length; i++) {
                animStateList.Add(
                    animStates[i].state.name, animStates[i].state.motion.averageDuration);
            }
        }*/
    }

    void SetAttSpdModifier() {
        attSpdModifier = 1;
        for (int i = 0; i < attackEffects.activeUpgrades.Count; i++) {
            switch (attackEffects.activeUpgrades[i]) {
                case UpgradeType.GROUND:
                    attSpdModifier += 0.166f;
                    break;
                case UpgradeType.AIR:
                    break;
                case UpgradeType.SPARK:
                    attSpdModifier -= 0.166f;
                    break;
                default:
                    break;
            }
        }
        if (attSpdModifier < 0.5f) {
            attSpdModifier = 0.5f;
        }
        if (attSpdModifier > 1.5f) {
            attSpdModifier = 1.5f;
        }
    }
    
    /*
    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + transform.GetChild(0).forward * aimerDistance, aimerRadius);
    }
    */
}
