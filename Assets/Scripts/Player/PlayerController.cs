using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerStates{
    NOT_IN_CONTROL,
    NORMAL,
    ROLL,
    CRAWL,
    ATTACK,
    BASEBALL,
    SAMURAI,
    ROLL_SLASH,
    GROUND_POUND,
    CLIMB,
    LONG_JUMP,
    CARRYING,
    TONGUE_PULL,
    GRAPPLE
};

public class PlayerController : MonoBehaviour {

    public float walkSpd = 8f;
    public float runSpd = 12f;
    public float deadzone = 0.1f;
    
    [HideInInspector] public float deadzoneSquared;
    [HideInInspector] public int stepsSinceLastGrounded;
    [HideInInspector] public PlayerStates state = PlayerStates.NORMAL;
    [HideInInspector] public float moveSpeedMod = 1;
    [HideInInspector] public float xAxis, yAxis;
    [HideInInspector] public Vector3 contactPoint, contactNormal, steepNormal;
    [HideInInspector] public string contactTag;
    [HideInInspector] public bool playerOnSlippery;
    public bool PlayerGrounded => groundContactCount > 0;
    public bool PlayerOnSteep => steepContactCount > 0;

    [SerializeField] private Transform playerInputSpace = default;
    [SerializeField] private float walkAcceleration = 60;
    [SerializeField] private float runAcceleration = 120;
    [SerializeField] private float maxJumpAcceleration = 25;
    [Space]
    [SerializeField, Range(0f, 90f)] private float maxGroundAngle = 40f;
    [SerializeField, Range(0f, 90f)] private float maxStairsAngle = 50f;
    [SerializeField, Range(0f, 90f)] private float maxSlipperyAngle = 0f;
    [SerializeField, Range(0f, 90f)] private float maxMediumSlipAngle = 20f;
    [Header("Snapping player to ground:")]
    [SerializeField, Range(0f, 100f)] private float maxSnapSpeed = 25f;
    [SerializeField, Min(0f)] private float probeDistance = 2.5f;
    [SerializeField] private LayerMask probeMask = -1, stairsMask = -1;

    //private PlayerRunning run;
    private PlayerRotate rotate;
    private PlayerAnimations animate;
    private PlayerGravity gravity;
    private PlayerCarrying carry;
    private PlayerJumping jump;
    private PlayerLongJump longJump;
    private PlayerAttacks attack;
    private PlayerSpecialAttack special;
    private PlayerRunning run;
    private PlayerRolling roll;
    private PlayerCrawl crawl;
    private PlayerClimbing climb;
    private TongueController tongue;
    private PlayerGrapple grapple;
    private PlayerHurt hurt;
    private PlayerColliderChanges colChanger;
    //private PlayerCarrying carry;
    //private PlayerAttacking attack;
    private Transform graphics;
    private Rigidbody rb, connectedRb, previousConnectedRb;
    private Animator anim;
    private Coroutine accelModCoroutine;
    private Vector3 inputVector;
    private Vector3 velocity, relativeVelo, desiredVelocity, connectionVelocity;
    private Vector3 connectionWorldPos, connectionLocalPos;
    private int groundContactCount, steepContactCount;
    private float groundDot, minGroundDotProd, minSlipperyDotProd, minMediumSlipDotProd, minStairsDotProd;
    private float accelerationMod = 1;


