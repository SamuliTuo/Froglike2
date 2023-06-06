using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpecialAttack : MonoBehaviour {

    public PlayerSpecialScriptable attackScript = null;
    [HideInInspector] public bool hijackControls = false;

    [Header("Ground Pound")]
    [SerializeField] private float groundPoundInitialStopDuration = 0.4f;
    [SerializeField] private float groundPoundSpeed = 30f;
    private PlayerController control;
    private PlayerGravity gravity;
    private PlayerAttacks attack;
    private PlayerJumping jump;
    private PlayerStamina stamina;
    private SwordTrigger swordCol;
    private SpecialTrigger specialCol;
    private PlayerRolling roll;
    private PlayerCrawl crawl;
    private PlayerRotate rotate;
    private PlayerColliderChanges colliders;
    private AttackInstance currentAttackInstance;
    private Animator anim;
    private Rigidbody rb;
    private Collider col;
    private Transform model;
    private Vector3 groundPoundLastLocation;
    private NumberType attackType;
    private bool buttonHeld = false;
    private bool applyStartForce = true;
    private bool takeStep = false;
    private bool takeAimedStep = false;
    private bool groundPoundEnding = false;
    private bool charging = false;
    private float t, currentCharge, perc;

    Vector3 targetDir;
    Collider[] nearEnemyCols;


    void Start() 
        {
        control = GetComponent<PlayerController>();
        gravity = GetComponent<PlayerGravity>();
        attack = GetComponent<PlayerAttacks>();
        jump = GetComponent<PlayerJumping>();
        stamina = GetComponent<PlayerStamina>();
        swordCol = GetComponentInChildren<SwordTrigger>();
        specialCol = GetComponentInChildren<SpecialTrigger>();
        roll = GetComponent<PlayerRolling>();
        crawl = GetComponent<PlayerCrawl>();
        rotate = GetComponentInChildren<PlayerRotate>();
        colliders = GetComponent<PlayerColliderChanges>();
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        model = transform.GetChild(0);
    }

    // Input listeners
    void OnSpecial(InputValue input) 
    {
        buttonHeld = input.isPressed;
        if (!buttonHeld)
            return;

        if (control.state == PlayerStates.GROUND_POUND ||
            control.state == PlayerStates.BASEBALL ||
            control.state == PlayerStates.SAMURAI ||
            control.state == PlayerStates.ROLL_SLASH) 
        {
            return;
        }

        ChooseAndStartSpecial();
    }

    void ChooseAndStartSpecial()
    {
        if (stamina.HasStamina(attackScript.initialStaminaCost) == false)
        {
            stamina.FlashStaminaBar();
            return;
        }

        if (control.state == PlayerStates.NORMAL &&
            (control.PlayerGrounded || control.stepsSinceLastGrounded < 3))
        {
            StartBaseballSamurai(PlayerStates.BASEBALL);
        }
        else if (control.state == PlayerStates.ROLL || (control.state == PlayerStates.NORMAL && control.stepsSinceLastGrounded >= 3))
        {
            roll.StopRoll();
            StartBaseballSamurai(PlayerStates.SAMURAI);
        }
    }

    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    public void SpecialFixedUpdate() 
    {
        if (control.state == PlayerStates.GROUND_POUND)
            GroundPoundFixed();
        else if ((control.state == PlayerStates.BASEBALL || control.state == PlayerStates.SAMURAI) && hijackControls)
            BaseballSamuraiMovementWithTarget();
    }

    public void ResetSpecial()
    {
        StopAllCoroutines();
        Time.timeScale = 1;
        t = currentCharge = 0;
        hijackControls = false;
        charging = false;
        control.ResetQueuedAction();
        takeStep = takeAimedStep = false;
        specialCol.ColliderOff();
        swordCol.ColliderOff();
        model.rotation = Quaternion.LookRotation(new(model.forward.x, 0, model.forward.z));
    }

    // Baseball Samurai ~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public void StartBaseballSamurai(PlayerStates state)
    {
        control.ResetQueuedAction();
        stamina.TryDrainStamina(attackScript.initialStaminaCost);
        StopAllCoroutines();
        gravity.StopAfterSpecialGrav();
        applyStartForce = true;
        control.state = state;
        if (state == PlayerStates.BASEBALL)
            anim.CrossFade("attack_special_baseballCharge", 0.4f, 0);
        else if (state == PlayerStates.SAMURAI)
            anim.CrossFade("attack_special_samuraiCharge", 0.4f, 0);

        takeStep = takeAimedStep = false;
        t = currentCharge = -attackScript.timeBeforeChargeStarts;
        forceT = forcePerc = perc = 0;
        if (control.GetInput().sqrMagnitude > control.deadzoneSquared) {
            model.rotation = Quaternion.LookRotation(control.GetInput(), Vector3.up);
        }
        rotate.SetRotateSpdMod(0f);
        StartCoroutine(BaseballSamurai(state));
    }

    private float forceT;
    private float forcePerc;
    IEnumerator BaseballSamurai(PlayerStates state)
    {
        // CHARGE
        perc = 0;
        while (buttonHeld && currentCharge < attackScript.maxChargeDuration)
        {
            if (currentCharge >= 0)
            {
                if (stamina.TryDrainStamina(attackScript.chargingStaminaCost * (1 - perc) * Time.unscaledDeltaTime))
                {
                    forceT += Time.unscaledDeltaTime;
                    forcePerc = forceT / attackScript.maxChargeDuration;
                }
            }
            charging = true;
            RotateWhileCharging();
            currentCharge += Time.unscaledDeltaTime;
            perc = Mathf.Max(0, currentCharge / attackScript.maxChargeDuration);
            Time.timeScale = Mathf.Lerp(attackScript.timeScalePercMinMax.y, attackScript.timeScalePercMinMax.x, perc);
            perc = Mathf.Sin(perc * Mathf.PI * 0.5f);
            anim.speed = Mathf.Lerp(1.1f, 4, perc);
            LerpCameraDistance(); /////////////////?????????????? <<<<<<<<<< t�t� ei oo tehty viel ollenkaan, lerppaa kamera v�h�n sis��n ladatessa? jotai efektii
            yield return null;
        }
        // ATTAKC
        perc = currentCharge / attackScript.maxChargeDuration;
        colliders.ChangeToSmallCollider();
        float dmgWithCrit = Mathf.Lerp(attackScript.damageMinMax.x, attackScript.damageMinMax.y, perc);
        dmgWithCrit = attackType == NumberType.crit ? dmgWithCrit * attackScript.critMultiplier : dmgWithCrit;
        attack.target = null;
        if (control.GetInput().sqrMagnitude > control.deadzoneSquared)
        {
            targetDir = attack.AimAndChooseTarget(
                2.1f + Mathf.Clamp(perc * 1.8f, 1, attackScript.maxChargeDuration),
                1.7f + Mathf.Clamp(perc * 1.8f, 1, attackScript.maxChargeDuration));
            model.rotation = Quaternion.LookRotation(new(targetDir.x, 0, targetDir.z));
        }
        currentAttackInstance = Singleton.instance.PlayerAttackSpawner.SpawnAttack(attackScript, model, perc);
            

        specialCol.ColliderOn(currentAttackInstance);
        swordCol.ColliderOn(currentAttackInstance);
        StartCoroutine(TurnHitboxesOffWithDelay(state));
        Singleton.instance.PlayerHurt.Invulnerability(
            Mathf.Lerp(attackScript.timeInvulnerableMinMax.x, attackScript.timeInvulnerableMinMax.y, perc));
        perc = Mathf.Sin(perc * Mathf.PI * 0.5f);
        charging = false;
        anim.speed = 1;
        Time.timeScale = 1;
        IgnoreNearbyEnemies(true);
        rotate.SetRotateSpdMod(0);
        if (state == PlayerStates.BASEBALL)
            anim.Play("attack_special_baseballSwing", 0, 0);
        else if (state == PlayerStates.SAMURAI)
            anim.Play("attack_special_samuraiSwing", 0, 0);

        takeStep = true;
        hijackControls = false;
        attackType = forceT / attackScript.maxChargeDuration > 0.75f ? NumberType.crit : NumberType.normal;
        yield return Helpers.GetWait(Mathf.Lerp(attackScript.hitDurationMinMax.x, attackScript.hitDurationMinMax.y, perc));

        // END
        if (colliders.TryToStandUp() == false)
        {
            EndBaseballSamurai();
            crawl.InitCrawlOnStuckUnder();
            colliders.ChangeToSmallCollider();
            yield break;
        }
        colliders.ChangeToStandUpColliders();

        if (control.IsQueuedAction(QueuedAction.SPECIAL))
        {
            if (stamina.HasStamina(attackScript.initialStaminaCost) == false)
            {
                stamina.FlashStaminaBar();
                EndBaseballSamurai();
                control.state = PlayerStates.NORMAL;
                yield break;
            }
            else
            {
                ContinueBasSam(state);
                yield break;
            }
        }
        EndBaseballSamurai();
        QueueCheckForSpecial();
    }

    public void EndBaseballSamurai()
    {
        if (control.PlayerGrounded == false)
        {
            gravity.AfterBasebSamuraiGravity();
        }
        Time.timeScale = 1;
        IgnoreNearbyEnemies(false);
        charging = false;
        control.InitAccelerationModReturn(0.15f, false);
        rotate.InitRotateSpdModReturn(0.15f);
        model.rotation = Quaternion.LookRotation(new(model.forward.x, 0, model.forward.z));
        hijackControls = false;
        attack.target = null;
        takeStep = takeAimedStep = false;
        if (swordCol.triggerEnabled == true)
        {
            swordCol.ColliderOff();
        }
    }

    void QueueCheckForSpecial()
    {
        StopAllCoroutines();
        gravity.StopJumpCoroutines();
        if (control.IsQueuedAction(QueuedAction.NULL))
        {
            control.state = PlayerStates.NORMAL;
        }
        else
        {
            control.InitQueuedAction();
        }
    }


    public Vector3 BaseballSamuraiMovement(
        Vector3 xAxis, float currX, Vector3 zAxis, float currZ)
    {
        if (takeStep) 
        {
            Step();
        }

        Vector3 currentVelo = control.GetVelocity();
        Vector3 desiredVelo;
        if (charging)
        {
            if (currentVelo.y < 0)
                desiredVelo = new(currentVelo.x * 0.9f, currentVelo.y * 0.7f, currentVelo.z * 0.9f);
            else
                desiredVelo = currentVelo * 0.9f;
        }
        else
        {
            desiredVelo = currentVelo * 0.8f;
        }
        Vector3 newVelo = Vector3.MoveTowards(currentVelo, desiredVelo, 100);
        if (applyStartForce)
        {
            if (rb.velocity.y < 0)
            {
                newVelo += Vector3.up * -rb.velocity.y;
            }
            applyStartForce = false;
        }
        return newVelo;
    }

    void Step()
    {
        Vector3 force = Vector3.zero;
        if (control.GetInput().sqrMagnitude > control.deadzoneSquared)
        {
            model.rotation = Quaternion.LookRotation(control.GetInput());
        }
        
        Vector2 playerInput;
        playerInput.x = Mathf.Abs(control.xAxis) > control.deadzone ? control.xAxis : 0;
        playerInput.y = Mathf.Abs(control.yAxis) > control.deadzone ? control.yAxis : 0;
        playerInput = playerInput.normalized; // Vector2.ClampMagnitude(playerInput, 1f);
        Transform inputSpace = control.GetPlrInputSpace();

        if (inputSpace)
        {
            Vector3 inputSpaceForward = inputSpace.forward;
            if (inputSpace.forward.y < 0)
            {
                if (control.PlayerGrounded)
                {
                    inputSpaceForward.y = 0;
                    inputSpaceForward = inputSpaceForward.normalized;
                }
                else
                {
                    perc = Mathf.Sin(perc * Mathf.PI * 0.5f);
                    inputSpaceForward.y *= perc;
                    inputSpaceForward = inputSpaceForward.normalized;
                }
            }
            if (control.PlayerGrounded == false && control.GetInput().sqrMagnitude < control.deadzoneSquared)
                force += model.forward * Mathf.Lerp(attackScript.stepForceMinMax.x, attackScript.stepForceMinMax.y, forcePerc);
            else
                force += (inputSpaceForward * playerInput.y + inputSpace.right * playerInput.x).normalized *
                        Mathf.Lerp(attackScript.stepForceMinMax.x, attackScript.stepForceMinMax.y, forcePerc);            
        }
        else
        {
            force += (model.forward * playerInput.y + model.right * playerInput.x) *
                Mathf.Lerp(attackScript.stepForceMinMax.x, attackScript.stepForceMinMax.y, forcePerc);
        }
        rb.AddForce(force, ForceMode.Impulse);
        takeStep = takeAimedStep = false;
    }

    public void BaseballSamuraiMovementWithTarget()
    {
        if (takeAimedStep && t < 0.2f)
        {
            AimedStep();
        }
        else
        {
            takeStep = takeAimedStep = false;
            hijackControls = false;
        }
        t += Time.deltaTime;
    }
    
    void AimedStep()
    {
        Vector3 targetPosWithOffset =
            attack.target.position - (attack.target.position - new Vector3(
            transform.position.x, attack.target.position.y, transform.position.z)).normalized * 1.6f;
        Vector3 dirLen = targetPosWithOffset - transform.position;
        control.SetVelocity(dirLen * 1000 * Time.deltaTime);
        model.rotation = Quaternion.LookRotation(new Vector3(
            attack.target.position.x - transform.position.x,
            0,
            attack.target.position.z - transform.position.z).normalized);
    }

    void ContinueBasSam(PlayerStates state)
    {
        if (state == PlayerStates.BASEBALL)
            StartBaseballSamurai(PlayerStates.SAMURAI);
        else if (state == PlayerStates.SAMURAI)
            StartBaseballSamurai(PlayerStates.BASEBALL);
    }

    void IgnoreNearbyEnemies(bool state)
    {
        if (state == true)
        {
            nearEnemyCols = Physics.OverlapSphere(transform.position, 50);
            for (int i = 0; i < nearEnemyCols.Length; i++)
            {
                if (nearEnemyCols[i].CompareTag("Enemy"))
                {
                    Physics.IgnoreCollision(col, nearEnemyCols[i], state);
                }
            }
        }
        else
        {
            for (int i = 0; i < nearEnemyCols.Length; i++)
            {
                if (nearEnemyCols[i] != null)
                {
                    Physics.IgnoreCollision(col, nearEnemyCols[i], state);
                }
            }
            nearEnemyCols = null;
        }
    }

    void LerpCameraDistance()
    {

    }

    void RotateWhileCharging()
    {
        var p = currentCharge / attackScript.maxChargeDuration;
        p *= p;
        rotate.SetRotateSpdMod(p * attackScript.maxChargeRotationMult);
    }

    IEnumerator TurnHitboxesOffWithDelay(PlayerStates state)
    {
        if (state == PlayerStates.BASEBALL)
        {
            yield return Helpers.GetWait(attackScript.hitboxLifetime);
            if (swordCol.triggerEnabled == true)
            {
                specialCol.ColliderOff();
                swordCol.ColliderOff();
            }
        }
        else if (state == PlayerStates.SAMURAI)
        {
            yield return Helpers.GetWait(attackScript.hitboxLifetime);
            if (swordCol.triggerEnabled == true)
            {
                specialCol.ColliderOff();
                swordCol.ColliderOff();
            }
        }
    }


    // Ground pound ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public void StartGroundPound() 
    {
        control.state = PlayerStates.GROUND_POUND;
        control.ResetQueuedAction();
        hijackControls = true;
        groundPoundEnding = false;
        anim.Play("attack_airStomp_start", 0, 0);
        groundPoundLastLocation = Vector3.up * 999999;
        t = 0;
        StartCoroutine(GroundPound());
    }

    void GroundPoundFixed() 
    {
        if (t < groundPoundInitialStopDuration)
        {
            control.SetVelocity(Vector3.zero);
        }
        else if (!control.PlayerGrounded)
        {
            if ((transform.position - groundPoundLastLocation).magnitude < 0.1f &&
                groundPoundEnding == false)
            {
                groundPoundEnding = true;
                StartCoroutine(EndGroundPound());
            }
            groundPoundLastLocation = transform.position;
            control.SetVelocity(Vector3.down * groundPoundSpeed);
        }
    }

    IEnumerator GroundPound() 
    {
        while (t < groundPoundInitialStopDuration) 
        {
            t += Time.deltaTime;
            yield return null;
        }
        while (!control.PlayerGrounded) 
        {
            yield return null;
        }
        if (groundPoundEnding == false)
        {
            groundPoundEnding = true;
            StartCoroutine(EndGroundPound());
        }
    }

    IEnumerator EndGroundPound()
    {
        anim.Play("attack_airStomp_landing_normal", 0, 0);
        control.InitAccelerationModReturn(0.6f, false);
        rotate.InitRotateSpdModReturn(0.6f);
        hijackControls = false;
        yield return Helpers.GetWait(0.2f);
        GroundPoundQueueCheck();
        yield return Helpers.GetWait(0.2f);
        if (control.IsQueuedAction(QueuedAction.SPECIAL) && stamina.HasStamina(attackScript.initialStaminaCost))
        {
            StartBaseballSamurai(PlayerStates.SAMURAI);
        }
        else
        {
            GroundPoundQueueCheck();
        }
    }

    void GroundPoundQueueCheck()
    {
        if (control.IsQueuedAction(QueuedAction.JUMP))
        {
            StopAllCoroutines();
            control.state = PlayerStates.NORMAL;
            jump.InitGroundPoundJump();
        }
        else if (control.IsQueuedAction(QueuedAction.NULL) == false)
        {
            StopAllCoroutines();
            control.InitQueuedAction();
        }
    }
}
