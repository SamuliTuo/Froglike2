using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
//using UnityEditor.Animations;

public class PlayerClimbing : MonoBehaviour {

    public float positionOffset = 0.5f;
    public float offsetFromWall = 0.45f;
    public float speedMultiplier = 0.2f;
    public float climbSpeed = 3;
    public float fastClimbSpeed = 5;
    public float rotateSpeed = 3;
    public float rayTowardsMoveDir = 0.5f;
    public float rayForwardToWallDist = 1;
    public float horizontal;
    public float vertical;
    public bool isMid;
    public bool isClimbing;
    public IKSnapshot baseIKsnapshot;
    public FreeClimbAnimHook anim_hook;
    public Animator anim;

    [SerializeField] private Avatar genericAvatar = null, humanoidAvatar = null;
    [SerializeField] float groundDetachAngle = 40;
    [SerializeField] float wallJumpUpwardF = 30;
    [SerializeField] float wallJumpDirectionalF = 40;

    //InputManager input;
    //PlayerState _states;
    private PlayerController control;
    private PlayerJumping jump;
    private PlayerRunning run;
    //PlayerStamina _stamina;
    //PlayerDashing _dash;
    //PlayerAnimations_02 _anim;
    private Rigidbody rb;
    private Transform helper, frogModel;
    private Vector3 startPos;
    private Vector3 targetPos;
    private Vector3 lastHelperUp;
    private Quaternion startRot;
    private Quaternion targetRot;
    private bool jumpPressed = false;
    private bool goingToStand = false;
    private bool inPosition;
    private bool isLerping;
    private bool detachNeutralQueued = false;
    private bool detachingStarted = false;
    private float currentClimbSpeed;
    private float delta;
    private float t;
    

    
    void Start() {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        //input = GetComponentInParent<InputManager>();
        //_states = GetComponent<PlayerState>();
        control = GetComponent<PlayerController>();
        jump = GetComponent<PlayerJumping>();
        run = GetComponent<PlayerRunning>();
        //_stamina = transform.parent.GetComponentInChildren<PlayerStamina>();
        //_dash = GetComponent<PlayerDashing>();
        //anim = transform.parent.GetComponentInChildren<PlayerAnimations_02>();
        frogModel = transform.GetChild(0);
        currentClimbSpeed = climbSpeed;
        Init();
    }

    public void Init() {
        helper = new GameObject().transform;
        helper.name = "climb helper";
        anim_hook.Init(this, helper);
    }

    public void GrabWall(Vector3 inputDir) {
        if (!(control.state == PlayerStates.NORMAL || control.state == PlayerStates.LONG_JUMP)) {
            return;
        }
        if (control.PlayerOnSteep && !control.PlayerGrounded) {
            int layerMask = (1 << 3) | (1 << 4) | (1 << 6);
            layerMask = ~layerMask;
            RaycastHit hit;
            Vector3 startPosition = transform.position;
            Vector3 step = Vector3.up * 0.2f;
            Vector3 rayDir = inputDir.normalized * 0.3f;
            float rayLength = 0.8f;
            for (int i = 0; i < 4; i++) {
                Debug.DrawRay(startPosition - step * i + rayDir, frogModel.forward * rayLength);
                if (Physics.Raycast(startPosition - step * i + rayDir, inputDir, out hit, rayLength, layerMask)) {
                    if (!hit.collider.CompareTag("Climbable")) {
                        return;
                    }
                    control.SetVelocity(Vector3.zero);
                    t = 0;
                    //FrogAudioPlayer.current.ClimbStartSFX();
                    InitForClimb(hit, transform.position);
                    return;
                }
            }
        }
    }

    float CompareVectorToWallAngle(Vector3 inputDir) {
        Vector2 vec1 = new Vector2(inputDir.x, inputDir.z);
        Vector2 vec2 = new Vector2(control.steepNormal.x, control.steepNormal.z);
        Debug.Log("angle" + Vector2.Angle(vec1, vec2) + ",  input: " + inputDir.magnitude);
        return Vector2.Angle(vec1, vec2);
    }

