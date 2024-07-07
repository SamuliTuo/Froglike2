using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRotate : MonoBehaviour {

    [HideInInspector] public float rotateSpdMod = 1;

    [SerializeField] private float rotateSpd = 1;
    [SerializeField] private float airRotateSpd = 1;
    [SerializeField] private float rollRotateSpeed = 1;

    private PlayerController control;
    private Coroutine rotateModRoutine = null;
    private Rigidbody rb;
    private float t = 0;

    void Start() {
        control = GetComponentInParent<PlayerController>();
        rb = GetComponentInParent<Rigidbody>();
    }

    public void RotatePlayer() {
        if (control.state == PlayerStates.ROLL && 
            new Vector3(rb.velocity.x, 0, rb.velocity.z).sqrMagnitude > 0.1f) 
        {
            transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    Quaternion.LookRotation(
                        new Vector3(control.GetRelativeVelo().x, 0, control.GetRelativeVelo().z), Vector3.up),
                rollRotateSpeed * Time.deltaTime);
        }
        else if (t > 0) {
            if (control.PlayerGrounded == false) {
                t -= Time.deltaTime;
                var velocity = rb.velocity;
                var latestVelo = new Vector3(
                    velocity.x, 0, velocity.z).sqrMagnitude > 0 ? velocity : transform.forward;
                transform.rotation = Quaternion.RotateTowards(
                        transform.rotation,
                        Quaternion.LookRotation(new Vector3(latestVelo.x, 0, latestVelo.z), Vector3.up),
                        rotateSpd * Time.deltaTime * 10
                );
            }
            else {
                t = 0;
            }
            
        }
        else {
            Vector3 facing;
            if (control.GetInput().sqrMagnitude > control.deadzoneSquared) {
                facing = control.GetInput();
            }
            else {
                facing = new Vector3(transform.forward.x, 0, transform.forward.z);
            }
            float rotSpd = rotateSpd * rotateSpdMod;
            if (!control.PlayerGrounded) {
                rotSpd = airRotateSpd;
            }
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.LookRotation(facing),
                rotSpd * Time.deltaTime);
        }
    }

    public void SetRotateSpdMod(float value) {
        if (rotateModRoutine != null) {
            StopCoroutine(rotateModRoutine);
        }
        rotateSpdMod = value;
    }

    public void InitRotateSpdModReturn(float returnTime) {
        if (rotateModRoutine != null) {
            StopCoroutine(rotateModRoutine);
        }
        rotateModRoutine = StartCoroutine(ReturnRotSpd(returnTime));
    }

    IEnumerator ReturnRotSpd(float returnTime) {
        float t = 0;
        while (t < returnTime) {
            t += Time.deltaTime;
            var perc = t / returnTime;
            perc *= perc;
            rotateSpdMod = Mathf.Lerp(0, 1, perc);
            yield return null;
        }
        rotateSpdMod = 1;
    }

    public void AlignToVelocityForTime(float time) {
        t = time;
    }
}
