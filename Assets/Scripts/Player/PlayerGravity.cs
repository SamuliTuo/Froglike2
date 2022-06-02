using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerGravity : MonoBehaviour {

    [HideInInspector] public bool jumpPressed = false;
    [HideInInspector] public float afterJumpExtraGravMultiplier = 0;
    [HideInInspector] public Coroutine apexGravCoroutine = null;

    [SerializeField] private float fallGravMult = 2.2f - 1;
    [SerializeField] private float lowJumpGravMult = 2.6f - 1;
    [SerializeField] private float apexGravTimer = 0.3f;

    private Rigidbody rb;
    private PlayerController control;
    private PlayerAttacks attack;
    private PlayerSpecialAttack special;
    private float apexGravMult = 1;
    private float perc, t;


    void Awake() {
        control = GetComponent<PlayerController>();
        attack = GetComponent<PlayerAttacks>();
        special = GetComponent<PlayerSpecialAttack>();
    }

    void Start() {
        rb = GetComponent<Rigidbody>();
    }

    void OnJump(InputValue value) {
        jumpPressed = value.isPressed;
    }

    public void HandleGravity() {
        if (attack.hijackControls || special.hijackControls) {
            return;
        }
        // Always applying the gravity once.
        SimpleGravity();
        AfterJumpExtraGravity();
        // apply extra gravity when falling
        if (rb.velocity.y < 0 && !control.PlayerGrounded) {
            rb.velocity += Vector3.up * Physics.gravity.y * apexGravMult * fallGravMult * Time.deltaTime;
        }
        else if (rb.velocity.y > 0) {
            // ...when the jump button is released
            if (!jumpPressed || control.state == PlayerStates.ATTACK) {
                rb.velocity += Vector3.up * Physics.gravity.y * lowJumpGravMult * Time.deltaTime;
            }
            // ...on walls
            else if (jumpPressed && control.PlayerOnSteep) {
                var dot = Mathf.Abs(Vector3.Dot(Vector3.up, control.steepNormal));
                rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpGravMult * dot) * Time.deltaTime;
            }
        }
    }

    public void SimpleGravity() {
        if (control.PlayerGrounded && rb.velocity.sqrMagnitude < 0.01f) {
            rb.velocity +=
                control.contactNormal *
                (Vector3.Dot(Physics.gravity, control.contactNormal) * Time.deltaTime);
        }
        else {
            rb.velocity += Physics.gravity * Time.deltaTime;
        }
    }
    
    public IEnumerator ApexGravMultiplier() {
        t = 0;
        yield return new WaitForFixedUpdate();
        while (rb.velocity.y > 0) {
            yield return null;
        }
        while (rb.velocity.y <= 0 && jumpPressed && !control.PlayerGrounded) {
            perc = t / apexGravTimer;
            perc *= perc;
            apexGravMult = Mathf.Lerp(-0.5f, 1, perc);
            t += Time.deltaTime;
            yield return null;
        }
        apexGravMult = 1;
    }

    public void SetAfterJumpExtraGravMultiplier(float value) {
        afterJumpExtraGravMultiplier = value;
    }
    void AfterJumpExtraGravity() {
        if (rb.velocity.y > 0 && !control.PlayerGrounded) {
            rb.velocity += Vector3.up * Physics.gravity.y * afterJumpExtraGravMultiplier * Time.deltaTime;
        }
    }
}