    public void InitForClimb(RaycastHit hit, Vector3 origin) {
        anim_hook.currentLerpSpeed = 10000f;

        // nää on vielä väärin!
        /*
        Vector3 rhPos = new Vector3(0.45f, 0.2f, 0.1f);
        Vector3 lhPos = new Vector3(-0.45f, 0.2f, 0.1f);
        Vector3 lfPos = new Vector3(-0.3f, -0.9f, 0.1f);
        Vector3 rfPos = new Vector3(0.3f, -0.9f, 0.1f);

        Vector3 rhPos = baseIKsnapshot.rh;
        Vector3 lhPos = baseIKsnapshot.lh;
        Vector3 lfPos = baseIKsnapshot.lf;
        Vector3 rfPos = baseIKsnapshot.rf;

        anim_hook.UpdateIKPosition(AvatarIKGoal.RightHand, hit.point + rhPos);
        anim_hook.UpdateIKPosition(AvatarIKGoal.LeftHand, hit.point + lhPos);
        anim_hook.UpdateIKPosition(AvatarIKGoal.LeftFoot, hit.point + lfPos);
        anim_hook.UpdateIKPosition(AvatarIKGoal.RightFoot, hit.point + rfPos);
        //
        */
        //_anim.currentWallJumpMoveStiffness = 0;
        
        anim.avatar = humanoidAvatar;
        control.state = PlayerStates.CLIMB;
        goingToStand = false;
        detachingStarted = false;
        origin.y += 0.1f;
        helper.position = PosWithOffset(origin, hit.point);
        helper.transform.rotation = Quaternion.LookRotation(-hit.normal);
        rb.velocity = Vector3.zero;
        control.SetVelocity(Vector3.zero);
        rb.isKinematic = true;
        startPos = transform.position;
        targetPos = hit.point + (hit.normal * offsetFromWall);
        frogModel.localEulerAngles = Vector3.zero;
        transform.rotation = Quaternion.LookRotation(-hit.normal, Vector3.up);
        t = 0;
        inPosition = false;
        isMid = true;
        anim.Play("climb_idle", 0);
        anim_hook.CreatePosition(targetPos, Vector3.zero, false);
        anim_hook.currentLerpSpeed = anim_hook.lerpSpeed;
    }

    public void ClimbUpdate() {
        delta = Time.deltaTime;
        if (goingToStand) {
            if (!detachingStarted) {
                anim.avatar = genericAvatar;
                frogModel.localPosition = new Vector3(0, -1, 0);
                //FrogAudioPlayer.current.ClimbUpSFX();
                anim.Play("climbToStand", 0, 0);
                DisableIKWeighs();
                detachingStarted = true;
            }
            DetachToGround(targetPos);
            return;
        }
        Tick(delta);
        //jumpPressed |= input.A_Button(false);
        //detachNeutralQueued |= input.Y_Button(false);
    }
    
    public void ClimbFixed() {
        if (jumpPressed) {
            DetachToJump();
        }
        if (detachNeutralQueued) {
            DetachNeutral();
        }
    }

    public void Tick(float delta) {
        if (!inPosition) {
            inPosition = true;
            isMid = false;
            //GetInPosition();
            return;
        }
        if (!isLerping) {
            horizontal = control.xAxis;//.JoystickHorizontal();
            vertical = control.yAxis;// input.JoystickVertical();
            //float moveAmount = Mathf.Abs(horizontal) + Mathf.Abs(vertical);     //redundant?!?!?!
            Vector3 h = helper.right * horizontal;
            Vector3 v = helper.up * vertical;
            Vector3 moveDir = (h + v).normalized;
            if (isMid) {
                if (moveDir == Vector3.zero)
                    return;
                else { }
                    //FrogAudioPlayer.current.ClimbPlopSFX();
            }
            else {
                bool canMove = CanMove(moveDir);
                if (goingToStand) {
                    return;
                }
                if (!canMove || moveDir == Vector3.zero) {
                    return;
                }
            }

            isMid = !isMid;
            t = 0;
            isLerping = true;
            startPos = transform.position;
            Vector3 targetDir = helper.position - startPos;
            float dist = Vector3.Distance(helper.position, startPos) * 0.5f;
            targetDir *= dist;
            targetDir += transform.position;
            targetPos = (isMid) ? targetDir : helper.position;

            anim_hook.CreatePosition(targetPos, moveDir, isMid);
        }
        else {
            t += delta * currentClimbSpeed;
            if (t > 1) {
                t = 1;
                isLerping = false;
            }
            Vector3 climbPos = Vector3.Lerp(startPos, targetPos, t);
            transform.position = climbPos;// - transform.up;
            transform.rotation = Quaternion.Slerp(
                transform.rotation, helper.rotation, delta * rotateSpeed);
            //frogModel.rotation = Quaternion.Slerp(frogModel.rotation, helper.rotation, delta * rotateSpeed);
        }
    }

