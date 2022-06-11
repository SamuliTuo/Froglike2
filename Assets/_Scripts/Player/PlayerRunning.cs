using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerRunning : MonoBehaviour{

    [HideInInspector] public bool running = false;
    [HideInInspector] public float runMultiplier = 1;

    [SerializeField] private float maxRunSpeedMult = 2;
    [SerializeField] private float runSpeedAccelRate = 1;
    [SerializeField] private float runSpeedDecelRate = 1;
    [SerializeField] private float toggleRunOffTimerMax = 0.5f;
    //[SerializeField] private float minVelocitySquaredToStopRunning = 33f;

    private PlayerController control;
    private float toggleRunOffTimer = 0;
    private float walkSquared;
    private float t = 0;


    void Start() {
        control = GetComponent<PlayerController>();
        walkSquared = control.walkSpd * control.walkSpd;
    }

    void OnRun(InputValue value) {
        if (value.isPressed) {
            running = true;
        }
    }


    public float RunningMultiplier() {
        if (running) {
            float perc = Mathf.Sin(t * Mathf.PI * 0.5f);
            if (ToggleRunOff(perc)) {
                t -= Time.deltaTime * runSpeedDecelRate;
                if (t < 0) {
                    t = 0;
                }
            }
            else if (t < 1) {
                t += Time.deltaTime * runSpeedAccelRate;
                if (t >= 1) {
                    t = 1;
                }
            }
            runMultiplier = 1 + maxRunSpeedMult * perc;
            return runMultiplier;
        }
        else {
            t = 0;
            runMultiplier = 1;
            return 1;
        }
    }

    bool ToggleRunOff(float perc) {
        if (control.GetInput().sqrMagnitude < control.deadzoneSquared ||
            control.GetRelativeVelo().sqrMagnitude < walkSquared * maxRunSpeedMult * perc) 
        {
            toggleRunOffTimer += Time.deltaTime;
            if (toggleRunOffTimer > toggleRunOffTimerMax) {
                running = false;
                t = 0;
            }
            return true;
        }
        else {
            toggleRunOffTimer = 0;
            return false;
        }
    }
}
