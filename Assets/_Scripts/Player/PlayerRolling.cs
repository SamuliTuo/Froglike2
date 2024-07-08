using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRolling : MonoBehaviour {

    [HideInInspector] public Vector3 rollDir;
    [HideInInspector] public bool continuousRoll = false;
    [HideInInspector] public bool canRoll = true;
    [HideInInspector] public float rollingSpeed;

    //[SerializeField] private float rollPressDelay = 0.3f;
    [SerializeField] private float rollStartingSpeed = 5f;
    [SerializeField] private float rollDirChangeSpeed = 0.01f;
    [SerializeField] private float cRollDirChangeSped = 0.5f;
    [SerializeField] private float rollDirChangeAmount = 0.01f;
    [SerializeField] private float cRollDirChangeAmount = 0.03f;
    [SerializeField] private float rollDirChangeAmount_air = 0.01f;
    [SerializeField] private float cRollDirChangeAmount_air = 0.03f;

    [Space]
    public float cRollMoveSpeed = 33;
    [SerializeField] private float lerpSpeed = 3.3f;
    [SerializeField] private float longJLandingLerpSpeed = 3.3f;
    [SerializeField] private float longLandingLerpStartPerc = 0.2f;
    [Space]
    [SerializeField] private float stopSpdWalk = 5;
    [SerializeField] private float stopSpdRun = 5;
    [SerializeField] private float maxSpeed = 33;
    [Header("Roll control")]
    [SerializeField] private float rollSteeringRate_normal = 1;
    [SerializeField] private float rollSteeringRate_opposite = 3;
    [SerializeField] private float rollSteeringRate_decelerateToMaxSpeed = 0.1f;
    [SerializeField] private float rollSteeringRate_noInput = 0.1f;
    //[SerializeField] private float rollCloudTrailPlaybackSpeed = 2f;

    private PlayerController control;
    private PlayerAnimations animate;
    private PlayerRotate rotate;
    private PlayerCrawl crawl;
    private PlayerRollingUpgrades rollUpgradeEffects;
    private PlayerColliderChanges colliders;
    private Transform graphics;
    private Rigidbody rb;
    private Animator anim;
    private float currentLerpSpeed;
    private float startSpeed;
    private float stopSpd = 1;
    private float t;
    //private float xAxis, yAxis;

    void Start() {
        control = GetComponent<PlayerController>();
        animate = GetComponentInChildren<PlayerAnimations>();
        rotate = GetComponentInChildren<PlayerRotate>();
        crawl = GetComponent<PlayerCrawl>();
        rollUpgradeEffects = GetComponentInChildren<PlayerRollingUpgrades>();
        colliders = GetComponent<PlayerColliderChanges>();
        graphics = transform.GetChild(0);
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    // Called when Roll-input is pressed
    void OnRoll() {
        if (continuousRoll && t >= 1) 
        {
            continuousRoll = false;
            t = 1;
        }
        else if (control.state == PlayerStates.ROLL && t < 1) 
        {
            continuousRoll = true;
        }
        else if (canRoll && (control.state == PlayerStates.NORMAL || control.state == PlayerStates.CRAWL)) 
        {
            InitRoll();
        }
    }

    public void ContinueCRoll()
    {
        canRoll = false;
        continuousRoll = true;
        control.SetAccelerationMod(1);
        rotate.SetRotateSpdMod(1);
        control.state = PlayerStates.ROLL;
        anim.Play("roll", 0, 0);
        t = 1.99f;
        cRollSet = false;
        rollUpgradeEffects.StartRoll();
        rollDir = Vector3.RotateTowards(
            rollDir, control.GetInput(), 10000, 10000).normalized;
    
}

    public void InitRoll(bool cRoll = false) {
        cRollSet = false;
        canRoll = false;
        continuousRoll = cRoll;
        control.StopAfterSpecialGravity();
        control.SetAccelerationMod(1);
        rotate.SetRotateSpdMod(1);
        control.state = PlayerStates.ROLL;
        anim.Play("roll", 0, 0);
        startSpeed = maxSpeed;
        //ParticleEffects.current.PlayJumpVFX(rollCloudTrailPlaybackSpeed);
        stopSpd = stopSpdWalk;
        if (control.GetInput().sqrMagnitude == 0) {
            rollDir = graphics.forward;
        }
        else {
            rollDir = control.GetInput().normalized;
        }
        graphics.rotation = Quaternion.LookRotation(rollDir, Vector3.up);
        colliders.ChangeToSmallCollider();
        rb.velocity = rollDir * rollStartingSpeed;
        if (!control.PlayerGrounded) {
            rb.AddForce(Vector3.down * 3 * rb.mass, ForceMode.Impulse);
        }
        currentLerpSpeed = lerpSpeed;
        rollUpgradeEffects.StartRoll();
        t = 0;
    }

    public void InitLandingRoll() {
        canRoll = false;
        continuousRoll = false;
        control.state = PlayerStates.ROLL;
        //FrogAudioPlayer.current.PlayRoll();
        //ParticleEffects.current.PlayJumpVFX(rollCloudTrailPlaybackSpeed);
        anim.Play("landingRoll", -1, longLandingLerpStartPerc);
        startSpeed = Mathf.Max(control.GetVelocity().magnitude, maxSpeed);
        stopSpd = stopSpdRun;//stopSpdWalk;
        rollDir = new Vector3(graphics.forward.x, 0, graphics.forward.z).normalized;
        currentLerpSpeed = longJLandingLerpSpeed;
        t = longLandingLerpStartPerc;
        colliders.ChangeToSmallCollider();
    }

    bool cRollSet = false;
    float smokeTrailT = 0;
    public float smokeTrailTimeInAir = 0.5f;
    public float smokeTrailThreshholdSpeed = 4f;

    public Vector3 RollingSpeedVector3(
        Vector3 xAxis, float currX,
        Vector3 zAxis, float currZ)
    {
        if (t < 1) {
            t += currentLerpSpeed * Time.deltaTime;
            float perc = Mathf.Sin(t * Mathf.PI * 0.5f);
            rollingSpeed = Mathf.Lerp(startSpeed, maxSpeed, perc);
        }
        else if (t < 2) {
            //stop if speed falls 2 low
            //if (rb.velocity.sqrMagnitude < (cRollMoveSpeed * 0.3f) * (cRollMoveSpeed * 0.3f)) {
            //    animate.FadeToAnimation("idleWalkRun", 0.2f, 0);
            //    EndRoll();
            //    return rb.velocity;
            //}

            if (continuousRoll) {
                if (cRollSet == false)
                {
                    Singleton.instance.CameraChanger.ToggleCamera(cameras.ROLL);
                    animate.FadeToAnimation("roll_continuous", 0.15f, 0);
                    cRollSet = true;
                }
                anim.SetFloat("RollVelo", rb.velocity.magnitude);
                rollingSpeed = cRollMoveSpeed;
            }
            else {
                t += currentLerpSpeed * Time.deltaTime * 2;
                float perc = t - 1;
                perc = perc * perc * perc * (perc * (6f * perc - 15f) + 10f);
                rollingSpeed = Mathf.Lerp(maxSpeed, stopSpd, perc);
            }
        }
        else {
            rollingSpeed = stopSpd;
            EndRoll();
        }

        rollUpgradeEffects.RollEffectsSpawnAOEs();

        // Steering:
        if (control.PlayerGrounded)
        {
            float changeSpd = continuousRoll ? cRollDirChangeSped : rollDirChangeSpeed;
            float changeAmount = continuousRoll ? cRollDirChangeAmount : rollDirChangeAmount;
            rollDir = Vector3.RotateTowards(
            rollDir, control.GetInput(), changeAmount, changeSpd).normalized;

            // smoke trail
            if (rb.velocity.sqrMagnitude > smokeTrailThreshholdSpeed)
            {
                Singleton.instance.ParticleEffects.SpawnContinuousSmoke(transform.position + Vector3.down, -rollDir);
                smokeTrailT = 0;
            }
        }
        else
        {
            if (!control.PlayerOnSteep && smokeTrailT < smokeTrailTimeInAir)
            {
                smokeTrailT += Time.deltaTime;
                Singleton.instance.ParticleEffects.SpawnContinuousSmoke(transform.position + Vector3.down, -rollDir);
            }
            //if (control.PlayerOnSteep && control.GetInput().magnitude < control.deadzone)
            //{
            //    EndRoll();
            //    return Vector3.zero;
            //}
            float changeSpd = continuousRoll ? cRollDirChangeSped : rollDirChangeSpeed;
            float changeAmount = continuousRoll ? cRollDirChangeAmount_air : rollDirChangeAmount_air;
            rollDir = Vector3.RotateTowards(
            rollDir, control.GetInput(), changeAmount, changeSpd).normalized;
        }

        // Speed handling
        float desiredX = (rollDir * rollingSpeed).x;
        float desiredZ = (rollDir * rollingSpeed).z;
        Vector2 currentVelo = new Vector2(currX, currZ);
        Vector2 desiredVelo = new Vector2(desiredX, desiredZ);
        float currentVeloSqrMag = currentVelo.sqrMagnitude;
        float desiredVeloSqrMag = desiredVelo.sqrMagnitude;
        Vector2 newVelo;
        float dotProd = Vector3.Dot(desiredVelo.normalized, currentVelo.normalized);
        if (dotProd > 0)
        {
            float sum = currentVeloSqrMag - desiredVeloSqrMag;
            //print("velos: "+sum);
            if (currentVeloSqrMag > desiredVeloSqrMag)
            {
                float _perc = Mathf.Clamp(sum / control.maxVelocityHarderCap - desiredVeloSqrMag, 0, 1);
                float lerpAmount = Mathf.Lerp(dotProd, 0, _perc * _perc);
                newVelo = Vector2.MoveTowards(
                    currentVelo, 
                    desiredVelo, 
                    Mathf.Lerp(rollSteeringRate_normal, rollSteeringRate_decelerateToMaxSpeed, lerpAmount));
            }
            else
            {
                newVelo = Vector2.MoveTowards(currentVelo, desiredVelo, rollSteeringRate_normal);
            }
        }
        else if (control.GetInput().magnitude < control.deadzone)
        {
            newVelo = Vector2.MoveTowards(currentVelo, desiredVelo, rollSteeringRate_noInput);
        }
        else
        {
            newVelo = Vector2.MoveTowards(currentVelo, desiredVelo, rollSteeringRate_opposite);
        }
        //print("current velo: " + currentVelo.magnitude + ", desired velo: " + desiredVelo.magnitude + ", new velo: " + newVelo.magnitude);
        return xAxis * (newVelo.x - currX) + zAxis * (newVelo.y - currZ);
    }

    public void RollingHitEffects(Collision col) {
        rollUpgradeEffects.RollEffectsCollisions(col);
    }

    public void EndRoll() {
        t = 2;
        continuousRoll = false;
        Singleton.instance.CameraChanger.ToggleCamera(cameras.NORMAL);

        if (colliders.TryToStandUp() == false) {
            crawl.InitCrawlOnStuckUnder();
        }
        else {
            colliders.ChangeToStandUpColliders();
            control.moveSpeedMod = 1;
            rotate.SetRotateSpdMod(1);
            control.state = PlayerStates.NORMAL;
        }
    }

    public void StopRoll(bool turnOffRollCamera = false) {
        t = 2;
        continuousRoll = false;
        if (turnOffRollCamera)
        {
            Singleton.instance.CameraChanger.ToggleCamera(cameras.NORMAL);
        }
    }
}