    bool CanMove(Vector3 moveDir) {
        Vector3 origin = transform.position;
        float dist = rayTowardsMoveDir;
        Vector3 dir = moveDir;
        int layerMask = (1 << 3) | (1 << 4) | (1 << 6);
        layerMask = ~layerMask;
        RaycastHit hit;
        //Raycast towards the direction of the input.
        Debug.DrawLine(origin, origin + (dir * dist), Color.red, 0.2f);
        //DebugLine.singleton.SetLine(origin, origin + (dir * dist), 0);
        if (Physics.Raycast(origin, dir, out hit, dist, layerMask)) {
            CheckGroundAngleForDetach(hit, layerMask);
            if (!hit.collider.CompareTag("Climbable")) {
                return false;
            }
            //Check if we encounter an inwards-corner.
            helper.position = PosWithOffset(origin, hit.point);
            helper.rotation = Quaternion.LookRotation(-hit.normal);
            return true;
        }
        origin += dir * dist;
        dir = helper.forward;
        float dist2 = rayForwardToWallDist;
        //Raycast from the "climb direction"-ray's endpoint, towards the wall-normal.
        Debug.DrawLine(origin, origin + (dir * dist2), Color.red, 0.2f);
        //DebugLine.singleton.SetLine(origin, origin + (dir * dist2), 1);
        if (Physics.Raycast(origin, dir, out hit, dist2, layerMask)) {
            CheckGroundAngleForDetach(hit, layerMask);
            if (!hit.collider.CompareTag("Climbable")) {
                return false;
            }
            helper.position = PosWithOffset(origin, hit.point);
            helper.rotation = Quaternion.LookRotation(-hit.normal);
            return true;
        }
        origin += dir * dist2;
        dir = -moveDir;
        //Raycast around a corner
        Debug.DrawLine(origin, origin + dir, Color.red, 0.2f);
        //DebugLine.singleton.SetLine(origin, origin + dir, 1);
        if (Physics.Raycast(origin, dir, out hit, rayTowardsMoveDir * 1.5f, layerMask)) {
            CheckGroundAngleForDetach(hit, layerMask);
            if (!hit.collider.CompareTag("Climbable")) {
                return false;
            }
            helper.position = PosWithOffset(origin, hit.point);
            helper.rotation = Quaternion.LookRotation(-hit.normal);
            return true;
        }
        origin += dir * dist2;
        dir = -Vector3.up;
        if (Physics.Raycast(origin, dir, out hit, dist2, layerMask)) {
            if (!hit.collider.CompareTag("Climbable")) {
                return false;
            }
            float angle = Vector3.Angle(-helper.forward, hit.normal);
            if (angle < 40) {
                helper.position = PosWithOffset(origin, hit.point);
                helper.rotation = Quaternion.LookRotation(-hit.normal);
                CheckGroundAngleForDetach(hit, layerMask);
                return true;
            }
        }
        return false;
    }

    void GetInPosition() {
        //Vector3 tPos = Vector3.Lerp(startPos, targetPos - helper.up, t);
        transform.position = targetPos;// - helper.up;
        transform.rotation = Quaternion.Slerp(transform.rotation, helper.rotation, delta * 10000);
        //anim_hook.CreatePosition(targetPos, Vector3.zero, false);
        //inPosition = true;
        if (t > 1) {
            t = 1;
            inPosition = true;
            anim_hook.CreatePosition(targetPos, Vector3.zero, false);
            anim_hook.currentLerpSpeed = anim_hook.lerpSpeed;
        }
        t += 1;
        
    /*
        t += delta;
        if (t > 1) {
            t = 1;
            inPosition = true;
            anim_hook.CreatePosition(targetPos, Vector3.zero, false);
        }
        Vector3 tPos = Vector3.Lerp(startPos, targetPos, t);
        transform.position = tPos;
        transform.rotation = Quaternion.Slerp(transform.rotation, helper.rotation, delta * rotateSpeed);
      */  
    }

