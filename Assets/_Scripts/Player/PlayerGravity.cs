using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using Palmmedia.ReportGenerator.Core.Reporting.Builders;

public class PlayerGravity : MonoBehaviour {

    [HideInInspector] public bool jumpPressed = false;
    public float afterJumpExtraGrav = 0;
    [HideInInspector] public Coroutine apexGravCoroutine = null;
    [HideInInspector] public Coroutine brakeCoroutine = null;

    [SerializeField] private float fallGravMult = 2.2f - 1;
    [SerializeField] private float continuousRollFallGravMult = 4f;
    [SerializeField] private float lowJumpGravMult = 2.6f - 1;
    
    [SerializeField] private float wallGravMult = 3 - 1;
    [SerializeField] private float apexGravTimer = 0.3f;

    [Header("Extra gravity after jump")]
    [SerializeField] private float brakeDuration = 0.5f;
    [SerializeField] private float maxBrakeForce = 10;

    private Rigidbody rb;
    private PlayerController control;
    private PlayerInput input;
    private PlayerJumping jump;
    private PlayerAttacks attack;
    private PlayerSpecialAttack special;
    private float apexGravMult = 0;
    private float perc, t;
    private float apexT = 1;


    void Awake() {
        control = GetComponent<PlayerController>();
        input = GetComponent<PlayerInput>();
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

    public void StopJumpCoroutines()
    {
        StopAllCoroutines();
        StopAfterSpecialGrav();
        afterJumpExtraGrav = 0;
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
        if (afterSpecialgravityT > 0)
        {
            AfterSpecialGrav();
            return;
        }
        
        // Apex:
        _apexPoint = Mathf.InverseLerp(_jumpApexThreshold, 0, Mathf.Abs(rb.velocity.y));
        if (!control.PlayerGrounded && 
            _apexPoint != 0 && 
            (jumpPressed || (attack.currentAttack == attack.rollAttack && input.actions["Attack"].IsPressed()))
            )
        {
            if (jump.currentJump != JumpType.WALL)
            {
                var apexBonus = control.GetInput() * _apexBonus * _apexPoint;
                rb.velocity += apexBonus * Time.deltaTime;
            }
            _fallSpeed = Mathf.Lerp(_minFallSpeed, fallGravMult, _apexPoint);
            rb.velocity += Vector3.up * Physics.gravity.y * _fallSpeed * Time.deltaTime;
            return;
        }
        // Apply the gravity once.
        SimpleGravity();

        // After jump extra gravity
        if (rb.velocity.y > 0 && !control.PlayerGrounded && afterJumpExtraGrav > 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * afterJumpExtraGrav * Time.deltaTime;
        }
        // apply extra gravity if falling, or...
        if (rb.velocity.y < 0 && !control.PlayerGrounded)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * fallGravMult * Time.deltaTime;
        }
        else if (rb.velocity.y > 0) {
            //  ...when the jump button is released
            if (control.state == PlayerStates.ATTACK)
            {
                if (attack.currentAttack != attack.rollAttack && (!jumpPressed || !input.actions["Attack"].IsPressed()))
                {
                    rb.velocity += Vector3.up * Physics.gravity.y * lowJumpGravMult * Time.deltaTime;
                }
            }
            else if (!jumpPressed)
            {
                rb.velocity += Vector3.up * Physics.gravity.y * lowJumpGravMult * Time.deltaTime;
            }
            //  ...on walls
            else if (jumpPressed && control.PlayerOnSteep) {
                var dot = Mathf.Abs(Vector3.Dot(Vector3.up, control.steepNormal));
                rb.velocity += Vector3.up * Physics.gravity.y * (wallGravMult * dot) * Time.deltaTime;
            }
        }

        // Wall hang (janky town)
        if (control.PlayerOnSteep && rb.velocity.y < 0 && control.state == PlayerStates.NORMAL)
        {
            rb.velocity = rb.velocity - (rb.velocity * Time.deltaTime * changeRatef);
        }
        else if (control.PlayerOnSteep && rb.velocity.y < 0 && control.state == PlayerStates.ROLL)
        {
            print("wall go down, something fucky somewhere here...");
            rb.velocity = new (rb.velocity.x, rb.velocity.y - (rb.velocity.y * Time.deltaTime * wallHangingRollMode), rb.velocity.z);
        }
        CapFallSpeed();
    }

    public float wallHangingRollMode = 1;
    public float changeRatef = 5;
    void CapFallSpeed()
    {
        if (rb.velocity.y < -_maxFallSpeed && control.state != PlayerStates.BASEBALL && control.state != PlayerStates.SAMURAI)
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
            if (rb.velocity.y < 0.25f)
            {
                t = brakeDuration;
            }
            perc = t / brakeDuration;
            perc = Mathf.Sin(perc * Mathf.PI * 0.5f);
            afterJumpExtraGrav = Mathf.Lerp(0, maxBrakeForce, perc);
            t += Time.deltaTime;
            yield return null;
        } 
        while (rb.velocity.y > 0.25f)
        {
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
        }
        apexT = 1;
    }

    float afterSpecialgravityT = 0;
    [SerializeField] private float afterSpecialGravityReturnRate = 2;
    public void AfterBasebSamuraiGravity()
    {
        afterSpecialgravityT = 1;
    }

    void AfterSpecialGrav()
    {
        rb.velocity = new(rb.velocity.x, rb.velocity.y * Mathf.Lerp(0.7f, 0.95f, afterSpecialgravityT), rb.velocity.z);
        afterSpecialgravityT -= Time.deltaTime * afterSpecialGravityReturnRate;
        if (control.PlayerGrounded)
        {
            afterSpecialgravityT = 0;
        }
        rb.velocity += Vector3.up * Physics.gravity.y * (1 - afterSpecialgravityT);
    }
    public void StopAfterSpecialGrav()
    {
        afterSpecialgravityT = 0;
    }
}
