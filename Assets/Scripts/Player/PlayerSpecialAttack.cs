using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpecialAttack : MonoBehaviour {

    [HideInInspector] public bool hijackControls = false;
    [HideInInspector] public bool canSlash = true;

    [SerializeField] private float specialCooldown = 0.5f;
    [Header("Baseball Samurai")]
    [SerializeField] private float baseballSamuraiChargeDuration = 2;
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
    private Transform model;
    private Vector3 groundPoundLastLocation;
    private bool buttonHeld = false;
    private bool takeStep = false;
    private bool takeAimedStep = false;
    private bool groundPoundEnding = false;
    private bool attackQueued, rollQueued, specialQueued, jumpQueued = false;
    private float t, buttonHeldTime, perc;


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
        t = buttonHeldTime = 0;
        hijackControls = false;
        jumpQueued = attackQueued = rollQueued = specialQueued = false;
        takeStep = takeAimedStep = false;
    }

    // Baseball Samurai ~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public void StartBaseballSamurai(PlayerStates state)
    {
        StopAllCoroutines();
        canSlash = false;
        jumpQueued = attackQueued = rollQueued = specialQueued = false;
        control.state = state;
        if (state == PlayerStates.BASEBALL)
            anim.CrossFade("attack_special_baseballCharge", 0.25f, 0);
        else if (state == PlayerStates.SAMURAI)
            anim.CrossFade("attack_special_samuraiCharge", 0.25f, 0);

        takeStep = takeAimedStep = false;
        t = buttonHeldTime = 0;
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
        float desiredX = control.GetRelativeVelo().x * 0.8f; //Mathf.Lerp(0.8f, 1, buttonHeldTime / baseballSamuraiChargeDuration);
        float desiredZ = control.GetRelativeVelo().z * 0.8f; //Mathf.Lerp(0.8f, 1, buttonHeldTime / baseballSamuraiChargeDuration);
        Vector2 currentVelo = new Vector2(currX, currZ);
        Vector2 desiredVelo = new Vector2(desiredX, desiredZ);
        Vector2 newVelo = Vector2.MoveTowards(currentVelo, desiredVelo, 100);
        return xAxis * (newVelo.x - currX) + zAxis * (newVelo.y - currZ);
    }

    void Step()
    {
        if (control.GetInput().sqrMagnitude > control.deadzoneSquared)
        {
            model.rotation = Quaternion.LookRotation(control.GetInput());
        }
        if (rb.velocity.y < 0)
        {
            rb.AddForce(Vector3.up * -rb.velocity.y * rb.mass, ForceMode.Impulse);
        }
        rb.AddForce(control.GetInput().normalized * stepMinimumForce * Mathf.Lerp(1, stepMaximumForceMult, buttonHeldTime / baseballSamuraiChargeDuration), ForceMode.Impulse);
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
        // Charge - 1st hit
        
        while (buttonHeld && buttonHeldTime < baseballSamuraiChargeDuration)
        {
            RotateWhileCharging();
            buttonHeldTime += Time.unscaledDeltaTime;
            if (control.PlayerGrounded == false)
                Time.timeScale = Mathf.Lerp(0.7f, 0f, buttonHeldTime / baseballSamuraiChargeDuration);
            else
                Time.timeScale = Mathf.Lerp(1, 0f, buttonHeldTime / baseballSamuraiChargeDuration);
                
            yield return null;
        }
        Time.timeScale = 1;
        BaseballSamuraiHitboxTiming(state);
        rotate.SetRotateSpdMod(0);
        if (state == PlayerStates.BASEBALL)
            anim.Play("attack_special_baseballSwing", 0, 0);
        else if (state == PlayerStates.SAMURAI)
            anim.Play("attack_special_samuraiSwing", 0, 0);
        
        attack.target = null;
        if (control.GetInput().sqrMagnitude > control.deadzoneSquared)
        {
            model.rotation = Quaternion.LookRotation(attack.AimAndChooseTarget(
                2.1f + Mathf.Clamp(buttonHeldTime * 1.8f, 1, baseballSamuraiChargeDuration),
                1.7f + Mathf.Clamp(buttonHeldTime * 1.8f, 1, baseballSamuraiChargeDuration)));
        }
        if (attack.target != null) {
            takeAimedStep = true;
            hijackControls = true;
        }
        else {
            takeStep = true;
            hijackControls = false;
        }
        SpawnSlashEffect(state);
        if (state == PlayerStates.BASEBALL)
            yield return Helpers.GetWait(firstHitDuration);
        else if (state == PlayerStates.SAMURAI)
            yield return Helpers.GetWait(secondHitDuration);


        if (specialQueued && canSlash)
        {
            print("special queued starting");
            if (state == PlayerStates.BASEBALL)
                StartBaseballSamurai(PlayerStates.SAMURAI);
            else if (state == PlayerStates.SAMURAI)
                StartBaseballSamurai(PlayerStates.BASEBALL);
        }
        else
        {
            print("end special");
            EndBaseballSamurai();
        }
    }

    void RotateWhileCharging()
    {
        var p = buttonHeldTime / baseballSamuraiChargeDuration;
        p *= p;
        rotate.SetRotateSpdMod(p * maxChargeRotationMult);
    }

    void SpawnSlashEffect(PlayerStates state)
    {
        if (state == PlayerStates.BASEBALL)
        {
            float b = buttonHeldTime / 2.5f;
            currentAttackInstance = Singleton.instance.PlayerAttackSpawner.SpawnAttack(
                new Vector3(0, 0.75f, 1.2f),
                Quaternion.LookRotation(model.forward, Vector3.up) * Quaternion.Euler(new Vector3(0, 0, 95)),
                Mathf.Lerp(1.3f, 2, b),
                Mathf.Lerp(1.3f, 2, b),
                Mathf.Lerp(4.5f, 25, b),
                Mathf.Lerp(2, 40, b),
                Mathf.Lerp(0.1f, 0.2f, b),
                0.15f,
                buttonHeldTime);
        }
        else if (state == PlayerStates.SAMURAI)
        {
            float b = buttonHeldTime / 2.5f;
            currentAttackInstance = Singleton.instance.PlayerAttackSpawner.SpawnAttack(
                new Vector3(0, 0.75f, 1.2f),
                Quaternion.LookRotation(model.forward, Vector3.up) * Quaternion.Euler(new Vector3(0, 0, 95)),
                Mathf.Lerp(1.3f, 2, b),
                Mathf.Lerp(1.3f, 2, b),
                Mathf.Lerp(4.5f, 25, b),
                Mathf.Lerp(2, 40, b),
                Mathf.Lerp(0.1f, 0.2f, b),
                0.15f,
                buttonHeldTime);
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
        Time.timeScale = 1;
        control.InitAccelerationModReturn(0.15f, false);
        rotate.InitRotateSpdModReturn(0.15f);
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