    void Start() {
        Singleton.instance.SetPlayerScripts();
        graphics = transform.GetChild(0);
        rb = GetComponent<Rigidbody>();
        rotate = GetComponentInChildren<PlayerRotate>();
        animate = GetComponentInChildren<PlayerAnimations>();
        gravity = GetComponent<PlayerGravity>();
        carry = GetComponent<PlayerCarrying>();
        jump = GetComponent<PlayerJumping>();
        longJump = GetComponent<PlayerLongJump>();
        attack = GetComponent<PlayerAttacks>();
        special = GetComponent<PlayerSpecialAttack>();
        run = GetComponent<PlayerRunning>();
        roll = GetComponent<PlayerRolling>();
        crawl = GetComponent<PlayerCrawl>();
        climb = GetComponent<PlayerClimbing>();
        tongue = GetComponent<TongueController>();
        grapple = GetComponent<PlayerGrapple>();
        hurt = GetComponent<PlayerHurt>();
        colChanger = GetComponent<PlayerColliderChanges>();
        anim = GetComponentInChildren<Animator>();
        deadzoneSquared = deadzone * deadzone;
        CalculateGroundAngleDotProducts();
    }

    void Update() {
        //Debug.Log(state);
        //print(PlayerGrounded);
        //print("State = " + state + ",  attack hijack = " + attack.hijackControls + ",  special hijack = " + special.hijackControls);

        if (state == PlayerStates.NOT_IN_CONTROL || state == PlayerStates.GRAPPLE) {
            return;
        }
        InputsToCameraSpace();
        /*
        if (state == PlayerStates.CLIMB) {
            climb.ClimbUpdate();
            return;
        }*/
        if (state == PlayerStates.CARRYING) {
            carry.CarryObj();
        }
        else if (state == PlayerStates.LONG_JUMP) {
            longJump.LongJumpUpdate();
        }
        crawl.CrawlUpdate();
        animate.AnimateNormalState();
        rotate.RotatePlayer();
    }
    
    void FixedUpdate() {
        if (state == PlayerStates.NOT_IN_CONTROL) {
            gravity.SimpleGravity();
            return;
        }
        if (state == PlayerStates.GRAPPLE) {
            return;
        }
        else if (state == PlayerStates.CLIMB) {
            climb.ClimbFixed();
            return;
        }
        else if (state == PlayerStates.LONG_JUMP) {
            longJump.LongJumpFixed();
        }
        UpdateState();
        AdjustVelocity();
        jump.JumpFixedUpdate();
        rb.velocity = velocity;
        gravity.HandleGravity();
        //climb.GrabWall(inputVector);
        ClearState();
    }

