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
}
