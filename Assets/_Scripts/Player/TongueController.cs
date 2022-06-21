using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TongueController : MonoBehaviour {

    public Transform tongueTip = null;
    public Transform mouth = null;

    [SerializeField] private Transform tongueCylinder = null;
    [SerializeField] private float tongueThickness = 1;
    [SerializeField] private float tongueWidth = 1;
    [SerializeField] private float lerpSpeedOut = 9;
    [SerializeField] private float carryablePullSpeed = 5;
    [SerializeField] private float tongueCooldown = 2;

    private Enemy_TongueInteraction enemyScript;
    private PlayerController control;
    private PlayerRotate rotate;
    private PlayerCarrying carry;
    private PlayerGrapple grapple;
    private Transform model;
    private Collider targetCol;
    private Rigidbody rb;
    private Animator anim;
    private Transform lockedTarget;
    private Transform target = null;
    public Transform GetTarget() {
        return target;
    }
    public void SetTarget(Transform target) {
        this.target = target;
    }
    private Vector3 startPos;
    private bool canTongue = true;
    private float t, perc, tonguePullSpeed;


    void Start() {
        control = GetComponent<PlayerController>();
        rotate = GetComponentInChildren<PlayerRotate>();
        carry = GetComponent<PlayerCarrying>();
        grapple = GetComponent<PlayerGrapple>();
        model = transform.GetChild(0);
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    void OnTongue() {
        InitTonguePull();
    }

    public void InitTonguePull()
    {
        if (canTongue && target != null && control.state == PlayerStates.NORMAL)
        {
            if (target.CompareTag("Enemy"))
            {
                enemyScript = target.root.GetComponent<Enemy_TongueInteraction>();
                if (enemyScript.TongueInteract(transform))
                {
                    lockedTarget = enemyScript.tongueTargetTransform;
                    this.tonguePullSpeed = enemyScript.tonguePullSpeed;
                    StartPull();
                }
                else enemyScript = null;
            }
            else if (target.CompareTag("Grapple_stop") || target.CompareTag("Grapple_swing"))
            {
                StartCoroutine(TongueCooldown());
                grapple.InitGrapple(target);
            }
            else if (target.CompareTag("Carryable"))
            {
                lockedTarget = target;
                tonguePullSpeed = carryablePullSpeed;
                targetCol = target.GetComponent<Collider>();
                targetCol.enabled = false;
                StartPull();
            }
        }
    }

    void StartPull() {
        control.state = PlayerStates.TONGUE_PULL;
        model.rotation = Quaternion.LookRotation(new Vector3(
            lockedTarget.position.x - transform.position.x,
            0,
            lockedTarget.position.z - transform.position.z).normalized);
        rotate.InitRotateSpdModReturn(1f);
        if (control.PlayerGrounded) {
            control.SetVelocity(rb.velocity * 0.25f);
            rb.velocity *= 0.25f;
        }
        anim.Play("tonguePull", 0, 0);
        control.SetAccelerationMod(0);
        control.InitAccelerationModReturn(1f, false);
        StartCoroutine(TongueCooldown());
        StartCoroutine(LerpTongue());
    }

    IEnumerator LerpTongue() {
        t = perc = 0;
        ToggleTongueGraphicsOn();

        // Shoot tongue out
        while (t < 1) {
            t += lerpSpeedOut * Time.deltaTime;
            tongueTip.position = Vector3.Lerp(mouth.position, lockedTarget.position, t);
            UpdateTongueCylinderGraphics();
            yield return null;
        }
        startPos = lockedTarget.position;

        // Pull tongue in
        while (t >= 1 && t < 2) {
            t += tonguePullSpeed * Time.deltaTime;
            if (lockedTarget.CompareTag("Carryable")) {
                tongueTip.position = LerpCarryablePull();
            }
            else if (lockedTarget.CompareTag("Enemy")) {
                tongueTip.position = enemyScript.LerpEnemyPull(t - 1);
            }
            UpdateTongueCylinderGraphics();
            yield return null;
        }

        // End cleanup
        ToggleTongueGraphicsOff();
        if (lockedTarget.CompareTag("Enemy")) {
            enemyScript.EndEnemyPull();
            enemyScript = null;
            lockedTarget = null;
            control.state = PlayerStates.NORMAL;
        }
        else if (lockedTarget.CompareTag("Carryable")) {
            EndTonguePullCarryable();
        }
    }

    Vector3 LerpCarryablePull() {
        perc = (t - 1) * (t - 1);
        lockedTarget.position = Vector3.Lerp(startPos, mouth.position + model.forward, perc);
        return lockedTarget.position;
    }

    void EndTonguePullCarryable() {
        targetCol.enabled = true;
        if (lockedTarget.CompareTag("Carryable")) {
            carry.PickUpObj(lockedTarget);
            lockedTarget = null;
        }
        else {
            lockedTarget = null;
            control.state = PlayerStates.NORMAL;
        }
    }

    public void InterruptTonguePull() {
        StopAllCoroutines();
        if (enemyScript != null) {
            enemyScript.EndEnemyPull();
            enemyScript = null;
        }
        if (targetCol != null) {
            targetCol.enabled = true;
        }
        ToggleTongueGraphicsOff();
        lockedTarget = null;
    }

    IEnumerator TongueCooldown() {
        canTongue = false;
        yield return Helpers.GetWait(tongueCooldown);
        canTongue = true;
    }

    // Tongue graphics
    public void UpdateTongueCylinderGraphics() {
        tongueCylinder.position = Vector3.Lerp(mouth.position, tongueTip.position, 0.5f);
        tongueCylinder.LookAt(tongueTip.position);
        tongueCylinder.transform.GetChild(0).localScale = new Vector3(
            tongueWidth, (mouth.position - tongueTip.position).magnitude * 0.5f, tongueThickness);
    }

    public void ToggleTongueGraphicsOn() {
        tongueTip.position = mouth.position;
        tongueTip.gameObject.SetActive(true);
        UpdateTongueCylinderGraphics();
        tongueCylinder.gameObject.SetActive(true);
    }

    public void ToggleTongueGraphicsOff() {
        tongueTip.gameObject.SetActive(false);
        tongueCylinder.gameObject.SetActive(false);
    }
}
