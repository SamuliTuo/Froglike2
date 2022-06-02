using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeClimbAnimHook : MonoBehaviour {

    public float w_rh;
    public float w_lh;
    public float w_rf;
    public float w_lf;
    public float wallOffset = 0.2f;
    public float lerpSpeed = 20;
    public float currentLerpSpeed = 20;

    private Animator anim;
    private IKSnapshot ikBase;
    private IKSnapshot current = new IKSnapshot();
    private IKSnapshot next = new IKSnapshot();    //redundant??
    private IKGoals goals = new IKGoals();
    private Vector3 rh, lh, rf, lf;
    private Vector3 prevMoveDir;
    private Transform helper;
    private bool isMirror;
    private bool isLeft;
    private float delta;

    public void Init(PlayerClimbing climb, Transform helperTrans) {
        anim = climb.anim;
        ikBase = climb.baseIKsnapshot;
        helper = helperTrans;
    }

    public void CreatePosition(Vector3 origin, Vector3 moveDir, bool isMid) {
        delta = Time.deltaTime;
        Vector3 relativeDir = transform.up;
        relativeDir = transform.InverseTransformDirection(moveDir);
        HandleAnim(relativeDir, isMid);
        if (isMid) {
            UpdateGoals(relativeDir);
            prevMoveDir = relativeDir;
        }
        else {
            UpdateGoals(prevMoveDir);
        }
        IKSnapshot ik = CreateSnapshot(origin);
        CopySnapshot(ref current, ik);
        SetIKPosition(isMid, goals.lf, current.lf, AvatarIKGoal.LeftFoot);
        SetIKPosition(isMid, goals.rf, current.rf, AvatarIKGoal.RightFoot);
        SetIKPosition(isMid, goals.lh, current.lh, AvatarIKGoal.LeftHand);
        SetIKPosition(isMid, goals.rh, current.rh, AvatarIKGoal.RightHand);
        UpdateIKWeight(AvatarIKGoal.LeftFoot, 1);
        UpdateIKWeight(AvatarIKGoal.RightFoot, 1);
        UpdateIKWeight(AvatarIKGoal.LeftHand, 1);
        UpdateIKWeight(AvatarIKGoal.RightHand, 1);
    }
    void UpdateGoals(Vector3 moveDir) {
        isLeft = (moveDir.x <= 0);
        if (moveDir.x > 0.1f && moveDir.x < -0.1f) {
            goals.rh = isLeft;
            goals.lh = !isLeft;
            goals.rf = !isLeft;
            goals.lf = isLeft;
        }
        else {
            bool isEnabled = isMirror;
            if (moveDir.y < 0) {
                isEnabled = !isEnabled;
            }
            goals.rh = !isEnabled;
            goals.lh = isEnabled;
            goals.rf = isEnabled;
            goals.lf = !isEnabled;
        }
    }
    void HandleAnim(Vector3 moveDir, bool isMid) {
        if (isMid) {
            if (moveDir.y < -0.3f || moveDir.y > 0.3f) {
                if (moveDir.x > -0.25f && moveDir.x < 0.25f) {
                    isMirror = !isMirror;
                    anim.SetBool("mirror", isMirror);
                }
                else {
                    if (moveDir.y < 0) {
                        isMirror = (moveDir.x > 0);
                        anim.SetBool("mirror", isMirror);
                    }
                    else {
                        isMirror = (moveDir.x < 0);
                        anim.SetBool("mirror", isMirror);
                    }
                }
                anim.CrossFade("climb_up", 4f);
            }
        }
        else anim.CrossFade("climb_idle", 4f);
    }
    public IKSnapshot CreateSnapshot(Vector3 origin) {
        IKSnapshot r = new IKSnapshot();
        Vector3 _lh = LocalToWorld(ikBase.lh);
        r.lh = GetPosActual(_lh, AvatarIKGoal.LeftHand);
        Vector3 _rh = LocalToWorld(ikBase.rh);
        r.rh = GetPosActual(_rh, AvatarIKGoal.RightHand);
        Vector3 _lf = LocalToWorld(ikBase.lf);
        r.lf = GetPosActual(_lf, AvatarIKGoal.LeftFoot);
        Vector3 _rf = LocalToWorld(ikBase.rf);
        r.rf = GetPosActual(_rf, AvatarIKGoal.RightFoot);
        return r;
    }

    Vector3 GetPosActual(Vector3 ori, AvatarIKGoal goal) {
        Vector3 r = ori;
        Vector3 origin = ori;
        Vector3 dir = helper.forward;
        origin += -(dir * 0.2f);

        int layerMask = (1 << 3) | (1 << 4) | (1 << 6);
        layerMask = ~layerMask;
        RaycastHit hit;
        bool isHit = false;
        if (Physics.Raycast(origin, dir, out hit, 1.5f, layerMask)) {
            Vector3 _r = hit.point + (hit.normal * wallOffset);
            r = _r;
            isHit = true;
        }
        if (!isHit) {
            switch (goal) {
                case AvatarIKGoal.LeftFoot:
                    r = LocalToWorld(ikBase.lf);
                    break;
                case AvatarIKGoal.RightFoot:
                    r = LocalToWorld(ikBase.rf);
                    break;
                case AvatarIKGoal.LeftHand:
                    r = LocalToWorld(ikBase.lh);
                    break;
                case AvatarIKGoal.RightHand:
                    r = LocalToWorld(ikBase.rh);
                    break;
                default:
                    break;
            }
        }
        return r;
    } 

    Vector3 LocalToWorld(Vector3 targetPos) {
        Vector3 r = helper.position;
        r += helper.right * targetPos.x;
        r += helper.forward * targetPos.z;
        r += helper.up * targetPos.y;
        return r;
    }
    public void CopySnapshot(ref IKSnapshot to, IKSnapshot from) {
        to.rh = from.rh;
        to.lh = from.lh;
        to.rf = from.rf;
        to.lf = from.lf;
    }
    void SetIKPosition(bool isMid, bool isTrue, Vector3 pos, AvatarIKGoal goal) {
        if (isMid) {
            if (isTrue) {
                Vector3 p = GetPosActual(pos, goal);
                UpdateIKPosition(goal, p);
            }
        }
        else {
            if (!isTrue) {
                Vector3 p = GetPosActual(pos, goal);
                UpdateIKPosition(goal, p);
            }
        }
    }
    public void UpdateIKPosition(AvatarIKGoal goal, Vector3 pos) {
        switch (goal) {
            case AvatarIKGoal.LeftFoot:
                lf = pos;
                break;
            case AvatarIKGoal.RightFoot:
                rf = pos;
                break;
            case AvatarIKGoal.LeftHand:
                lh = pos;
                break;
            case AvatarIKGoal.RightHand:
                rh = pos;
                break;
            default:
                break;
        }
    }
    public void UpdateIKWeight(AvatarIKGoal goal, float w) {
        switch (goal) {
            case AvatarIKGoal.LeftFoot:
                w_lf = w;
                break;
            case AvatarIKGoal.RightFoot:
                w_rf = w;
                break;
            case AvatarIKGoal.LeftHand:
                w_lh = w;
                break;
            case AvatarIKGoal.RightHand:
                w_rh = w;
                break;
            default:
                break;
        }
    }
    void OnAnimatorIK() {
        delta = Time.deltaTime;
        SetIKPos(AvatarIKGoal.LeftHand, lh, w_lh);
        SetIKPos(AvatarIKGoal.RightHand, rh, w_rh);
        SetIKPos(AvatarIKGoal.LeftFoot, lf, w_lf);
        SetIKPos(AvatarIKGoal.RightFoot, rf, w_rf);
    }

    public void SetIKPos(AvatarIKGoal goal, Vector3 targetPos, float weight) {
        IKStates ikState = GetIKStates(goal);
        if (ikState == null) {
            ikState = new IKStates();
            ikState.goal = goal;
            ikStates.Add(ikState);
        }
        if (weight == 0) {
            ikState.isSet = false;
        }
        if (!ikState.isSet) {
            ikState.position = GoalToBodyBones(goal).position;
            ikState.isSet = true;
        }
        ikState.positionWeight = weight;
        ikState.position = Vector3.Lerp(ikState.position, targetPos, delta * currentLerpSpeed);

        anim.SetIKPositionWeight(goal, ikState.positionWeight);
        anim.SetIKPosition(goal, ikState.position); 
    }

    Transform GoalToBodyBones(AvatarIKGoal goal) {
        switch (goal) {
            case AvatarIKGoal.LeftFoot:
                return anim.GetBoneTransform(HumanBodyBones.LeftFoot);
            case AvatarIKGoal.RightFoot:
                return anim.GetBoneTransform(HumanBodyBones.RightFoot);
            case AvatarIKGoal.LeftHand:
                return anim.GetBoneTransform(HumanBodyBones.LeftHand);
            default:
            case AvatarIKGoal.RightHand:
                return anim.GetBoneTransform(HumanBodyBones.RightHand);

        }
    }
    IKStates GetIKStates(AvatarIKGoal goal) {
        IKStates r = null;
        foreach (IKStates i in ikStates) {
            if (i.goal == goal) {
                r = i;
                break;
            }
        }
        return r;
    }
    List<IKStates> ikStates = new List<IKStates>();
    class IKStates {
        public AvatarIKGoal goal;
        public Vector3 position;
        public float positionWeight;
        public bool isSet = false;
    }
}

public class IKGoals {
    public bool rh;
    public bool lh;
    public bool lf;
    public bool rf;
}