    void InputsToCameraSpace() {
        Vector2 playerInput;
        playerInput.x = Mathf.Abs(xAxis) > deadzone ? xAxis : 0;
        playerInput.y = Mathf.Abs(yAxis) > deadzone ? yAxis : 0;
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);
        if (playerInputSpace) {
            Vector3 forward = playerInputSpace.forward;
            forward.y = 0f;
            forward.Normalize();
            Vector3 right = playerInputSpace.right;
            right.y = 0f;
            right.Normalize();
            inputVector = forward * playerInput.y + right * playerInput.x;
            desiredVelocity = inputVector * walkSpd * moveSpeedMod; // *_running.runningSpeedMult 
        }
        else {
            desiredVelocity =
                new Vector3(playerInput.x, 0f, playerInput.y) * walkSpd;
        }
    }

    void UpdateState() {
        stepsSinceLastGrounded++;
        velocity = rb.velocity;
        if (PlayerGrounded || SnapToGround()) {
            stepsSinceLastGrounded = 0;
            //jump.lateJumpTimer = jump.maxLateJumpTimer;
            if (groundContactCount > 1) {
                contactNormal.Normalize();
            }
        }
        else {
            jump.lateJumpTimer--;
            if (jump.lateJumpTimer <= 0) {
                contactNormal = Vector3.up;
            }
        }
        if (connectedRb) {
            UpdateConnectionState();
        }
    }

    void AdjustVelocity() {
        Vector3 xAx = ProjectOnContactPlane(Vector3.right).normalized;
        Vector3 zAx = ProjectOnContactPlane(Vector3.forward).normalized;
        relativeVelo = velocity - connectionVelocity;

        if (state == PlayerStates.LONG_JUMP) { //!jump.playerCanJump || 
            return;
        }
        else if (attack.hijackControls && attack.target != null) {
            attack.AttackWithTarget();
            return;
        }
        else if (special.hijackControls) {
            special.SpecialFixedUpdate();
            return;
        }

        //anim.relativeVelo = relativeVelo;
        float currentX = Vector3.Dot(relativeVelo, xAx);
        float currentZ = Vector3.Dot(relativeVelo, zAx);
        if (state == PlayerStates.ROLL) {
            velocity += roll.RollingSpeedVector3(xAx, currentX, zAx, currentZ);
            return;
        }
        else if (state == PlayerStates.ATTACK) {
            velocity += attack.AttackWithoutTargetVector3(xAx, currentX, zAx, currentZ);
            return;
        }
        else if (state == PlayerStates.BASEBALL || state == PlayerStates.SAMURAI) {
            velocity += special.BaseballSamuraiMovement(xAx, currentX, zAx, currentZ);
            return;
        }

        float acceleration;
        if (PlayerGrounded) {
            if (run.running) {
                acceleration = runAcceleration;
            }
            else {
                acceleration = walkAcceleration;
            }
        }
        else {
            acceleration = maxJumpAcceleration;
        }
        acceleration *= accelerationMod;
        float maxSpeedChange = acceleration * Time.deltaTime;
        Vector2 currentVelo = new Vector2(currentX, currentZ);
        Vector2 desiredVelo = new Vector2(desiredVelocity.x, desiredVelocity.z);
        desiredVelo *= run.RunningMultiplier();
        Vector2 newVelo = Vector2.MoveTowards(currentVelo, desiredVelo, maxSpeedChange);
        velocity += xAx * (newVelo.x - currentX) + zAx * (newVelo.y - currentZ);
    }

    void ClearState() {
        groundContactCount = steepContactCount = 0;
        contactNormal = steepNormal = connectionVelocity = Vector3.zero;
        previousConnectedRb = connectedRb;
        connectedRb = null;
    }

    Vector3 ProjectOnContactPlane(Vector3 v) {
        return v - contactNormal * Vector3.Dot(v, contactNormal);
    }

    void OnCollisionEnter(Collision col) {
        EvaluateCollision(col);
    }

    void OnCollisionStay(Collision col) {
        EvaluateCollision(col);
    }

    void EvaluateCollision(Collision col) {
        contactPoint = col.GetContact(0).point;
        for (int i = 0; i < col.contactCount; i++) {
            if (state == PlayerStates.ROLL) {
                roll.RollingHitEffects(col);
            }
            if (col.collider.CompareTag("Spike")) {
                hurt.Hurt(col.GetContact(i), 2);
            }
            else {
                groundDot = minGroundDotProd;
                break;
            }
        }
        float minDot = GetMinDot(col.gameObject.layer);
        for (int i = 0; i < col.contactCount; i++) {
            Vector3 normal = col.GetContact(i).normal;
            if (normal.y >= minDot) {
                groundContactCount += 1;
                contactNormal += normal;
                connectedRb = col.rigidbody;
                roll.canRoll = true;
                special.canSlash = true;
                jump.airJumpUsed = false;
                jump.lateJumpTimer = jump.maxLateJumpTimeSteps;
            }
            else if (normal.y > -0.5f) {
                steepContactCount += 1;
                steepNormal += normal;
                contactTag = col.GetContact(i).otherCollider.tag;
                if (groundContactCount == 0) {
                    connectedRb = col.rigidbody;
                }
            }
        }
    }

    /*
    bool CheckSteepContacts() {
        if (steepContactCount > 1) {
            steepNormal.Normalize();
            if (steepNormal.y >= minGroundDotProd) {
                groundContactCount = 1;
                contactNormal = steepNormal;
                return true;
            }
        }
        return false;
    }*/

    void UpdateConnectionState() {
        if (connectedRb == previousConnectedRb) {
            Vector3 connectionMovement =
                connectedRb.transform.TransformPoint(connectionLocalPos) -
                connectionWorldPos;
            connectionVelocity = connectionMovement / Time.deltaTime;
        }
        connectionWorldPos = rb.position;
        connectionLocalPos = connectedRb.transform.InverseTransformPoint(connectionWorldPos);
    }

    bool SnapToGround() {
        if (state == PlayerStates.NOT_IN_CONTROL) {
            return false;
        }
        if (stepsSinceLastGrounded > 1 || !jump.playerCanJump) {
            return false;
        }
        float speed = velocity.magnitude;
        if (speed > maxSnapSpeed) {
            return false;
        }
        if (!Physics.Raycast(
                rb.position, Vector3.down, out RaycastHit hit,
                probeDistance, probeMask
            )) {
            return false;
        }
        if (hit.normal.y < GetMinDot(hit.collider.gameObject.layer)) {
            return false;
        }

        groundContactCount = 1;
        contactNormal = hit.normal;
        float dot = Vector3.Dot(velocity, hit.normal);
        if (dot > 0f) {
            velocity = (velocity - hit.normal * dot).normalized * speed;
        }
        connectedRb = hit.rigidbody;
        return true;
    }

    void CalculateGroundAngleDotProducts() {
        minGroundDotProd = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        minStairsDotProd = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
        minSlipperyDotProd = Mathf.Cos(maxSlipperyAngle * Mathf.Deg2Rad);
        minMediumSlipDotProd = Mathf.Cos(maxMediumSlipAngle * Mathf.Deg2Rad);
    }

    public void SetAccelerationMod(float value) {
        if (accelModCoroutine != null) {
            StopCoroutine(accelModCoroutine);
        }
        accelerationMod = value;
    }

    public void InitAccelerationModReturn(float returnTime, bool returnOnGrounded) {
        if (accelModCoroutine != null) {
            StopCoroutine(accelModCoroutine);
        }
        accelModCoroutine = StartCoroutine(AccelerationMod(returnTime, returnOnGrounded));
    }

    IEnumerator AccelerationMod(float returnTime, bool returnOnGrounded) {
        float t = 0;
        while (t < returnTime) {
            t += Time.deltaTime;
            var perc = t / returnTime;
            perc *= perc;
            if (returnOnGrounded && PlayerGrounded) {
                t = returnTime;
            }
            accelerationMod = Mathf.Lerp(0, 1, perc);
            yield return null;
        }
        accelerationMod = 1;
    }

    float GetMinDot(int layer) {
        return (stairsMask & (1 << layer)) == 0 ?
            groundDot : minStairsDotProd;
    }
    public float[] GetMaxGroundAngles() {
        float[] angles = { maxGroundAngle, maxStairsAngle, maxSlipperyAngle, maxMediumSlipAngle };
        return angles;
    }
    public bool getGrounded() {
        return PlayerGrounded;
    }
    public void SetVelocity(Vector3 velo){
        velocity = velo;
    }
    public Vector3 GetVelocity() {
        return velocity;
    }
    public Vector3 GetRelativeVelo() {
        return relativeVelo;
    }
    public Vector3 GetInput() {
        return inputVector;
    }

    public void ResetPlayer() {
        groundContactCount = 1;
        roll.StopRoll();
        attack.StopAttacks();
        if (state == PlayerStates.CLIMB) {
            climb.StopClimb();
        }
        special.ResetSpecial();
        grapple.InterruptGrapple();
        tongue.InterruptTonguePull();
        Singleton.instance.CameraChanger.ToggleLongJumpCamera(false);
        if (colChanger.currentCol == colTypes.SMALL && !colChanger.TryToStandUp()) {
            crawl.InitCrawlOnStuckUnder();
            return;
        }
        else {
            colChanger.ChangeToStandUpColliders();
        }
        //state = PlayerStates.NORMAL;
    }

    void OnXAxis(InputValue value) {
        xAxis = value.Get<float>();
    }

    void OnYAxis(InputValue value) {
        yAxis = value.Get<float>();
    }
}
