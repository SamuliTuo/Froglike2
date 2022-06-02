using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJumping : MonoBehaviour {

    public int maxLateJumpTimeSteps = 12;

    [HideInInspector] public bool playerJumpPressed = false;
    [HideInInspector] public bool playerCanJump = true;
    [HideInInspector] public bool airJumpUsed = false;
    [HideInInspector] public int lateJumpTimer = 0;

    [SerializeField] private int maxEarlyJumpTimeSteps = 12;
    [Space]
    [SerializeField] private float normalJump_moveSpdReduct = 1f;
    [SerializeField] private float jumpHeight = 4f;
    [SerializeField] private float runHeightMultiplier = 1.2f;
    [SerializeField] private float groundPoundHeightMultiplier = 1.3f;
    [Header("Extra gravity after jump")]
    [SerializeField] private float brakeDuration = 0.5f;
    [SerializeField] private float maxBrakeForce = 10;
    [Header("Walljump")]
    [SerializeField] private float wallJumpHorizontalMult = 0.9f;
    [SerializeField] private float wallJumpMinimumAngle = 66f;
    [SerializeField] private float wallJumpVerticalMult = 0.7f; 
    [SerializeField] private float upwardsWallJumpHeight = 11f;
    [SerializeField] private float accelModTimeOnWalljump = 1.3f;
    [SerializeField] private float wallJumpRotationAlignDuration = 1f;
    [Header("Airjump")]
    [SerializeField] private float airJumpHorizForce = 70;
    [SerializeField] private float airJumpVertiForce = 40f;
    [SerializeField] private float upwardsAirJumpHeight = 30;
    [SerializeField] private float airJumpRotationAlignDuration = 1f;
    [SerializeField] private float accelModTimeOnAirJump = 0.3f;
    [SerializeField] private float accelModTimeOnUpwardsAirJump = 0.3f;

    private PlayerController control;
    private PlayerGravity gravity;
    private PlayerLongJump longJump;
    private PlayerRunning run;
    private PlayerJumpingUpgrades upgrades;
    private PlayerRotate rotate;
    private Animator anim;
    private Rigidbody rb;
    private Transform graphics;
    private Coroutine brakeCoroutine = null;
    private bool groundPoundJumpQueued = false;
    private bool normalJumpQueued = false;
    private bool upwardsAirJump = false;
    private int earlyJumpPressTimer = 0;
    private float jumpSpeed;
    private float brakeT;
    private float perc;


    void Start() {
        control = GetComponent<PlayerController>();
        gravity = GetComponent<PlayerGravity>();
        longJump = GetComponent<PlayerLongJump>();
        run = GetComponent<PlayerRunning>();
        upgrades = GetComponentInChildren<PlayerJumpingUpgrades>();
        rotate = GetComponentInChildren<PlayerRotate>();
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        graphics = transform.GetChild(0);
    }

    // Gets the state of jump-button:
    void OnJump(InputValue value) {
        playerJumpPressed |= value.isPressed;
        earlyJumpPressTimer = maxEarlyJumpTimeSteps;
    }

    public void JumpFixedUpdate() {
        if (groundPoundJumpQueued)
            GroundPoundJump();
        else if (normalJumpQueued)
            NormalJump();

        if (playerJumpPressed) {
            if (earlyJumpPressTimer > 0) {
                earlyJumpPressTimer--;
            }
            else {
                playerJumpPressed = false;
            }
            ChooseJump();
        }
    }

    void ChooseJump() {
        if (control.state == PlayerStates.ATTACK ||
            control.state == PlayerStates.GROUND_POUND ||
            control.state == PlayerStates.ROLL_SLASH ||
            control.state == PlayerStates.BASEBALL_SAMURAI ||
            control.state == PlayerStates.LONG_JUMP) {
            return;
        }

        jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
        if (control.state == PlayerStates.ROLL) {
            if (control.PlayerGrounded || lateJumpTimer > 0) {
                longJump.LongJump();
            }
        }
        else if (control.PlayerGrounded || 
            (earlyJumpPressTimer > 0 && control.PlayerGrounded) ||
            lateJumpTimer > 0 && playerCanJump)
        {
            NormalJump();
        }
        else if (control.PlayerOnSteep) {
            WallJump();
        }
        else if (!airJumpUsed) {
            InitAirJump();
        }
    }

    public void InitGroundPoundJump() {
        groundPoundJumpQueued = true;
    }
    public void InitNormalJump() {
        normalJumpQueued = true;
    }

    void GroundPoundJump() {
        upgrades.JumpUpgrades();
        control.SetVelocity(Vector3.up * jumpSpeed * groundPoundHeightMultiplier);
        anim.Play("jump", 0, 0);
        groundPoundJumpQueued = false;
        StartJumpBrakes();
        StartCoroutine(JustJumped());
    }

    void NormalJump() {
        normalJumpQueued = false;
        Vector3 jumpDir = Vector3.up;
        var velo = control.GetVelocity();
        float alignedSpeedDot = Vector3.Dot(new Vector3(velo.x, 0, velo.z), jumpDir);
        upgrades.JumpUpgrades();
        if (alignedSpeedDot > 0f) {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeedDot, 0f);
        }
        control.SetVelocity(
            new Vector3(velo.x * normalJump_moveSpdReduct, 0, velo.z * normalJump_moveSpdReduct) +
            (jumpDir * jumpSpeed * Mathf.Max(1, run.runMultiplier * runHeightMultiplier)));
        //particles
        //audio
        anim.Play("jump", 0, 0);
        StartJumpBrakes();
        StartCoroutine(JustJumped());
    }

    void WallJump() {
        anim.Play("jump", 0, 0);
        //_anim.StraightToJumpAnimation();
        Vector3 wallNormal = new Vector3(control.steepNormal.x, 0, control.steepNormal.z).normalized;
        //Vector3 newDir = wallNormal;
        //Vector3 inputDir = control.GetInput();
        upgrades.WalljumpUpgrades(control.steepNormal, control.contactPoint);
        Vector3 newDir = Vector3.RotateTowards(wallNormal, Vector3.up, 1.16937f, 0) * upwardsWallJumpHeight;
        ActivateWallJump(newDir);
        /*

        // Rotate jumpDir towards input:
        if (inputDir.sqrMagnitude > 0.2f * 0.2f) {

            // Upwards walljump
            var dot = Vector3.Dot(wallNormal, inputDir.normalized);
            if (dot < -0.97f) {
                newDir = Vector3.RotateTowards(
                    wallNormal, Vector3.up, 1.16937f, 0) * upwardsWallJumpHeight;
                ActivateWallJump(newDir);
                return;
            }
            else {
                Vector3 crossProd = Vector3.Cross(wallNormal, inputDir.normalized);
                float leftOrRightOfNormal = Vector3.Dot(crossProd, Vector3.up);

                // Upwards-diagonal walljump
                if (dot < -0.7f) {
                    newDir = Vector3.RotateTowards(
                        wallNormal, Vector3.up, 1.16937f, 0) * upwardsWallJumpHeight;
                    newDir = Quaternion.Euler(0, 45 * leftOrRightOfNormal, 0) * newDir;
                    ActivateWallJump(newDir);
                    return;
                }
                else {
                    newDir = Quaternion.AngleAxis(
                        wallJumpMinimumAngle * leftOrRightOfNormal, Vector3.up) * wallNormal;
                }
            }
        }
        // Rotate dir from velocity when no input:
        else {
            Vector3 velo = new Vector3(control.GetVelocity().x, 0, control.GetVelocity().z);
            if (velo.sqrMagnitude > 3f * 3f) {
                Vector3 crossProd = Vector3.Cross(wallNormal, velo.normalized);
                float leftOrRightOfNormal = Vector3.Dot(crossProd, Vector3.up);
                newDir = Quaternion.AngleAxis(
                    (wallJumpMinimumAngle * 0.6f) * leftOrRightOfNormal, Vector3.up) * wallNormal;
            }
        }
        var dir = (newDir * wallJumpHorizontalMult + Vector3.up * wallJumpVerticalMult) * jumpSpeed;
        ActivateWallJump(dir);
        */
    }

    public void ActivateWallJump(Vector3 dir) {
        StartJumpBrakes();
        StartCoroutine(JustJumped());
        control.SetVelocity(dir);
        rotate.AlignToVelocityForTime(wallJumpRotationAlignDuration);
        control.InitAccelerationModReturn(accelModTimeOnWalljump, true);
    }

    void InitAirJump() {
        Vector3 force = InputCheckVec3();
        if (upwardsAirJump) {
            anim.Play("airDash_upwards", 0, 0);
            control.InitAccelerationModReturn(accelModTimeOnUpwardsAirJump, true);
        }
        else {
            anim.Play("airDash", 0, 0);
            control.InitAccelerationModReturn(accelModTimeOnAirJump, true);
            graphics.rotation = Quaternion.LookRotation(control.GetInput(), Vector3.up);
        }
        //rotate.AlignToVelocityForTime(airJumpRotationAlignDuration);
        control.SetVelocity(Vector3.zero);
        rb.velocity = Vector3.zero;
        rb.AddForce(force, ForceMode.Impulse);
        upgrades.JumpUpgrades();
        airJumpUsed = true;
        StartCoroutine(JustJumped());
        StartJumpBrakes();
        //_run.ToggleRun(true);
        // Effects:
        //FrogAudioPlayer.current.PlayDashSFX();
        //_leafUI.UpdateStaminaUI(false);
        //ParticleEffects.current.PlayDashVFX(transform.root.position);
        //ParticleEffects.current.PlayJumpVFX(cloudTrailTMult);
        //HitStop.current.StopTime(dashHitstopTime);
    }

    Vector3 InputCheckVec3() {
        upwardsAirJump = false;
        var inputVec = control.GetInput();
        if (inputVec.sqrMagnitude <= control.deadzoneSquared) {
            upwardsAirJump = true;
            return graphics.forward * 0.01f + new Vector3(0, upwardsAirJumpHeight, 0);
        }
        else {
            return inputVec * airJumpHorizForce + Vector3.up * airJumpVertiForce;
        }
    }

    public IEnumerator JustJumped() {
        playerJumpPressed = false;
        //longJumpPressed = false;
        //canLongJumpWallJump = false;
        lateJumpTimer = 0;
        earlyJumpPressTimer = 0;
        playerCanJump = false;
        yield return Helpers.GetWait(0.2f);
        lateJumpTimer = 0;
        playerCanJump = true;
        //canLongJumpWallJump = true;
        //longJumpAttacked = false;
    }

    void StartJumpBrakes() {
        if (brakeCoroutine != null) {
            StopCoroutine(brakeCoroutine);
        }
        brakeCoroutine = StartCoroutine(JumpBrakes());

        if (gravity.apexGravCoroutine != null) {
            StopCoroutine(gravity.apexGravCoroutine);
        }
        gravity.apexGravCoroutine = StartCoroutine(gravity.ApexGravMultiplier());
    }
    IEnumerator JumpBrakes()
    {
        brakeT = 0;
        while (brakeT < brakeDuration)
        {
            perc = brakeT / brakeDuration;
            perc = Mathf.Sin(perc * Mathf.PI * 0.5f);
            gravity.SetAfterJumpExtraGravMultiplier(Mathf.Lerp(0, maxBrakeForce, perc));
            brakeT += Time.deltaTime;
            yield return null;
        }
        gravity.SetAfterJumpExtraGravMultiplier(0f);
    }
}
