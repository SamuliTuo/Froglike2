using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerGravity : MonoBehaviour {

    [HideInInspector] public bool jumpPressed = false;
    [HideInInspector] public float afterJumpExtraGrav = 0;
    [HideInInspector] public Coroutine apexGravCoroutine = null;
    [HideInInspector] public Coroutine brakeCoroutine = null;

    [SerializeField] private float fallGravMult = 2.2f - 1;
    [SerializeField] private float lowJumpGravMult = 2.6f - 1;
    [SerializeField] private float apexGravTimer = 0.3f;

    [Header("Extra gravity after jump")]
    [SerializeField] private float brakeDuration = 0.5f;
    [SerializeField] private float maxBrakeForce = 10;

    private Rigidbody rb;
    private PlayerController control;
    private PlayerJumping jump;
    private PlayerAttacks attack;
    private PlayerSpecialAttack special;
    private float apexGravMult = 0;
    private float perc, t;
    private float apexT = 1;


    void Awake() {
        control = GetComponent<PlayerController>();
        jump = GetComponent<PlayerJumping>();
        attack = GetComponent<PlayerAttacks>();
        special = GetComponent<PlayerSpecialAttack>();
    }

    void Start() {
        rb = GetComponent<Rigidbody>();
    }

    void OnJump(InputValue value) {
        jumpPressed = value.isPressed;
    }

    public float _jumpApexThreshold = 1;
    public float _apexBonus = 1;
    public float _minFallSpeed = 1;
    public float _maxFallSpeed = 10f;
    float _apexPoint;
    float _fallSpeed;
    public void HandleGravity() {
        if (attack.hijackControls || special.hijackControls) {
            return;
        }
        if (control.state == PlayerStates.SAMURAI || control.state == PlayerStates.BASEBALL) {
            if (!control.PlayerGrounded && rb.velocity.y < 0)
            {
                rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * 0.8f, rb.velocity.z);
            }
        }
        
        // Apex:
        _apexPoint = Mathf.InverseLerp(_jumpApexThreshold, 0, Mathf.Abs(rb.velocity.y));
        if (!control.PlayerGrounded && _apexPoint != 0 && jumpPressed)
        {
            if (jump.currentJump != JumpType.WALL)
            {
                var apexBonus = control.GetInput() * _apexBonus * _apexPoint;
                rb.velocity += apexBonus * Time.deltaTime;
            }
            _fallSpeed = Mathf.Lerp(_minFallSpeed, lowJumpGravMult, _apexPoint);
            rb.velocity += Vector3.up * Physics.gravity.y * _fallSpeed * Time.deltaTime;
            return;
        }
        // Apply the gravity once.
        SimpleGravity();

        // After jump extra gravity
        if (rb.velocity.y > 0 && !control.PlayerGrounded && afterJumpExtraGrav > 0) {
            rb.velocity += Vector3.up * Physics.gravity.y * afterJumpExtraGrav * Time.deltaTime;
        }
        // apply extra gravity if falling, or...
        if (rb.velocity.y < 0 && !control.PlayerGrounded) {
            rb.velocity += Vector3.up * Physics.gravity.y * fallGravMult * Time.deltaTime;
        }
        else if (rb.velocity.y > 0) {
            //  ...when the jump button is released
            if (!jumpPressed || control.state == PlayerStates.ATTACK) {
                rb.velocity += Vector3.up * Physics.gravity.y * lowJumpGravMult * Time.deltaTime;
            }
            //  ...on walls
            else if (jumpPressed && control.PlayerOnSteep) {
                var dot = Mathf.Abs(Vector3.Dot(Vector3.up, control.steepNormal));
                rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpGravMult * dot) * Time.deltaTime;
            }
        }
        // 
        CapFallSpeed();
    }

    void CapFallSpeed()
    {
        if (rb.velocity.y < -_maxFallSpeed)
        {
            rb.velocity = new Vector3(rb.velocity.x, -_maxFallSpeed, rb.velocity.z);
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
    public IEnumerator JumpBrakes()
    {
        t = 0;
        yield return new WaitForFixedUpdate();
        while (t < brakeDuration)
        {
            if (rb.velocity.y < -0.25f)
            {
                t = brakeDuration;
            }
            perc = t / brakeDuration;
            perc = Mathf.Sin(perc * Mathf.PI * 0.5f);
            afterJumpExtraGrav = Mathf.Lerp(0, maxBrakeForce, perc);
            t += Time.deltaTime;
            yield return null;
        }
        afterJumpExtraGrav = 0;
    }

    float apexStartVelo;
    float apexVelo;
    public IEnumerator ApexGravMultiplier() {
        yield return new WaitForFixedUpdate();
        while (rb.velocity.y > -0.25f) {
            yield return null;
        }
        apexT = 0;
        apexStartVelo = rb.velocity.y;
        while (!control.PlayerGrounded && apexT < apexGravTimer) {
            perc = apexT / apexGravTimer;
            if (perc < 0.5f) {
                perc *= 2;
                perc = Mathf.Sin(perc * Mathf.PI * 0.5f);
                apexVelo = Mathf.Lerp(apexStartVelo, 0, perc);
            }
            else if (perc < 1f) {
                perc = (perc - 0.5f) * 2;
                perc = Mathf.Sin(perc * Mathf.PI * 0.5f);
                //perc = 1f - Mathf.Cos(perc * Mathf.PI * 0.5f);
                apexGravMult = Mathf.Lerp(0, fallGravMult, perc);
            }

            apexT += Time.deltaTime;
            if (!jumpPressed) {
                apexT += Time.deltaTime * 3;
            }
            yield return null;
            

            /*
            apexGravMult = perc;
            if (!jumpPressed)
            {
                apexT += Time.deltaTime * 3f;
                //apexGravMult = 0.1f;
                
                /*
                perc = t / apexGravTimer;
                perc = Mathf.Sin(perc * Mathf.PI * 0.5f);
                apexGravMult = Mathf.Lerp(0f, 1, perc);
                //
            }
            apexT += Time.deltaTime;
            yield return null;
            */
        }
        apexT = 1;
    }

    void AfterJumpExtraGravity() {
        
    }


}
