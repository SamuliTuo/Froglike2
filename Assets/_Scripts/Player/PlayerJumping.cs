using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public enum JumpType { NULL, NORMAL, AIR, WALL, ROLL, ROLL_WALL }

public class PlayerJumping : MonoBehaviour {

    public int maxLateJumpTimeSteps = 12;

    [HideInInspector] public bool playerJumpPressed = false;
    [HideInInspector] public bool playerCanJump = true;
    [HideInInspector] public bool airJumpUsed = false;
    [HideInInspector] public int lateJumpTimer = 0;
    [HideInInspector] public JumpType currentJump = JumpType.NULL;

    [SerializeField] private int maxEarlyJumpTimeSteps = 12;
    [Space]
    [SerializeField] private float normalJump_moveSpdReduct = 1f;
    [SerializeField] private float rollJump_moveSpdMult = 1.5f;
    [SerializeField] private float jumpHeight = 4f;
    [SerializeField] private float runHeightMultiplier = 1.2f;
    [SerializeField] private float groundPoundHeightMultiplier = 1.3f;
    [Header("Walljump")]
    [SerializeField] private int wallJumpCoyoteFrames = 10;
    [SerializeField] private float wallJumpHorizontalMult = 0.9f;
    [SerializeField] private float wallJumpMinimumAngle = 66f;
    [SerializeField] private float wallJumpVerticalMult = 0.7f; 
    [SerializeField] private float upwardsWallJumpHeight = 11f;
    [SerializeField] private float accelModTimeOnWalljump = 1.3f;
    [SerializeField] private float accelModTimeOnRollingWalljump = 0.6f;
    [SerializeField] private float wallJumpRotationAlignDuration = 1f;
    [Header("RollWallJump")]
    [SerializeField] private float rollWallJumpMinimumAngle = 66f;
    [SerializeField] private float wallRollJump_moveSpdReduct = 0.9f;
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
    private PlayerRolling rolling;
    private PlayerJumpingUpgrades upgrades;
    private PlayerRotate rotate;
    private Animator anim;
    private Rigidbody rb;
    private Transform graphics;
    private bool groundPoundJumpQueued = false;
    private bool normalJumpQueued = false;
    private bool upwardsAirJump = false;
    private int earlyJumpPressTimer = 0;
    private float jumpSpeed;


