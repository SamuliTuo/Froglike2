using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttacks : MonoBehaviour {

    [HideInInspector] public bool hijackControls = false;
    [HideInInspector] public Transform target = null;

    [SerializeField] private PlayerAbilityUpgradeHolder attackEffects = null;
    public PlayerAttackScriptable groundAttack;
    public PlayerAttackScriptable airAttack;
    public PlayerAttackScriptable rollAttack;
    [SerializeField] private float airAttackStepForceVertical = 1;
    [SerializeField] private float airAttackStopForceHorizontal = 1;
    [SerializeField] private float targetPosOffsetFromPlayer = 1.6f;
    [SerializeField] private float aimerRadius = 2;
    [SerializeField] private float aimerDistance = 2;

    private WeaponBuffController weaponBuff;
    private PlayerController control;
    private PlayerGravity gravity;
    private PlayerSpecialAttack special;
    private PlayerJumping jump;
    private PlayerMouthController mouth;
    private TongueController tongue;
    private PlayerRotate rotate;
    private PlayerRolling roll;
    private PlayerColliderChanges colliders;
    private AttackInstance currentAttackInstance;
    public PlayerAttackScriptable currentAttack = null;
    public PlayerAttackScriptable nextAttack = null;
    private SwordTrigger weaponTrigger;
    private AttackHitEffects attackHitEffects;
    private Animator anim;
    private Dictionary<string, float> animStateList = new Dictionary<string, float>();
    private Rigidbody rb;
    private Transform model;
    private bool startedMoveReturn = false;
    private bool startedRotReturn = false;
    private float attSpdModifier = 1;
    private float attackDuration;
    private float t;


    void Start() {
        print("trying to do a NO STEP attack ova here!"); // rivillä 137 tarkemmin
        weaponBuff = GetComponentInChildren<WeaponBuffController>();
        control = GetComponent<PlayerController>();
        gravity = GetComponent<PlayerGravity>();
        special = GetComponent<PlayerSpecialAttack>();
        jump = GetComponent<PlayerJumping>();
        mouth = GetComponent<PlayerMouthController>();
        tongue = GetComponent<TongueController>();
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
        if (control.state == PlayerStates.NORMAL) {
            InitAttack();
        }
        else if (control.state == PlayerStates.ROLL) {
            if (colliders.TryToStandUp())
            {
                StartCoroutine(jump.JustJumped());
                if (roll.continuousRoll)
                {
                    roll.StopRoll();
                    colliders.ChangeToStandUpColliders();
                    InitAttack(rollAttack);
                }
                else
                {
                    roll.StopRoll(true);
                    colliders.ChangeToStandUpColliders();
                    InitAttack();
                }
            }
        }
    }

    public void InitAttack() {
        if (control.PlayerGrounded == false) {
            takeAirStep = true;
            InitAttack(airAttack);
        }
        else if (control.PlayerGrounded || control.stepsSinceLastGrounded < jump.maxLateJumpTimeSteps)  {
            InitAttack(groundAttack);
        }
    }
    private Enemy_TongueInteraction targetScript = null;
    private float inputMagWhileStartingAttack = 0;
    void InitAttack(PlayerAttackScriptable attack) {
        inputMagWhileStartingAttack = control.GetInput().magnitude;
        control.state = PlayerStates.ATTACK;
        currentAttack = attack;
        control.StopAfterSpecialGravity();
        nextAttack = null;
        control.ResetQueuedAction();
        SetAttSpdModifier();
        attackHitEffects.SetActiveUpgrades(weaponBuff.activeBuff);
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
        targetScript = null;

        //startPos = transform.position;
        if (control.GetInput().sqrMagnitude > control.deadzoneSquared)
        {
            ///////////////////////////////
            /// puuuu.. tee askeleeton-versio lyönneistä
        }
        var lookRot = AimAndChooseTarget(aimerRadius, aimerDistance);
        model.rotation = Quaternion.LookRotation(new(lookRot.x, 0, lookRot.z));

        if (attack == rollAttack)
        {
            print("is this roll attack!" + control.GetInput().magnitude);
            Vector3 rollNewVelo = model.forward * currentAttack.stepForce * control.GetInput().magnitude * 0 + transform.up * currentAttack.stepForceUp;
            roll.rollDir = rollNewVelo * 5;
            control.SetNextVelo(rollNewVelo);
        }

        else if (target != null) {
            switch (targetScript.enemySize)
            {
                case EnemySizes.SMALL:
                    targetPosOffsetFromPlayer = 1.5f; break;
                case EnemySizes.MEDIUM:
                    targetPosOffsetFromPlayer = 2f; break;
                case EnemySizes.LARGE:
                    targetPosOffsetFromPlayer = 2.5f; break;
                default:
                    break;
            }
            hijackControls = true;
            control.SetVelocity(Vector3.zero);
            rb.velocity = Vector3.zero;
        }
        else if (!control.PlayerGrounded && control.GetInput().sqrMagnitude > control.deadzoneSquared) 
        {    
            control.SetVelocity(model.forward * currentAttack.stepForce);
        }

        startedMoveReturn = false;
        anim.Play(attack.animatorStateName, 0, attack.animationStartPerc);
        currentAttackInstance = Singleton.instance.PlayerAttackSpawner.SpawnAttack(currentAttack, model);
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

    bool takeAirStep = false;
    public Vector3 AttackWithoutTargetVector3(
        Vector3 xAxis, float currX,
        Vector3 zAxis, float currZ) 
    {
        if (takeAirStep && currentAttack != rollAttack)
        {
            print("time for AIRSTEP!");
            takeAirStep = false;
            control.SetVelocity(Vector3.zero);
            control.airStop = true;
            Vector3 veloAdd = Vector3.down * airAttackStepForceVertical + model.forward * airAttackStopForceHorizontal;//Vector3.down * rb.velocity.y + Vector3.down * airAttackStepForceVertical + model.forward * airAttackStopForceHorizontal;
            gravity.StopJumpCoroutines();
            return veloAdd;
        }

        float perc = 0;
        if (t < currentAttack.stepDuration * attSpdModifier) {
            perc = 1 - t / (currentAttack.stepDuration * attSpdModifier);
        }
        float desiredX =
            (model.forward * Mathf.Lerp(currentAttack.stepEndForce, currentAttack.stepForce, perc)).x *
            (2 - attSpdModifier) *
            inputMagWhileStartingAttack;
        float desiredZ =
            (model.forward * Mathf.Lerp(currentAttack.stepEndForce, currentAttack.stepForce, perc)).z *
            (2 - attSpdModifier) *
            inputMagWhileStartingAttack;

        Vector2 currentVelo = new Vector2(currX, currZ);
        Vector2 desiredVelo = new Vector2(desiredX, desiredZ);
        Vector2 newVelo;

        if (currentAttack == rollAttack)
            newVelo = Vector2.MoveTowards(currentVelo, desiredVelo, 0.9f);
        else if (control.PlayerGrounded)
            newVelo = Vector2.MoveTowards(currentVelo, desiredVelo, 100);
        else
            newVelo = Vector2.MoveTowards(currentVelo, desiredVelo, 0.1f);
        
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

            // Roll attack
            if (attack == rollAttack)
            {
                //control.SetNextVelo(model.forward * currentAttack.stepForce + transform.up * currentAttack.stepForceUp);
                yield return null;
            }

            // Movement:
            else if (t < attack.moveReturnPerc * attackDuration * attSpdModifier)
            {
                control.SetAccelerationMod(0);
            }
            else if (!startedMoveReturn)
            {
                control.InitAccelerationModReturn(attack.moveReturnDuration, false);
                hijackControls = false;
                startedMoveReturn = true;
            }

            // Rotation:
            if (t < attack.rotReturnDuration * attackDuration * attSpdModifier)
            {
                rotate.SetRotateSpdMod(0);
            }
            else if (!startedRotReturn)
            {
                rotate.InitRotateSpdModReturn(attack.rotReturnDuration);
            }

            // If action queued, start it:
            if (t > attack.nextAttackInitPerc * attackDuration * attSpdModifier)
            {
                if (control.IsQueuedAction(QueuedAction.ATTACK))
                {
                    nextAttack = currentAttack.nextAttack;
                    if (nextAttack != null)
                    {
                        anim.speed = 1;
                        takeAirStep = false;
                        weaponTrigger.ColliderOff();
                        InitAttack(nextAttack);
                        yield break;
                    }
                }
                else if (attack == rollAttack && !control.IsQueuedAction(QueuedAction.SPECIAL)) // && control.GetInput().magnitude > control.deadzone)
                {
                    roll.ContinueCRoll();
                    yield break;
                }
                else if (control.IsQueuedAction(QueuedAction.NULL) == false)
                {
                    roll.StopRoll(true);
                    anim.speed = 1;
                    weaponTrigger.ColliderOff();
                    hijackControls = takeAirStep = false;
                    nextAttack = currentAttack = null;
                    control.InitQueuedAction();
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
        takeAirStep = hijackControls = false;
        currentAttack = nextAttack = null;
        control.ResetQueuedAction();
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
            //dir = (transform.position - transform.position).normalized;
            targetScript = target.root.GetComponent<Enemy_TongueInteraction>();
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
