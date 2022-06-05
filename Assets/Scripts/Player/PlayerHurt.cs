using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHurt : MonoBehaviour {

    [SerializeField] private float invulnerabilityTime = 1f;
    [SerializeField] private float kbForceUp = 1f;
    [SerializeField] private float kbForceHoriz = 1f;
    [Space]
    [SerializeField] private float moveReduction = 0.5f;
    [SerializeField] private float rotateReduction = 0.5f;

    private PlayerController control;
    private PlayerRotate rotate;
    private PlayerHP hp;
    private Animator anim;
    private Rigidbody rb;
    private Material mat;
    private Color baseColor;
    private Vector3 hurtDir;
    private bool invulnerable = false;
    

    void Start() {
        control = GetComponent<PlayerController>();
        rotate = GetComponentInChildren<PlayerRotate>();
        hp = GetComponent<PlayerHP>();
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
        mat = transform.GetChild(0).GetChild(2).GetComponent<SkinnedMeshRenderer>().material;
        baseColor = mat.color;
    }

    public void Hurt(ContactPoint col, float damage) {
        Hurt(col.point, damage);
    }

    public void Hurt(Vector3 point, float damage) {
        if (invulnerable) {
            return;
        }
        control.ResetPlayer();
        if (hp.AddHPAndCheckIfStillAlive(-damage)) {
            invulnerable = true;
            control.state = PlayerStates.NOT_IN_CONTROL;
            hurtDir = new Vector3(
                (transform.position - point).x,
                0,
                (transform.position - point).z).normalized;
            rb.velocity = hurtDir * kbForceHoriz + Vector3.up * kbForceUp;
            anim.Play("longJumpStun", 0, 0);
            rotate.InitRotateSpdModReturn(rotateReduction);
            control.InitAccelerationModReturn(moveReduction, false);
            StartCoroutine(HurtCoroutine());
        }
        else {
            //ded
            SceneManager.LoadScene(3);
        }
    }

    IEnumerator HurtCoroutine() {
        mat.color = Color.red;
        yield return Helpers.GetWait(invulnerabilityTime * 0.3f);
        control.ResetPlayer();
        if (control.state != PlayerStates.CRAWL) {
            control.state = PlayerStates.NORMAL;
        }
        yield return Helpers.GetWait(invulnerabilityTime * 0.7f);
        invulnerable = false;
        mat.color = baseColor;
    }
}
