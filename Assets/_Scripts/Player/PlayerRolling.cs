using Cinemachine;
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

    [Header("cRoll camera")]
    [SerializeField] private float cameraFollowToggleSpeedOfCRollPerc = 0.5f;
    [SerializeField] private float cRollCameraSlowMovingResetSpeed = 10;
    [SerializeField] private float cRollCameraFastMovingResetSpeedMin = 0.6f;
    [SerializeField] private float cRollCameraFastMovingResetSpeedMax = 0.1f;
    [SerializeField] private float cRollSlowCameraResetTime = 20;

    [Space]
    public float cRollMoveSpeed = 33;
    public float runRollingSpeedMultiplier = 10f;
    [SerializeField] private float lerpSpeed = 3.3f;
    [SerializeField] private float longJLandingLerpSpeed = 3.3f;
    [SerializeField] private float longLandingLerpStartPerc = 0.2f;
    [Space]
    [SerializeField] private float stopSpdWalk = 5;
    [SerializeField] private float stopSpdRun = 5;
    [SerializeField] private float maxSpeed = 33;
    [Header("cRoll speed changes")]
    [SerializeField] private float rollAcceleratingRate_normal = 1;
    [SerializeField] private float rollAcceleratingRate_normal_running = 1;
    [SerializeField] private float rollAcceleratingRate_opposite = 3;
    [SerializeField] private float rollAcceleratingRate_opposite_running = 3;
    [SerializeField] private float rollAcceleratingRate_decelerateToMaxSpeed = 0.1f;
    [SerializeField] private float rollAcceleratingRate_noInput = 0.1f;
    [Header("cRoll turning")]
    [SerializeField] private float rollTurnRate_normal = 1;
    [SerializeField] private float rollTurnRate_normal_running = 1;
    [SerializeField] private float rollTurnRate_opposite = 2;
    [SerializeField] private float rollTurnRate_opposite_running = 3;
    [Space(10)]
    [SerializeField] private float rollCloudTrailPlaybackSpeed = 2f;

    private PlayerController control;
    private PlayerAnimations animate;
    private PlayerRotate rotate;
    private PlayerCrawl crawl;
    private PlayerRollingUpgrades rollUpgradeEffects;
    private PlayerColliderChanges colliders;
    private PlayerInput input;
    private Transform graphics;
    private Rigidbody rb;
    private Animator anim;
    [SerializeField] private CinemachineVirtualCamera rollCinemachineVirtualCamera = null;
    private CinemachinePOV rollCamera;
    private float currentLerpSpeed;
    private float startSpeed;
    private float stopSpd = 1;
    private float t;
    //private float xAxis, yAxis;

    void Start() {
        rollCamera = rollCinemachineVirtualCamera.GetCinemachineComponent<CinemachinePOV>();
        control = GetComponent<PlayerController>();
        animate = GetComponentInChildren<PlayerAnimations>();
        rotate = GetComponentInChildren<PlayerRotate>();
        crawl = GetComponent<PlayerCrawl>();
        rollUpgradeEffects = GetComponentInChildren<PlayerRollingUpgrades>();
        colliders = GetComponent<PlayerColliderChanges>();
        graphics = transform.GetChild(0);
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        input = GetComponent<PlayerInput>();
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
        if (rollCamera == null)
        {
            rollCamera = rollCinemachineVirtualCamera.GetComponent<CinemachinePOV>();
            print(rollCinemachineVirtualCamera);
            print(rollCamera);
        }
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
        bool runPressed = input.actions["Run"].IsPressed();
        print("runpressd : " + runPressed);
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

                rollingSpeed = GetCorrectContinuousRollRollSpeed(runPressed);
                if (control.getGrounded() && runPressed)
                {
                    anim.SetFloat("RollVelo", Mathf.Clamp(rb.velocity.magnitude * 2, 30, 90));
                    Singleton.instance.ParticleEffects.SpawnContinuousSmoke(transform.position - Vector3.up - graphics.forward * 0.5f, -graphics.forward + Vector3.up);
                }
                else
                {
                    anim.SetFloat("RollVelo", rb.velocity.magnitude);
                }
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



        // Steering...
        float changeSpd = continuousRoll ? cRollDirChangeSped : rollDirChangeSpeed;
        // ...on ground:
        if (control.PlayerGrounded)
        {
            // smoke trail
            if (rb.velocity.sqrMagnitude > smokeTrailThreshholdSpeed)
            {
                Singleton.instance.ParticleEffects.SpawnContinuousSmoke(transform.position + Vector3.down, -rollDir + Vector3.up * 0.5f);
                smokeTrailT = 0;
            }

            float changeAmount = continuousRoll ? cRollDirChangeAmount : rollDirChangeAmount;
            rollDir = HandleSteering(changeSpd, changeAmount, runPressed);
        }// ...in air:
        else
        {
            // smoke trail
            if (!control.PlayerOnSteep && smokeTrailT < smokeTrailTimeInAir)
            {
                smokeTrailT += Time.deltaTime;
                Singleton.instance.ParticleEffects.SpawnContinuousSmoke(transform.position + Vector3.down, -rollDir + Vector3.up * 0.5f);
            }

            float steerChangeAmount = continuousRoll ? cRollDirChangeAmount_air : rollDirChangeAmount_air;
            rollDir = HandleSteering(changeSpd, steerChangeAmount, runPressed);
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
        
        // Camera auto re-centering
        if (currentVeloSqrMag < (cRollMoveSpeed * cRollMoveSpeed) * cameraFollowToggleSpeedOfCRollPerc)
        {
            rollCamera.m_HorizontalRecentering.m_RecenteringTime = rollCamera.m_VerticalRecentering.m_RecenteringTime = cRollSlowCameraResetTime;
        }
        else
        {
            float perc = (currentVeloSqrMag - cRollMoveSpeed * cRollMoveSpeed) / (control.maxVelocitySoftCap - cRollMoveSpeed * cRollMoveSpeed);
            //print("lerping camera to follow with perc : "+perc);
            rollCamera.m_HorizontalRecentering.m_RecenteringTime = Mathf.Lerp(cRollCameraFastMovingResetSpeedMin, cRollCameraFastMovingResetSpeedMax, perc);
            rollCamera.m_VerticalRecentering.m_RecenteringTime = Mathf.Lerp(cRollCameraFastMovingResetSpeedMin, cRollCameraFastMovingResetSpeedMax, perc);

        }

        // Accelerations
        if (dotProd > 0)
        {
            // This right now controls SPEED AND STEERING!
            // Thats stupid.
            // Lets make some sense to it soon

            float sum = currentVeloSqrMag - desiredVeloSqrMag;
            //print("velos: "+sum);

            /// mitä jos tääl ois toi dotProd, sit sillä kerrottais jotenkin niitä kaasu ja ohjaus muutoksia?
            /// 
            /// jotenki :
            /// - (input ja velocity) dotti
            /// - kerro sillä joku ja käännä newVeloa jotenkin Vector2.Turn(dotattu vectori, turnRate_spesifijoku);
            /// - kerro sillä tai käänteisesti slilä tai jotenkin Vector2.newVelo * kerroin -> uus pituus.
            /// - !!! Tsekkaa, että täällä on deltaTime paikallaan jossain!!!!


            if (currentVeloSqrMag > desiredVeloSqrMag)
            {
                float _perc = Mathf.Clamp(sum / control.maxVelocityHarderCap - desiredVeloSqrMag, 0, 1);
                float lerpAmount = Mathf.Lerp(dotProd, 0, _perc * _perc);
                if (runPressed)
                {
                    newVelo = Vector2.MoveTowards(currentVelo, desiredVelo, Mathf.Lerp(rollAcceleratingRate_normal_running, rollAcceleratingRate_decelerateToMaxSpeed, lerpAmount));
                }
                else
                {
                    newVelo = Vector2.MoveTowards(currentVelo, desiredVelo, Mathf.Lerp(rollAcceleratingRate_normal, rollAcceleratingRate_decelerateToMaxSpeed, lerpAmount));
                }
            }
            else
            {
                if (runPressed)
                {
                    newVelo = Vector2.MoveTowards(currentVelo, desiredVelo, rollAcceleratingRate_normal_running);
                }
                else
                {
                    newVelo = Vector2.MoveTowards(currentVelo, desiredVelo, rollAcceleratingRate_normal);
                }
            }
        }
        else if (control.GetInput().magnitude < control.deadzone)
        {
            if (currentVeloSqrMag > (cRollMoveSpeed * cRollMoveSpeed) * 0.8f)
            {
                newVelo = Vector2.MoveTowards(currentVelo, desiredVelo, rollAcceleratingRate_noInput);
            }
            else
            {
                newVelo = Vector2.MoveTowards(currentVelo, desiredVelo, rollAcceleratingRate_normal);
            }
        }
        else
        {
            if (runPressed)
            {
                newVelo = Vector2.MoveTowards(currentVelo, desiredVelo, rollAcceleratingRate_opposite_running);
            }
            else
            {
                newVelo = Vector2.MoveTowards(currentVelo, desiredVelo, rollAcceleratingRate_opposite);
            }
        }
        //print("current velo: " + currentVelo.magnitude + ", desired velo: " + desiredVelo.magnitude + ", new velo: " + newVelo.magnitude);
        return xAxis * (newVelo.x - currX) + zAxis * (newVelo.y - currZ);
    }



    private float GetCorrectContinuousRollRollSpeed(bool runPressed)
    {
        float r = cRollMoveSpeed;
        if (runPressed)
        {
            r = cRollMoveSpeed * runRollingSpeedMultiplier;
        }
        
        return r;
    }

    private Vector3 HandleSteering(float changeSpd, float changeAmount, bool runPressed)
    {
        if (runPressed)
        {
            if (control.GetInput().sqrMagnitude > control.deadzoneSquared)
            {
                return Vector3.RotateTowards(rollDir, control.GetInput().normalized, changeAmount, changeSpd).normalized;
            }
            else
            {
                return Vector3.RotateTowards(rollDir, new Vector3(graphics.forward.x, 0, graphics.forward.z).normalized, changeAmount, changeSpd).normalized;
            }
        }
        else
        {
            return Vector3.RotateTowards(rollDir, control.GetInput(), changeAmount, changeSpd).normalized;
        }
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
