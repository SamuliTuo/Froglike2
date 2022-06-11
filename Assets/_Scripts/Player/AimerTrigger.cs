using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimerTrigger : MonoBehaviour {

    private TongueAimer aimer;


    private void Start() {
        aimer = GetComponentInParent<TongueAimer>();
    }

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Enemy")) {
            var target = other.transform.root.GetComponent<Enemy_TongueInteraction>().tongueTargetTransform;
            if (!aimer.tongueTargets.Contains(target)) {
                aimer.tongueTargets.Add(target);
            }
        }
        else if (!aimer.tongueTargets.Contains(other.transform) &&
            (other.CompareTag("Grapple_stop") ||
            other.CompareTag("Grapple_swing") ||
            other.CompareTag("Carryable"))) 
        {
            aimer.tongueTargets.Add(other.transform);
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.CompareTag("Enemy")) {
            var target = other.transform.root.GetComponent<Enemy_TongueInteraction>().tongueTargetTransform;
            if (aimer.tongueTargets.Contains(target)) {
                aimer.tongueTargets.Remove(target);
            }
        }
        else if (aimer.tongueTargets.Contains(other.transform)) {
            aimer.tongueTargets.Remove(other.transform);
        }
    }
}