    Vector3 PosWithOffset(Vector3 origin, Vector3 target) {
        Vector3 direction = origin - target;
        direction.Normalize();
        Vector3 offset = direction * offsetFromWall;
        return target + offset;
    }

    void DetachToJump() {
        anim.avatar = genericAvatar;
        rb.isKinematic = false;
        jumpPressed = false;
        rb.AddForce(Vector3.up * wallJumpUpwardF -
                    new Vector3(helper.forward.x, 0, helper.forward.z) * wallJumpDirectionalF,
                    ForceMode.Impulse
        );
        transform.rotation = Quaternion.LookRotation(
            new Vector3(-helper.forward.x, 0, -helper.forward.z).normalized, Vector3.up
        );
        //FrogAudioPlayer.current.PlayJumpSFX();
        anim.Play("wallJump", 0);
        DisableIKWeighs();
        jump.airJumpUsed = false;//.ResetAirdash();
        control.state = PlayerStates.NORMAL;
    }

    void DetachNeutral() {
        anim.avatar = genericAvatar;
        rb.isKinematic = false;
        detachNeutralQueued = false;
        rb.AddForce(Vector3.up - helper.forward * 3, ForceMode.Impulse);
        transform.rotation = Quaternion.LookRotation(
            new Vector3(helper.forward.x, 0, helper.forward.z).normalized, Vector3.up);
        //FrogAudioPlayer.current.ClimbStartSFX();
        anim.Play("Jump", 0, 0.4f);
        DisableIKWeighs();
        //_dash.ResetAirdash();
        control.state = PlayerStates.NORMAL;
    }

    void CheckGroundAngleForDetach(RaycastHit hit, LayerMask mask) {
        float hitAngle = Vector3.Angle(Vector3.up, hit.normal);
        if (hitAngle < groundDetachAngle) {
            if (Physics.Raycast(hit.point, Vector3.up, 2.1f, mask)) {
                return;
            }
            t = 0;
            startPos = transform.position;
            targetPos = hit.point;
            startRot = transform.rotation;
            targetRot = Quaternion.LookRotation(new Vector3(
                helper.forward.x, 0, helper.forward.z).normalized, Vector3.up);
            goingToStand = true;
        }
    }

    void DetachToGround(Vector3 targetPoint) {
        t += delta * 1.7f;
        if (t >= 1) {
            t = 1;
            rb.isKinematic = false;
            DisableIKWeighs();
            control.state = PlayerStates.NORMAL;
            anim.CrossFade("idleWalkRun", 0.2f);
            //_run.playerRunning = false;
            //_run.runningSpeedMult = 1;
            transform.position = targetPoint + Vector3.up;
            transform.rotation = targetRot;
        }
        float perc = t * t * (3f - 2f * t);
        Vector3 target = targetPoint + Vector3.up;
        Vector3 tPos = Vector3.Lerp(startPos, target, perc);
        Quaternion tRot = Quaternion.Slerp(startRot, targetRot, perc);
        transform.position = tPos;
        transform.rotation = tRot;
    }

    public void StopClimb() {
        frogModel.position = transform.position;
        anim.avatar = genericAvatar;
        rb.isKinematic = false;
        DisableIKWeighs();
    }

    void DisableIKWeighs() {
        anim_hook.UpdateIKWeight(AvatarIKGoal.LeftFoot, 0);
        anim_hook.UpdateIKWeight(AvatarIKGoal.RightFoot, 0);
        anim_hook.UpdateIKWeight(AvatarIKGoal.LeftHand, 0);
        anim_hook.UpdateIKWeight(AvatarIKGoal.RightHand, 0);
    }
}


[System.Serializable]
public class IKSnapshot {
    public Vector3 rh, lh, lf, rf;
}
