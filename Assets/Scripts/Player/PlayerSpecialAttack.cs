using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpecialAttack : MonoBehaviour {

    [HideInInspector] public bool hijackControls = false;
    [HideInInspector] public bool canSlash = true;

    [SerializeField] private float specialCooldown = 0.5f;
    [Header("Baseball Samurai")]
    [SerializeField] private float maxChargeDuration = 2;
    [Space]
    [SerializeField] private float stepMinimumForce = 150f;
    [SerializeField] private float stepMaximumForceMult = 5f;
    [SerializeField] private float maxChargeRotationMult = 0.5f;
    [Space]
    [SerializeField] private float firstHitboxActivateTime = 0.1f;
    [SerializeField] private float firstHitHitboxLifetime = 0.2f;
    [SerializeField] private float firstHitDuration = 0.458f;
    [SerializeField] private float secondHitboxActivateTime = 0.3f;
    [SerializeField] private float secondHitHitboxLifetime = 0.4f;
    [SerializeField] private float secondHitDuration = 0.4f;
    [Header("Roll Slash")]
    [SerializeField] private float rollSlashDuration = 1;
    [SerializeField] private float rollSlashMoveSpeed = 10;
    [Header("Ground Pound")]
    [SerializeField] private float groundPoundInitialStopDuration = 0.4f;
    
    private PlayerController control;
    private PlayerGravity gravity;
    private PlayerAttacks attack;
    private PlayerJumping jump;
    private SwordTrigger swordCol;
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
    private bool buttonHeld = false;
    private bool applyStartForce = true;
    private bool takeStep = false;
    private bool takeAimedStep = false;
    private bool groundPoundEnding = false;
    private bool attackQueued, rollQueued, specialQueued, jumpQueued = false;
    private float t, currentCharge, perc;

    bool startedGrounded;
    Vector3 targetDir;
    Collider[] nearEnemyCols;


    void Start() 
    {
        control = GetComponent<PlayerController>();
        gravity = GetComponent<PlayerGravity>();
        attack = GetComponent<PlayerAttacks>();
        jump = GetComponent<PlayerJumping>();
        swordCol = GetComponentInChildren<SwordTrigger>();
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
            specialQueued = true;
            attackQueued = rollQueued = jumpQueued = false;
            return;
        }

        ChooseAndStartSpecial();
    }
    void OnRoll()
    {
        if (control.state == PlayerStates.GROUND_POUND || control.state == PlayerStates.ROLL_SLASH || control.state == PlayerStates.BASEBALL || control.state == PlayerStates.SAMURAI)
        {
            rollQueued = true;
            attackQueued = specialQueued = jumpQueued = false;
        }
    }
    void OnAttack()
    {
        if (control.state == PlayerStates.GROUND_POUND || control.state == PlayerStates.ROLL_SLASH || control.state == PlayerStates.BASEBALL || control.state == PlayerStates.SAMURAI)
        {
            attackQueued = true;
            specialQueued = rollQueued = jumpQueued = false;
        }
    }
    void OnJump(InputValue value)
    {
        var pressed = value.isPressed;
        if (!pressed)
            return;
            
        if (control.state == PlayerStates.GROUND_POUND)
        {
            jumpQueued = true;
            attackQueued = specialQueued = rollQueued = false;
        }
    }

    void ChooseAndStartSpecial()
    {
        jumpQueued = attackQueued = rollQueued = specialQueued = false;

        if (canSlash == false)
        {
            control.state = PlayerStates.NORMAL;
            return;
        }
        if (control.state == PlayerStates.NORMAL &&
            (control.PlayerGrounded || control.stepsSinceLastGrounded < 3))
        {
            StartBaseballSamurai(PlayerStates.BASEBALL);
        }
        else if (control.state == PlayerStates.ROLL || (control.state == PlayerStates.NORMAL && control.stepsSinceLastGrounded >= 3))
        {
            StartBaseballSamurai(PlayerStates.SAMURAI); // StartRollSlash();
        }
        /*else if (control.state == PlayerStates.NORMAL &&
            !control.PlayerGrounded &&
            control.stepsSinceLastGrounded > 3)
        {
            StartGroundPound();
        }*/
    }

    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    public void SpecialFixedUpdate() 
    {
        /*
        if (control.state == PlayerStates.ROLL_SLASH)
            RollSlashFixed();
        */
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
        jumpQueued = attackQueued = rollQueued = specialQueued = false;
        takeStep = takeAimedStep = false;
        model.rotation = Quaternion.LookRotation(new(model.forward.x, 0, model.forward.z));
    }

    // Baseball Samurai ~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    bool charging = false;
    public void StartBaseballSamurai(PlayerStates state)
    {
        StopAllCoroutines();
        gravity.StopAfterSpecialGrav();
        canSlash = false;
        applyStartForce = true;
        jumpQueued = attackQueued = rollQueued = specialQueued = false;
        control.state = state;
        if (state == PlayerStates.BASEBALL)
            anim.CrossFade("attack_special_baseballCharge", 0.5f, 0);
        else if (state == PlayerStates.SAMURAI)
            anim.CrossFade("attack_special_samuraiCharge", 0.5f, 0);

        takeStep = takeAimedStep = false;
        t = currentCharge = 0;
        if (control.GetInput().sqrMagnitude > control.deadzoneSquared) {
            model.rotation = Quaternion.LookRotation(control.GetInput(), Vector3.up);
        }
        rotate.SetRotateSpdMod(0f);
        StartCoroutine(BaseballSamurai(state));
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
            /*
            if (currentVelo.y < 0)
                desiredVelo = new(currentVelo.x * 0.8f, currentVelo.y * 0.99f, currentVelo.z * 0.8f);
            else
                desiredVelo = currentVelo * 0.8f;
            */
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
        float heldPerc = currentCharge / maxChargeDuration;

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
                    heldPerc = Mathf.Sin(heldPerc * Mathf.PI * 0.5f);
                    inputSpaceForward.y *= heldPerc;
                    inputSpaceForward = inputSpaceForward.normalized;
                }
            }
            if (control.PlayerGrounded == false && control.GetInput().sqrMagnitude < control.deadzoneSquared)
                force += model.forward *
                    (1 - (currentCharge / maxChargeDuration)) *
                    stepMinimumForce * Mathf.Lerp(1, stepMaximumForceMult, heldPerc);
            else
                force += (inputSpaceForward * playerInput.y + inputSpace.right * playerInput.x) *
                        stepMinimumForce * Mathf.Lerp(1, stepMaximumForceMult, heldPerc);            
        }
        else
        {
            force += (model.forward * playerInput.y + model.right * playerInput.x) * 
                stepMinimumForce * Mathf.Lerp(1, stepMaximumForceMult, heldPerc);
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

    IEnumerator BaseballSamurai(PlayerStates state)
    {
        // CHARGE
        if (control.PlayerGrounded)
            startedGrounded = true;
        else
            startedGrounded = false;

        while (buttonHeld && currentCharge < maxChargeDuration)
        {
            charging = true;
            RotateWhileCharging();
            currentCharge += Time.unscaledDeltaTime;
            perc = currentCharge / maxChargeDuration;
            perc = Mathf.Sin(perc * Mathf.PI * 0.5f);
            if (startedGrounded)
            {
                Time.timeScale = Mathf.Lerp(0.9f, 0.1f, perc);
                anim.speed = Mathf.Lerp(1.1f, 4, perc);
            }
            else
            {
                Time.timeScale = Mathf.Lerp(0.65f, 0.1f, perc);
                anim.speed = Mathf.Lerp(1.6f, 4, perc);
            }
            LerpCameraDistance();
            yield return null;
        }
        // ATTACK
        charging = false;
        anim.speed = 1;
        Time.timeScale = 1;
        IgnoreNearbyEnemies(true);
        BaseballSamuraiHitboxTiming(state);
        rotate.SetRotateSpdMod(0);
        if (state == PlayerStates.BASEBALL)
            anim.Play("attack_special_baseballSwing", 0, 0);
        else if (state == PlayerStates.SAMURAI)
            anim.Play("attack_special_samuraiSwing", 0, 0);
        
        attack.target = null;
        if (control.GetInput().sqrMagnitude > control.deadzoneSquared)
        {
            targetDir = attack.AimAndChooseTarget(
                2.1f + Mathf.Clamp(currentCharge * 1.8f, 1, maxChargeDuration),
                1.7f + Mathf.Clamp(currentCharge * 1.8f, 1, maxChargeDuration));
            model.rotation = Quaternion.LookRotation(new(targetDir.x, 0, targetDir.z));
        }
        takeStep = true;
        hijackControls = false;
        SpawnSlashEffect(state);
        if (state == PlayerStates.BASEBALL)
            yield return Helpers.GetWait(firstHitDuration);
        else if (state == PlayerStates.SAMURAI)
            yield return Helpers.GetWait(secondHitDuration);

        // END
        if (specialQueued && canSlash)
        {
            if (state == PlayerStates.BASEBALL)
                StartBaseballSamurai(PlayerStates.SAMURAI);
            else if (state == PlayerStates.SAMURAI)
                StartBaseballSamurai(PlayerStates.BASEBALL);
        }
        else
        {
            EndBaseballSamurai();
        }
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
        var p = currentCharge / maxChargeDuration;
        p *= p;
        rotate.SetRotateSpdMod(p * maxChargeRotationMult);
    }

    void SpawnSlashEffect(PlayerStates state)
    {
        if (state == PlayerStates.BASEBALL)
        {
            float b = currentCharge / 2.5f;
            currentAttackInstance = Singleton.instance.PlayerAttackSpawner.SpawnAttack(
                new Vector3(0, 0.75f, 1.2f),
                Quaternion.LookRotation(model.forward, Vector3.up) * Quaternion.Euler(new Vector3(0, 0, 95)),
                Mathf.Lerp(1.3f, 2, b),
                Mathf.Lerp(1.3f, 2, b),
                Mathf.Lerp(4.5f, 25, b),
                Mathf.Lerp(2, 40, b),
                Mathf.Lerp(0.1f, 0.2f, b),
                0.03f,
                currentCharge);
        }
        else if (state == PlayerStates.SAMURAI)
        {
            float b = currentCharge / 2.5f;
            currentAttackInstance = Singleton.instance.PlayerAttackSpawner.SpawnAttack(
                new Vector3(0, 0.75f, 1.2f),
                Quaternion.LookRotation(model.forward, Vector3.up) * Quaternion.Euler(new Vector3(0, 0, 95)),
                Mathf.Lerp(1.3f, 2, b),
                Mathf.Lerp(1.3f, 2, b),
                Mathf.Lerp(4.5f, 25, b),
                Mathf.Lerp(2, 40, b),
                Mathf.Lerp(0.1f, 0.2f, b),
                0.03f,
                currentCharge);
        }
    }

    IEnumerator BaseballSamuraiHitboxTiming(PlayerStates state)
    {
        if (state == PlayerStates.BASEBALL)
        {
            yield return Helpers.GetWait(firstHitboxActivateTime);
            if (swordCol.triggerEnabled == false)
            {
                swordCol.ColliderOn(currentAttackInstance);
            }
            yield return Helpers.GetWait(firstHitHitboxLifetime);
            if (swordCol.triggerEnabled == true)
            {
                swordCol.ColliderOff();
            }
        }

        else if (state == PlayerStates.SAMURAI)
        {
            yield return Helpers.GetWait(secondHitboxActivateTime);
            if (swordCol.triggerEnabled == false)
            {
                swordCol.ColliderOn(currentAttackInstance);
            }
            yield return Helpers.GetWait(secondHitHitboxLifetime);
            if (swordCol.triggerEnabled == true)
            {
                swordCol.ColliderOff();
            }
        }
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
        EndAndCheckIfQueuedAction();
    }

    // Roll slash ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    /*
    void StartRollSlash() 
    {
        StopAllCoroutines();
        control.state = PlayerStates.ROLL_SLASH;
        roll.StopRoll();
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        control.SetVelocity(new Vector3(rb.velocity.x, 0, rb.velocity.z));
        control.SetAccelerationMod(1);
        control.moveSpeedMod = 1;
        colliders.ChangeToSmallCollider();
        if (control.GetInput().sqrMagnitude > control.deadzoneSquared)
        {
            model.rotation = Quaternion.LookRotation(control.GetInput());
        }
        anim.Play("attack_roll", 0, 0);
        
        hijackControls = true;
        canSlash = false;
        t = 0;
        StartCoroutine(RollSlash());
    }

    void RollSlashFixed() 
    {
        if (t < rollSlashDuration)
        {
            perc = 1 - t / rollSlashDuration;
            control.SetVelocity(model.forward * rollSlashMoveSpeed * perc);
        }
    }

    IEnumerator RollSlash() 
    {
        while (t < rollSlashDuration) 
        {
            control.SetAccelerationMod(0f);
            rotate.SetRotateSpdMod(0f);
            t += Time.deltaTime;
            yield return null;
        }
        EndRollSlash();
    }

    void EndRollSlash()
    {
        hijackControls = false;
        if (colliders.TryToStandUp() == false)
        {
            rotate.InitRotateSpdModReturn(0.1f);
            control.InitAccelerationModReturn(0.1f, false);
            crawl.InitCrawlOnStuckUnder();
            StopAllCoroutines();
        }
        else if (buttonHeld)
        {
            colliders.ChangeToStandUpColliders();
            anim.Play("attack_special_2ndPart", 0, 0);
            t = buttonHeldTime = 0.25f;
            control.state = PlayerStates.BASEBALL_SAMURAI;
            takeStep = takeAimedStep = false;
            attack.target = null;
            StartCoroutine(BaseballSamurai());
        }
        else
        {
            colliders.ChangeToStandUpColliders();
            rotate.InitRotateSpdModReturn(0.1f);
            control.InitAccelerationModReturn(0.1f, false);
            EndAndCheckIfQueuedAction();
        }
    }
    */
    //attack_special_2ndPart

    // Ground pound ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public void StartGroundPound() 
    {
        control.state = PlayerStates.GROUND_POUND;
        jumpQueued = attackQueued = rollQueued = specialQueued = false;
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
            control.SetVelocity(Vector3.down * 30f);
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
        CheckForQueuedActions();
        yield return Helpers.GetWait(0.2f);
        EndAndCheckIfQueuedAction();
    }

    // Ending queuecheck
    void CheckForQueuedActions()
    {
        if (jumpQueued)
        {
            control.state = PlayerStates.NORMAL;
            StopAllCoroutines();
            jump.InitGroundPoundJump();
        }
        if (attackQueued)
        {
            control.state = PlayerStates.NORMAL;
            StopAllCoroutines();
            attack.InitAttack();
        }
        else if (specialQueued)
        {
            control.state = PlayerStates.NORMAL;
            StopAllCoroutines();
            ChooseAndStartSpecial();
        }
        else if (rollQueued)
        {
            control.state = PlayerStates.NORMAL;
            StopAllCoroutines();
            roll.InitRoll();
        }
    }

    void EndAndCheckIfQueuedAction()
    {
        StopAllCoroutines();
        if (attackQueued)
        {
            attack.InitAttack();
        }
        else if (specialQueued)
        {
            ChooseAndStartSpecial();
        }
        else if (rollQueued)
        {
            roll.InitRoll();
        }
        else if (jumpQueued)
        {
            jump.InitGroundPoundJump();
        }
        else
        {
            control.state = PlayerStates.NORMAL;
        }
    }
}
