using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCrawl : MonoBehaviour {

    [Tooltip ("How often a spherecast upwards to try to stand up should be fired:")]
    [SerializeField] private float standUpTryInterval = 0.25f;

    private PlayerController control;
    private PlayerSpecialAttack special;
    private PlayerColliderChanges colliders;
    private Animator anim;

    void Start() {
        control = GetComponent<PlayerController>();
        special = GetComponent<PlayerSpecialAttack>();
        colliders = GetComponent<PlayerColliderChanges>();
        anim = GetComponentInChildren<Animator>();
    }

    void OnCrouch()
    {
        if (control.state == PlayerStates.NORMAL) {
            if (control.PlayerGrounded) {
                control.state = PlayerStates.CRAWL;
            }
            else if (control.stepsSinceLastGrounded > 3) {
                special.StartGroundPound();
            }
        }
        else if (control.state == PlayerStates.CRAWL) {
            StartCoroutine(GetUp());
        }
    }

    public void CrawlUpdate() {
        if (control.state != PlayerStates.CRAWL) {
            return;
        }
        anim.Play("XX_unused_crouch", 0, 0);
        colliders.ChangeToSmallCollider();
        control.moveSpeedMod = 0.5f;
        control.SetAccelerationMod(0.5f);
    }

    public void InitCrawlOnStuckUnder() {
        colliders.ChangeToSmallCollider();
        control.state = PlayerStates.CRAWL;
        StartCoroutine(GetUp());
    }

    IEnumerator GetUp() {
        if (colliders.TryToStandUp()) {
            control.moveSpeedMod = 1;
            control.SetAccelerationMod(1);
            colliders.ChangeToStandUpColliders();
            control.state = PlayerStates.NORMAL;
            yield break;
        }
        else
        {
            yield return Helpers.GetWait(standUpTryInterval);
            StartCoroutine(GetUp());
        }
    }
}
