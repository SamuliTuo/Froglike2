using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class PlayerLongJump : MonoBehaviour {

    [HideInInspector] public int stepsSinceOnWall = 0;

    [SerializeField] private float timeBeforeCanStun = 0.05f;
    [SerializeField] private int lateWallJumpMaxTime = 10;
    [SerializeField] private float LJHorizForce = 33f;
    [SerializeField] private float LJVertiForce = 11f;
    [SerializeField] private float LJWallJumpForce = 17f;
    [SerializeField] private float LJWallJumpAngleFromNormal = 65f;
    [SerializeField] private float LJSteerHoriz = 16f;
    [SerializeField] private float LJSteerUp = 3f;
    [SerializeField] private float LJSteerDown = 14f;
    [SerializeField] private float LJStunHorizF = 10f;
    [SerializeField] private float LJStunVertiF = 20f;
    [SerializeField] private float stunDuration = 1f;

    private PlayerController control;
    private PlayerJumping jump;
    private PlayerRolling roll;
    private PlayerJumpingUpgrades upgrades;
    private PlayerAnimations animations;
    private Animator anim;
    private Rigidbody rb;
    private Transform model;
    private Vector3 longJumpLastDir;
    private Vector3 wallNorm_latest, wallNorm_2ndLast, wallNorm_3rdLast, wallNorm_4thLast;
    private bool jumpPressed, attackUsed, specialUsed, applyAttackForce, applyLongJumpStopperForce, rotateModel;
    private bool stunned = false;
    private bool jumpForceApplied = false;
    private float t, stunT;


    void Start() {
        control = GetComponent<PlayerController>();
        jump = GetComponent<PlayerJumping>();
        roll = GetComponent<PlayerRolling>();
        upgrades = GetComponentInChildren<PlayerJumpingUpgrades>();
        animations = GetComponentInChildren<PlayerAnimations>();
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        model = transform.GetChild(0);
    }

    void OnJump(InputValue value) {
        jumpPressed = value.isPressed;
    }
    void OnSpecial(InputValue value) {
        if (stunned == false &&
            specialUsed == false &&
            value.isPressed == true &&
            control.state == PlayerStates.LONG_JUMP)
        {
            specialUsed = true;
            StartCoroutine(LongJumpAttack());
        }
    }
    void OnAttack() {
        if (stunned == false && 
            attackUsed == false && 
            control.state == PlayerStates.LONG_JUMP) 
        {
            attackUsed = true;
            StartCoroutine(LongJumpStoppingAttack());
        }
    }

    public void LongJump() {
        roll.StopRoll();
        rotateModel = true;
        jump.playerCanJump = false;
        jumpForceApplied = false;
        attackUsed = specialUsed = applyAttackForce = applyLongJumpStopperForce = stunned = false;
        wallNorm_latest = wallNorm_2ndLast = wallNorm_3rdLast = wallNorm_4thLast = Vector3.zero;
        stepsSinceOnWall = lateWallJumpMaxTime;
        control.state = PlayerStates.LONG_JUMP;
        t = 0;
        Singleton.instance.CameraChanger.ToggleCamera(cameras.LONGJUMP);
    }

    // Updating longjump
    public void LongJumpUpdate() {
        if (!stunned && rotateModel) {
            model.rotation = Quaternion.RotateTowards(
                model.rotation, Quaternion.LookRotation(rb.velocity), 100 * Time.deltaTime);

            //model.LookAt(model.position + rb.velocity);
        }
    }

    void ApplyJumpForce() {
        var velo = control.GetRelativeVelo();
        Vector3 jumpDir = new Vector3(velo.x, 0, velo.z).normalized;
        rb.velocity = (jumpDir * LJHorizForce) + (Vector3.up * LJVertiForce);
        control.SetVelocity((jumpDir * LJHorizForce) + (Vector3.up * LJVertiForce));
        longJumpLastDir = (jumpDir * LJHorizForce) + (Vector3.up * LJVertiForce);
        model.rotation = Quaternion.LookRotation(longJumpLastDir, Vector3.up);
        StartCoroutine(jump.JustJumped());
        anim.Play("longJump", 0, 0);
        upgrades.JumpUpgrades();
        jumpForceApplied = true;
    }

    public void LongJumpFixed() 
    {
        t += Time.deltaTime;
        if (stunned) {
            HandleLongJumpStun();
            return;
        }
        if (jumpForceApplied == false) {
            ApplyJumpForce();
        }

        // attack - logics
        if (applyAttackForce) 
        {
            rb.velocity = AttackForce();
            applyAttackForce = false;
        }
        if (applyLongJumpStopperForce) 
        {
            rb.velocity = SpecialForce();
            applyLongJumpStopperForce = false;
        }
        // landing
        LJSteering();
        if (control.PlayerGrounded && jump.playerCanJump) 
        {
            Singleton.instance.CameraChanger.ToggleCamera(cameras.NORMAL);
            roll.InitLandingRoll();
        }
        KeepTrackOfWalls();
        //if (stepsSinceOnWall < lateWallJumpMaxTime) {
            if (control.contactTag == "Carryable")
            {
                rb.velocity *= 0.97f;
            }
            else if (control.contactTag != "Lootable" && control.contactTag != "Collectable")
            {
                if (t >= timeBeforeCanStun)
                {
                    if (CheckCollisionAngleForStun())
                    {
                        StartCoroutine(GetStunned());
                        //HitStop.current.RestoreTimeScale();
                        return;
                    }
                    else if (jump.playerJumpPressed)
                    {
                        LJWallJump();
                    }
                    //HitStop.current.SlowTime(0.3f, 3);
                }
            }
        //}
    }

    void KeepTrackOfWalls() {
        if (control.PlayerOnSteep && jump.playerCanJump) {
            stepsSinceOnWall = 0;
            wallNorm_4thLast = wallNorm_3rdLast;
            wallNorm_3rdLast = wallNorm_2ndLast;
            wallNorm_2ndLast = wallNorm_latest;
            wallNorm_latest = control.steepNormal;
        }
        else if (control.PlayerOnSteep == false) {
            longJumpLastDir = rb.velocity;
        }
        else {
            stepsSinceOnWall++;
            if (stepsSinceOnWall > lateWallJumpMaxTime)
                wallNorm_latest = wallNorm_2ndLast = wallNorm_3rdLast = wallNorm_4thLast = Vector3.zero;
        }
    }

    void LJSteering() {
        if (jumpPressed) {
            rb.velocity += Vector3.up * LJSteerUp * Time.deltaTime;
        }
        else if (control.yAxis < -control.deadzone) {
            rb.velocity += Vector3.down * LJSteerDown * Time.deltaTime * Mathf.Min(t * 1.6f, 1);
        }
        if (control.xAxis < -control.deadzone || control.xAxis > control.deadzone) {
            rb.velocity +=
                model.right * LJSteerHoriz * control.xAxis * Mathf.Min(t, 1) * Time.deltaTime;
        }
    }

    // Attacks
    IEnumerator LongJumpAttack() {
        anim.Play("attack_longJump", 0, 0);
        yield return Helpers.GetWait(0.2f);
        applyAttackForce = true;
    }

    IEnumerator LongJumpStoppingAttack() {
        rotateModel = false;
        model.LookAt(model.position + new Vector3(rb.velocity.x, 0, rb.velocity.z));
        anim.Play("special_longJump", 0, 0);
        applyLongJumpStopperForce = true;
        yield return Helpers.GetWait(0.4f);

        Singleton.instance.CameraChanger.ToggleCamera(cameras.NORMAL);
        control.state = PlayerStates.NORMAL;
        if (control.PlayerGrounded == false) {
            animations.FadeToAnimation("jump", 0.2f, 0, 0.75f);
        }
    }
    Vector3 AttackForce() {
        if (rb.velocity.y < 0)
            return new Vector3(rb.velocity.x * 1.2f, 6, rb.velocity.z * 1.2f);
        else
            return rb.velocity * 1.2f + Vector3.up * 3;
    }
    Vector3 SpecialForce() {
        return rb.velocity * 0.2f + Vector3.up * 10;
    }

    // Wall contacts
    void LJWallJump() {
        Vector3 newDir;
        if (wallNorm_4thLast != Vector3.zero)
            newDir = wallNorm_4thLast;
        else if (wallNorm_3rdLast != Vector3.zero)
            newDir = wallNorm_3rdLast;
        else if (wallNorm_2ndLast != Vector3.zero)
            newDir = wallNorm_2ndLast;
        else if (wallNorm_latest != Vector3.zero)
            newDir = wallNorm_latest;
        else return;

        float leftOrRightOfNormal = Vector3.Dot(
            Vector3.Cross(newDir.normalized, control.GetRelativeVelo().normalized), Vector3.up);
        if (leftOrRightOfNormal <= 0)
            leftOrRightOfNormal = -1;
        else
            leftOrRightOfNormal = 1;

        newDir = Quaternion.AngleAxis(LJWallJumpAngleFromNormal * leftOrRightOfNormal, Vector3.up) * newDir;
        print(newDir);
        if (newDir.y < 0.5f) {
            newDir.y = 0.5f;
        }
        control.SetVelocity(newDir.normalized * LJWallJumpForce);
        rb.velocity = newDir.normalized * LJWallJumpForce;
        model.rotation = Quaternion.LookRotation(newDir, Vector3.up);

        upgrades.WalljumpUpgrades(newDir, control.contactPoint);
        anim.Play("longJump", 0, 0);
        StartCoroutine(jump.JustJumped());
        wallNorm_latest = wallNorm_2ndLast = wallNorm_3rdLast = wallNorm_4thLast = Vector3.zero;
        stepsSinceOnWall = lateWallJumpMaxTime;
        //ParticleEffects.current.PlayWallJumpVFX(_controller.contactPoint, _controller.steepNormal);
        //FrogAudioPlayer.current.PlayLongjumpSFX();
        //HitStop.current.RestoreTimeScale();
        //HitStop.current.StopTime(0.13f);
        //HitStop.current.SpeedUp(1f, 0.5f);
    }

    bool CheckCollisionAngleForStun() {
        float dot = Vector3.Dot(longJumpLastDir.normalized, control.steepNormal.normalized);
        if (dot < -0.8f) {
            upgrades.WalljumpUpgrades(control.steepNormal, control.contactPoint);
            return true;
        }
        else if (dot < -0.65f) {
            return false;
        }
        else {
            return false;
        }
    }

    // Tee näist stunneista joku fiksumpi kourutiini vaikka yhdistä siihen graduaalisti liikkeen takaisin antavaan joka taitaa olla controllerissa
    IEnumerator GetStunned() {
        Vector3 normal2D = new Vector3(control.steepNormal.x, 0, control.steepNormal.z).normalized;
        Vector3 stunDir = (normal2D * LJStunHorizF) + (Vector3.up * LJStunVertiF);
        stunned = true;
        control.SetVelocity(Vector3.zero);
        model.LookAt(model.position - normal2D);
        rb.velocity = Vector3.zero;
        //FrogAudioPlayer.current.PlayLongjumpStunSFX();
        anim.Play("longJumpStun", 0, 0);
        yield return Helpers.GetWait(0.05f);

        Singleton.instance.CameraChanger.ToggleCamera(cameras.NORMAL);
        control.SetVelocity(Vector3.zero);
        rb.velocity = Vector3.zero;
        rb.AddForce(stunDir, ForceMode.Impulse);
        stunT = 0;
    }

    void HandleLongJumpStun() {
        stunT += Time.deltaTime;
        if (stunT > stunDuration && (control.PlayerGrounded || control.playerOnSlippery) 
            || control.PlayerGrounded) 
        {
            //_run.runningSpeedMult = 1;
            //_run.playerRunning = false;
            EndLongJumpStun();
        }
    }

    void EndLongJumpStun() {
        Singleton.instance.CameraChanger.ToggleCamera(cameras.NORMAL);
        stunned = false;
        anim.CrossFade("idleWalkRun", 0.2f, 0);
        control.state = PlayerStates.NORMAL;
    }
}
