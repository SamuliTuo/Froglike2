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
    [SerializeField] private float firstStepActivateTime = 0.2f;
    [SerializeField] private float firstStepMoveForce = 80f;
    [SerializeField] private float secondStepMoveForce = 150f;
    [Space]
    [SerializeField] private float firstHitActivateTime = 0.1f;
    [SerializeField] private float firstHitHitboxLifetime = 0.2f;
    [SerializeField] private float secondHitActivateTime = 0.3f;
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
    private PlayerRotate rotate;
    private Animator anim;
    private Rigidbody rb;
    private Transform model;
    private Vector3 groundPoundLastLocation;
    private bool buttonHeld = false;
    private bool takeFirstStep = false;
    private bool takeSecondStep = false;
    private bool groundPoundEnding = false;
    private bool attackQueued, rollQueued, specialQueued, jumpQueued = false;
    private float t, buttonHeldTime, perc, stepTwoStartTime;


    void Start() 
    {
        control = GetComponent<PlayerController>();
        gravity = GetComponent<PlayerGravity>();
        attack = GetComponent<PlayerAttacks>();
        jump = GetComponent<PlayerJumping>();
        swordCol = GetComponentInChildren<SwordTrigger>();
        roll = GetComponent<PlayerRolling>();
        rotate = GetComponentInChildren<PlayerRotate>();
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
            control.state == PlayerStates.ROLL_SLASH || 
            control.state == PlayerStates.BASEBALL_SAMURAI)
        {
            specialQueued = true;
            attackQueued = rollQueued = jumpQueued = false;
            return;
        }

        ChooseAndStartSpecial();
    }
    void OnRoll()
    {
        if (control.state == PlayerStates.GROUND_POUND || control.state == PlayerStates.ROLL_SLASH || control.state == PlayerStates.BASEBALL_SAMURAI)
        {
            rollQueued = true;
            attackQueued = specialQueued = jumpQueued = false;
        }
    }
    void OnAttack()
    {
        if (control.state == PlayerStates.GROUND_POUND || control.state == PlayerStates.ROLL_SLASH || control.state == PlayerStates.BASEBALL_SAMURAI)
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

        if (control.state == PlayerStates.NORMAL &&
            (control.PlayerGrounded || control.stepsSinceLastGrounded < 3))
        {
            StartBaseballSamurai();
        }
        else if (canSlash && (control.state == PlayerStates.ROLL ||
            (control.state == PlayerStates.NORMAL && control.stepsSinceLastGrounded >= 3)))
        {
            StartRollSlash();
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
        if (control.state == PlayerStates.ROLL_SLASH)
            RollSlashFixed();
        else if (control.state == PlayerStates.GROUND_POUND)
            GroundPoundFixed();
        else if (control.state == PlayerStates.BASEBALL_SAMURAI && hijackControls)
            BaseballSamuraiMovementWithTarget();
    }

    public void Reset()
    {
        StopAllCoroutines();
        t = buttonHeldTime = 0;
        hijackControls = false;
    }

    // Baseball Samurai ~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public void StartBaseballSamurai()
    {
        control.state = PlayerStates.BASEBALL_SAMURAI;
        anim.Play("attack_special_1stPart", 0, 0);
        takeFirstStep = takeSecondStep = false;
        t = buttonHeldTime = 0;
        attack.target = null;
        if (control.GetInput().sqrMagnitude > control.deadzoneSquared)
        {
            model.rotation = Quaternion.LookRotation(attack.AimAndChooseTarget(3.7f, 3.3f));
        }
        if (attack.target != null) {
            hijackControls = true;
        }
        rotate.SetRotateSpdMod(0f);
        StartCoroutine(BaseballSamurai());
        if (control.GetInput().sqrMagnitude > control.deadzoneSquared)
        {
            StartCoroutine(Samurai1stStepTiming());
        }
        StartCoroutine(BaseballSamurai1stHitHitboxes());
    }

    public Vector3 BaseballSamuraiMovement(
        Vector3 xAxis, float currX, Vector3 zAxis, float currZ)
    {
        if (takeFirstStep) 
        {
            rb.AddForce(model.forward * firstStepMoveForce, ForceMode.Impulse);
            takeFirstStep = false;
        }
        if (takeSecondStep)
        {
            if (control.GetInput().sqrMagnitude > control.deadzoneSquared)
            {
                model.rotation = Quaternion.LookRotation(control.GetInput());
            }
            rb.AddForce(control.GetInput().normalized * secondStepMoveForce * buttonHeldTime, ForceMode.Impulse);
            takeSecondStep = false;
            t = baseballSamuraiChargeDuration * 0.15f;
        }

        float perc = 0;
        if (t < baseballSamuraiChargeDuration)
        {
            perc = 1 - t / baseballSamuraiChargeDuration;
        }
        t += Time.deltaTime;

        float desiredX = control.GetRelativeVelo().x * perc;
        float desiredZ = control.GetRelativeVelo().z * perc;
        Vector2 currentVelo = new Vector2(currX, currZ);
        Vector2 desiredVelo = new Vector2(desiredX, desiredZ);
        Vector2 newVelo = Vector2.MoveTowards(currentVelo, desiredVelo, 100);
        return xAxis * (newVelo.x - currX) + zAxis * (newVelo.y - currZ);
    }

    public void BaseballSamuraiMovementWithTarget()
    {
        if (t < 0.2f)
        {
            AimedStep();
        }
        else if (takeSecondStep == false)
        {
            takeFirstStep = false;
            rb.velocity = Vector3.zero;
            control.SetVelocity(Vector3.zero);
            hijackControls = false;
        }
        else if (t < stepTwoStartTime + 0.2f)
        {
            AimedStep();
        }
        else
        {
            takeSecondStep = false;
            rb.velocity = Vector3.zero;
            control.SetVelocity(Vector3.zero);
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

    IEnumerator Samurai1stStepTiming()
    {
        yield return Helpers.GetWait(firstStepActivateTime);
        takeFirstStep = true;
    }

    // testing:
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (control != null)
        {
            if (control.GetInput().sqrMagnitude > control.deadzoneSquared)
            {
                Gizmos.DrawWireSphere(transform.position + control.GetInput() * aimerDisttt, aimerRadiousss);
            }
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position + transform.GetChild(0).forward * aimerDisttt, aimerRadiousss);
        }
        
    }
    float aimerDisttt;
    float aimerRadiousss;
    // testing

    IEnumerator BaseballSamurai()
    {
        while (buttonHeld && buttonHeldTime < baseballSamuraiChargeDuration)
        {
            buttonHeldTime += Time.deltaTime;

            // testing:
            aimerRadiousss = 2.7f + Mathf.Clamp(buttonHeldTime * 1.8f, 1, baseballSamuraiChargeDuration * 1.3f);
            aimerDisttt = 2.3f + Mathf.Clamp(buttonHeldTime * 1.8f, 1, baseballSamuraiChargeDuration * 1.3f);
            // testing

            yield return null;
        }
        if (buttonHeldTime > 0.5f)
        {
            StartCoroutine(BaseballSamurai2ndHitHitboxes());
            anim.Play("attack_special_3rdPart", 0, 0);
            attack.target = null;
            if (control.GetInput().sqrMagnitude > control.deadzoneSquared)
            {
                model.rotation = Quaternion.LookRotation(attack.AimAndChooseTarget(
                    2.7f + Mathf.Clamp(buttonHeldTime * 1.8f, 1, baseballSamuraiChargeDuration * 1.3f),
                    2.3f + Mathf.Clamp(buttonHeldTime * 1.8f, 1, baseballSamuraiChargeDuration * 1.3f)));
            }
            if (attack.target != null)
            {
                stepTwoStartTime = t;
                hijackControls = true;
            }
            else
            {
                hijackControls = false;
            }
            takeSecondStep = true;
            yield return Helpers.GetWait(secondHitDuration);
            EndBaseballSamurai();
        }
        else
        {
            while (!anim.GetCurrentAnimatorStateInfo(0).IsName("attack_special_2ndPart"))
            {
                yield return null;
            }
            EndBaseballSamurai();
        }
    }
    IEnumerator BaseballSamurai1stHitHitboxes()
    {
        yield return Helpers.GetWait(firstHitActivateTime);
        if (swordCol.triggerEnabled == false)
        {
            swordCol.ToggleState(true);
        }
        yield return Helpers.GetWait(firstHitHitboxLifetime);
        if (swordCol.triggerEnabled == true)
        {
            swordCol.ToggleState(false);
        }
    }
    IEnumerator BaseballSamurai2ndHitHitboxes()
    {
        yield return Helpers.GetWait(secondHitActivateTime);
        if (swordCol.triggerEnabled == false)
        {
            swordCol.ToggleState(true);
        }
        yield return Helpers.GetWait(secondHitHitboxLifetime);
        if (swordCol.triggerEnabled == true)
        {
            swordCol.ToggleState(false);
        }
    }
    public void EndBaseballSamurai()
    {
        control.InitAccelerationModReturn(0.15f, false);
        rotate.InitRotateSpdModReturn(0.15f);
        hijackControls = false;
        attack.target = null;
        if (swordCol.triggerEnabled == true)
        {
            swordCol.ToggleState(false);
        }
        EndAndCheckIfQueuedAction();
    }

    // Roll slash ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    void StartRollSlash() 
    {
        control.state = PlayerStates.ROLL_SLASH;
        roll.StopRoll();
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        control.SetVelocity(new Vector3(rb.velocity.x, 0, rb.velocity.z));
        control.SetAccelerationMod(1);
        control.moveSpeedMod = 1;
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
        if (buttonHeld)
        {
            anim.Play("attack_special_2ndPart", 0, 0);
            t = buttonHeldTime = 0.25f;
            control.state = PlayerStates.BASEBALL_SAMURAI;
            takeFirstStep = takeSecondStep = false;
            hijackControls = false;
            attack.target = null;
            StartCoroutine(BaseballSamurai());
        }
        else
        {
            rotate.InitRotateSpdModReturn(0.1f);
            control.InitAccelerationModReturn(0.1f, false);
            hijackControls = false;
            EndAndCheckIfQueuedAction();
        }
    }

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
        control.state = PlayerStates.NORMAL;
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
    }
}