    void Start() {
        control = GetComponent<PlayerController>();
        gravity = GetComponent<PlayerGravity>();
        longJump = GetComponent<PlayerLongJump>();
        run = GetComponent<PlayerRunning>();
        rolling = GetComponent<PlayerRolling>();
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

            if (control.state == PlayerStates.ATTACK ||
                control.state == PlayerStates.GROUND_POUND ||
                control.state == PlayerStates.ROLL_SLASH ||
                control.state == PlayerStates.BASEBALL ||
                control.state == PlayerStates.SAMURAI ||
                control.state == PlayerStates.LONG_JUMP ||
                control.state == PlayerStates.MOUTH)
            {
                return;
            }
            ChooseJump();
        }
    }
    

    public void ChooseJump() {
        control.StopAfterSpecialGravity();
        jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
        if (control.state == PlayerStates.ROLL) {
            if (playerCanJump && (control.PlayerGrounded || lateJumpTimer > 0)) {
                RollJump();
                //longJump.LongJump();
            }
            else if (playerCanJump && control.PlayerOnSteep || control.playerOnSlippery || control.stepsSinceLastSteep <= wallJumpCoyoteFrames)
            {
                RollWallJump();
            }
        }
        else if (control.PlayerGrounded || 
            (earlyJumpPressTimer > 0 && control.PlayerGrounded) ||
            lateJumpTimer > 0 && playerCanJump)
        {
            NormalJump();
        }
        else if (control.PlayerOnSteep || control.stepsSinceLastSteep <= wallJumpCoyoteFrames)
        {
            print("walljumping");
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
        Singleton.instance.ParticleEffects.SpawnSmoke(transform.position + Vector3.down, Vector3.up);
        upgrades.JumpUpgrades();
        control.SetVelocity(Vector3.up * jumpSpeed * groundPoundHeightMultiplier);
        anim.Play("jump", 0, 0);
        groundPoundJumpQueued = false;
        StartJumpBrakes();
        StartCoroutine(JustJumped());
    }

    void NormalJump() {
        currentJump = JumpType.NORMAL;
        normalJumpQueued = false;
        Singleton.instance.ParticleEffects.SpawnSmoke(transform.position + Vector3.down, Vector3.up);
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
        //audio
        anim.Play("jump", 0, 0);
        StartJumpBrakes();
        StartCoroutine(JustJumped());
    }

    public float rollJumpHeightMultiplier = 0.75f;
    void RollJump() {
        currentJump = JumpType.ROLL;
        jumpSpeed *= rollJumpHeightMultiplier;
        Singleton.instance.ParticleEffects.SpawnSmoke(transform.position + Vector3.down, Vector3.up);
        Vector3 jumpDir = Vector3.up;
        var velo = control.GetVelocity();
        float alignSpeedDot = Vector3.Dot(new Vector3(velo.x, 0, velo.z), jumpDir);
        if (alignSpeedDot > 0f) {
            //jumpSpeed = Mathf.Max(jumpSpeed - alignSpeedDot, 0f);
        }
        control.SetVelocity(
            new Vector3(velo.x * normalJump_moveSpdReduct, 0, velo.z * normalJump_moveSpdReduct) + 
            (jumpDir * jumpSpeed));
        StartJumpBrakes();
        StartCoroutine(JustJumped());
    }

    public void WallJump() {

        control.stepsSinceLastSteep = wallJumpCoyoteFrames;
        jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
        currentJump = JumpType.WALL;
        anim.Play("jump", 0, 0);
        var point = control.contactPoint == null ? transform.position : control.contactPoint;
        Singleton.instance.ParticleEffects.SpawnSmoke(point, control.lastSteepNormal);
        //_anim.StraightToJumpAnimation();
        Vector3 wallNormal = new Vector3(control.lastSteepNormal.x, 0, control.lastSteepNormal.z).normalized;
        Vector3 newDir = wallNormal;// new Vector3(wallNormal.x * wallJumpHorizontalMult, wallNormal.y, wallNormal.z * wallJumpHorizontalMult) + Vector3.up * wallJumpVerticalMult;
        Vector3 inputDir = control.GetInput();
        upgrades.WalljumpUpgrades(control.steepNormal, control.contactPoint);
        //ActivateWallJump(newDir);
        
        

        // Rotate jumpDir towards input:
        if (inputDir.sqrMagnitude > 0.2f * 0.2f) {

            // Upwards walljump
            var dot = Vector3.Dot(wallNormal, inputDir.normalized);
            if (dot < -0.9f) {
                newDir = new Vector3(newDir.x * wallJumpHorizontalMult, newDir.y, newDir.z * wallJumpHorizontalMult) + Vector3.up * wallJumpVerticalMult;
                //newDir = Vector3.RotateTowards(
                    //wallNormal, Vector3.up, 1.16937f, 0);
                ActivateWallJump(newDir);
                return;
            }
            else {
                Vector3 crossProd = Vector3.Cross(wallNormal, inputDir.normalized);
                float leftOrRightOfNormal = Vector3.Dot(crossProd, Vector3.up);

                // Upwards-diagonal walljump
                if (dot < -0.7f) {
                    newDir = Vector3.RotateTowards(
                        wallNormal, Vector3.up, 1.16937f, 0);
                    newDir = Quaternion.Euler(0, 45 * leftOrRightOfNormal, 0) * newDir;
                    newDir = new Vector3(newDir.x * wallJumpHorizontalMult, newDir.y * wallJumpVerticalMult, newDir.z * wallJumpHorizontalMult);
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
            Vector3 crossProd = Vector3.Cross(wallNormal, new(control.GetVelocity().x, 0, control.GetVelocity().z));
            float leftOrRightOfNormal = Vector3.Dot(crossProd, Vector3.up);
            newDir = Quaternion.AngleAxis(
                (wallJumpMinimumAngle * 0.6f) * leftOrRightOfNormal, Vector3.up) * wallNormal;
        }
        //var dir = (newDir * wallJumpHorizontalMult + Vector3.up * wallJumpVerticalMult);
        //newDir = Vector3.RotateTowards(newDir, Vector3.up, 1.16937f, 0);
        newDir = new Vector3(newDir.x * wallJumpHorizontalMult, newDir.y, newDir.z * wallJumpHorizontalMult) + Vector3.up * wallJumpVerticalMult;
        ActivateWallJump(newDir);
    }

    public void ActivateWallJump(Vector3 dir)
    {
        Singleton.instance.ParticleEffects.SpawnSmoke(control.contactPoint, control.lastSteepNormal);
        StartJumpBrakes();
        StartCoroutine(JustJumped());
        control.SetVelocity(dir);
        graphics.rotation = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z), Vector3.up);
        //rotate.AlignToVelocityForTime(wallJumpRotationAlignDuration);
        control.InitAccelerationModReturn(accelModTimeOnWalljump, true);
    }

    // Rolling walljump
    public void RollWallJump()
    {
        control.stepsSinceLastSteep = wallJumpCoyoteFrames;
        jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
        currentJump = JumpType.ROLL_WALL;

        //anim.Play("jump", 0, 0);
        //_anim.StraightToJumpAnimation();
        Vector3 point = control.contactPoint == null ? transform.position : control.contactPoint;
        Singleton.instance.ParticleEffects.SpawnSmoke(point, control.lastSteepNormal);
        Vector3 wallNormal = new Vector3(control.lastSteepNormal.x, 0, control.lastSteepNormal.z).normalized;
        Vector3 newDir = wallNormal;// new Vector3(wallNormal.x * wallJumpHorizontalMult, wallNormal.y, wallNormal.z * wallJumpHorizontalMult) + Vector3.up * wallJumpVerticalMult;
        Vector3 inputDir = control.GetInput();
        upgrades.WalljumpUpgrades(control.steepNormal, control.contactPoint);
        //ActivateWallJump(newDir);



        // Rotate jumpDir towards input:
        if (inputDir.sqrMagnitude > 0.2f * 0.2f)
        {
            // Upwards walljump
            var dot = Vector3.Dot(wallNormal, inputDir.normalized);
            if (dot < -0.9f)
            {
                newDir = new Vector3(newDir.x * wallJumpHorizontalMult, newDir.y, newDir.z * wallJumpHorizontalMult) + Vector3.up * wallJumpVerticalMult;
                //newDir = Vector3.RotateTowards(
                //wallNormal, Vector3.up, 1.16937f, 0);
                ActivateWallJump(newDir);
                return;
            }
            else
            {
                Vector3 crossProd = Vector3.Cross(wallNormal, inputDir.normalized);
                float leftOrRightOfNormal = Vector3.Dot(crossProd, Vector3.up);

                // Upwards-diagonal walljump
                if (dot < -0.7f)
                {
                    newDir = Vector3.RotateTowards(
                        wallNormal, Vector3.up, 1.16937f, 0);
                    newDir = Quaternion.Euler(0, 45 * leftOrRightOfNormal, 0) * newDir;
                    newDir = new Vector3(newDir.x * wallJumpHorizontalMult, newDir.y * wallJumpVerticalMult, newDir.z * wallJumpHorizontalMult);
                    ActivateWallJump(newDir);
                    return;
                }
                else
                {
                    newDir = Quaternion.AngleAxis(
                        rollWallJumpMinimumAngle * leftOrRightOfNormal, Vector3.up) * wallNormal;
                }
            }
        }
        // Rotate dir from velocity when no input:
        else
        {
            Vector3 crossProd = Vector3.Cross(wallNormal, new(rolling.rollDir.x, 0, rolling.rollDir.z));
            float leftOrRightOfNormal = Vector3.Dot(crossProd, Vector3.up);
            newDir = Quaternion.AngleAxis(
                (rollWallJumpMinimumAngle * 0.6f) * leftOrRightOfNormal, Vector3.up) * wallNormal;
        }
        //var dir = (newDir * wallJumpHorizontalMult + Vector3.up * wallJumpVerticalMult);
        //newDir = Vector3.RotateTowards(newDir, Vector3.up, 1.16937f, 0);
        newDir = new Vector3(newDir.x * wallJumpHorizontalMult, newDir.y, newDir.z * wallJumpHorizontalMult).normalized;
        ActivateRollWallJump(newDir);
    }

    public void ActivateRollWallJump(Vector3 dir)
    {
        dir = Vector3.Normalize(new(dir.x, 0, dir.z));
        rolling.rollDir = dir;
        graphics.rotation = Quaternion.LookRotation(dir, Vector3.up);

        //jumpSpeed *= 0.75f;
        Vector3 jumpDir = Vector3.up;
        var velo = control.GetVelocity().magnitude;
        //float alignSpeedDot = Vector3.Dot(new Vector3(velo.x, 0, velo.z), jumpDir);
        //if (alignSpeedDot > 0f)
        //{
        //    jumpSpeed = Mathf.Max(jumpSpeed - alignSpeedDot, 0f);
        //}
        control.SetVelocity(
            new Vector3(dir.x * wallRollJump_moveSpdReduct * velo, 0, dir.z * wallRollJump_moveSpdReduct * velo) +
            (jumpDir * jumpSpeed));

        print("activating rollwallj "+Time.time);

        control.InitAccelerationModReturn(accelModTimeOnRollingWalljump, true);

        //StartJumpBrakes();
        StartCoroutine(JustJumped());
        StartJumpBrakes();
    }

    public void InitAirJump() {
        anim.Play("airDash_upwards", 0, 0);
        Singleton.instance.ParticleEffects.SpawnSmoke(transform.position + Vector3.down, Vector3.down);
        if (control.GetInput().sqrMagnitude > control.deadzoneSquared)
        {
            graphics.rotation = Quaternion.LookRotation(control.GetInput(), Vector3.up);
        }
        /*
        Vector3 force = InputCheckVec3();
        if (upwardsAirJump) {
            anim.Play("airDash_upwards", 0, 0);
            control.InitAccelerationModReturn(accelModTimeOnUpwardsAirJump, true);
        }
        else {
            anim.Play("airDash", 0, 0);
            control.InitAccelerationModReturn(accelModTimeOnAirJump, true);
            graphics.rotation = Quaternion.LookRotation(control.GetInput(), Vector3.up);
        }*/
        //rotate.AlignToVelocityForTime(airJumpRotationAlignDuration);

        var force = control.GetInput() * airJumpHorizForce + Vector3.up * airJumpVertiForce;
        rb.velocity = force;
        control.SetVelocity(force);
        /*control.SetVelocity(Vector3.zero);
        rb.velocity = Vector3.zero;
        rb.AddForce(force, ForceMode.Impulse);*/
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
        if (gravity.brakeCoroutine != null) {
            StopCoroutine(gravity.brakeCoroutine);
        }
        gravity.brakeCoroutine = StartCoroutine(gravity.JumpBrakes());


        if (gravity.apexGravCoroutine != null) {
            StopCoroutine(gravity.apexGravCoroutine);
        }
        gravity.apexGravCoroutine = StartCoroutine(gravity.ApexGravMultiplier());
    }

}
