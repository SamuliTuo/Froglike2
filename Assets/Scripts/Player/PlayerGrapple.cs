using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrapple : MonoBehaviour {

    [SerializeField] private float startSpeed = 3f;
    [SerializeField] private float grappleSpeed = 1f;
    [SerializeField] private float endPointJumpForce = 1f;

    private PlayerController control;
    private PlayerRotate rotate;
    private PlayerJumping jump;
    private TongueController tongue;
    private Rigidbody rb;
    private Transform target, model;
    private Animator anim;
    private Quaternion startRot;
    private Vector3 startPos, endPos, startVelocity, grappleDir, initialGrappleDir;
    private float t, perc, grappleLength;


    void Start() {
        control = GetComponent<PlayerController>();
        rotate = GetComponentInChildren<PlayerRotate>();
        jump = GetComponent<PlayerJumping>();
        tongue = GetComponent<TongueController>();
        rb = GetComponent<Rigidbody>();
        model = transform.GetChild(0);
        anim = GetComponentInChildren<Animator>();
    }

    public void InitGrapple(Transform target) {
        this.target = target;
        control.state = PlayerStates.GRAPPLE;
        control.SetAccelerationMod(0);
        rotate.SetRotateSpdMod(0);
        startPos = transform.position;
        startRot = model.rotation;
        initialGrappleDir = (target.position - startPos).normalized;
        grappleLength = (target.position - startPos).magnitude;
        startVelocity = rb.velocity;
        StartCoroutine(Grapple());
    }

    IEnumerator Grapple() {
        t = -1;
        tongue.ToggleTongueGraphicsOn();
        anim.Play("grapple_start", 0, 0);

        // Slow down and shoot tongue:
        while (t < 0) {
            t += startSpeed * Time.deltaTime;
            if (t < 0.5f) {
                tongue.tongueTip.position = Vector3.Lerp(
                    tongue.mouth.position, target.position, (t + 1) * 2);
            }
            else {
                tongue.tongueTip.position = target.position;
            }
            model.rotation = Quaternion.RotateTowards(
                model.rotation,
                Quaternion.LookRotation(initialGrappleDir, Vector3.up),
                1000 * Time.deltaTime
            );
            tongue.UpdateTongueCylinderGraphics();
            rb.velocity *= 0.8f;
            yield return null;
        }
        startPos = transform.position;
        anim.Play("grapple_grapple", 0, 0);

        // Grapple:
        while (t < 1) {
            t += grappleSpeed * Time.deltaTime;
            perc = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
            endPos = target.position + Vector3.up * 1.1f;
            tongue.tongueTip.position = target.position;
            if (t < 0.85f) {
                grappleDir = (endPos - transform.position).normalized;
            }
            model.rotation = Quaternion.RotateTowards(
                model.rotation,
                Quaternion.LookRotation(grappleDir, Vector3.up),
                1000 * Time.deltaTime
            );
            transform.position = GetPointOnBezierCurve(
                startPos,
                CalculateBlerp_p1_swing(),
                CalculateBlerp_p2_swing(),
                endPos, 
                perc);
            tongue.UpdateTongueCylinderGraphics();
            yield return null;
        }

        // End:
        tongue.ToggleTongueGraphicsOff();
        transform.position = target.position + Vector3.up * 1.1f;
        model.rotation = Quaternion.LookRotation(
            new Vector3(grappleDir.x, 0, grappleDir.z).normalized, Vector3.up);
        if (target.CompareTag("Grapple_stop")) {
            control.SetVelocity(Vector3.zero);
            rb.velocity = Vector3.zero;
            EndGrapple();
        }
        else {
            rb.velocity = grappleDir * endPointJumpForce;
            anim.Play("grapple_endPointJump", 0, 0);
            yield return Helpers.GetWait(0.25f);
            StartCoroutine(jump.JustJumped());
            jump.airJumpUsed = false;
            EndGrapple();
        }
    }

    void EndGrapple() {
        target = null;
        rotate.InitRotateSpdModReturn(0.4f);
        control.InitAccelerationModReturn(0.5f, false);
        control.state = PlayerStates.NORMAL;
    }

    public void InterruptGrapple() {
        StopAllCoroutines();
        tongue.ToggleTongueGraphicsOff();
        target = null;
    }

    Vector3 CalculateBlerp_p1() {
        Vector3 p1 =
            startPos
            + (initialGrappleDir * grappleLength * 0.1f)
            + Vector3.up * Mathf.Min(0.5f, Mathf.Abs(startVelocity.y * Time.deltaTime));
        return p1;
    }
    Vector3 CalculateBlerp_p2() {
        Vector3 p2 =
            target.position
            - (initialGrappleDir * grappleLength * 0.3f)
            + Vector3.up * grappleLength * 0.3f;
        return p2;
    }
    Vector3 CalculateBlerp_p1_swing() {
        Vector3 p1 =
            startPos
            + (initialGrappleDir * grappleLength * 0.1f)
            + Vector3.up * Mathf.Min(0.5f, Mathf.Abs(startVelocity.y * Time.deltaTime));
        return p1;
    }
    Vector3 CalculateBlerp_p2_swing() {
        Vector3 p2 =
            target.position
            - (initialGrappleDir * grappleLength * 0.5f)
            - Vector3.up * grappleLength * 0.1f;
        return p2;
    }
    Vector3 GetPointOnBezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
        float u = 1f - t;
        float t2 = t * t;
        float u2 = u * u;
        float u3 = u2 * u;
        float t3 = t2 * t;

        Vector3 result =
            (u3) * p0 +
            (3f * u2 * t) * p1 +
            (3f * u * t2) * p2 +
            (t3) * p3;

        return result;
    }
}
