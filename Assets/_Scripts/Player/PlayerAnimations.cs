using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour {

    private PlayerController control;
    private PlayerRunning run;
    private PlayerJumping jump;
    private Animator anim;

    //private AnimatorClipInfo[] currentClipInfo;

    void Start() {
        control = GetComponentInParent<PlayerController>();
        run = GetComponentInParent<PlayerRunning>();
        jump = GetComponentInParent<PlayerJumping>();
        anim = GetComponent<Animator>();
    }

    public void AnimateNormalState() {
        SlidingOnWallsAnimations();

        if ((control.state == PlayerStates.NORMAL || control.state == PlayerStates.CARRYING) && 
            jump.playerCanJump)
        {
            if (control.PlayerGrounded || control.stepsSinceLastGrounded < 5) {
                FadeToAnimation("idleWalkRun", 0.05f, 0);
                float velo = control.GetRelativeVelo().magnitude;
                float walkAnimBlend = 0;
                if (control.GetInput().sqrMagnitude >= control.deadzoneSquared) {
                    if (velo > control.walkSpd) {
                        walkAnimBlend = 1 + (velo - control.walkSpd) / (control.runSpd - control.walkSpd);
                    }
                    else {
                        walkAnimBlend = velo / control.walkSpd;
                    }
                }
                else {
                    if (walkAnimBlend > 0) {
                        walkAnimBlend -= Time.deltaTime * 10;
                        if (walkAnimBlend < 0) {
                            walkAnimBlend = 0;
                        }
                    }
                }
                anim.SetFloat("Velo", walkAnimBlend);
            }
            else if (!control.PlayerGrounded && !jump.airJumpUsed) {
                if (!anim.GetCurrentAnimatorStateInfo(0).IsName("jump")) {
                    anim.Play("jump", 0, 0.5f);
                }
            }
        }
    }

    public void FadeToAnimation(string name, float fadeDuration, int animLayer, float offset = 0) {
        if (anim.GetCurrentAnimatorStateInfo(animLayer).IsName(name) == false) {
            if (anim.GetAnimatorTransitionInfo(animLayer).anyState == false) {
                anim.CrossFade(name, fadeDuration, animLayer, offset);
            }
        }
    }

    float wallPerc = 0;
    public void SlidingOnWallsAnimations()
    {
        if (control.PlayerOnSteep && !control.PlayerGrounded)
        {
            wallPerc = Mathf.Min(wallPerc += Time.deltaTime * 3, 0.8f);
            float dirCheck = CheckIfLeftOrRight(transform.forward, control.steepNormal, transform.up);
            float dot = Vector3.Dot(control.steepNormal.normalized, transform.forward);
            float x = -dirCheck * (1 - Mathf.Abs(dot));
            float z = -dot;
            anim.SetFloat("wallX", x);
            anim.SetFloat("wallZ", z);
            print("x: " + x + ", z: " + z);
        }
        else
            wallPerc = Mathf.Max(wallPerc -= Time.deltaTime * 5, 0);

        anim.SetLayerWeight(2, wallPerc);
    }

    float CheckIfLeftOrRight(Vector3 fwd, Vector3 targetDir, Vector3 up)
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);
        // Right
        if (dir > 0f)
        {
            return 1f;
        }
        // Left
        else if (dir < 0f)
        {
            return -1f;
        }
        // Parallel
        else
        {
            return 0f;
        }
    }
}
