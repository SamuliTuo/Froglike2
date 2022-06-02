using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordTrigger : MonoBehaviour {

    [HideInInspector] public bool triggerEnabled = false;

    private AttackHitEffects hitEffects;
    private CapsuleCollider col;
    private List<GameObject> objectsHit = new List<GameObject>();

    void Start() {
        hitEffects = GetComponentInParent<AttackHitEffects>();
        col = GetComponent<CapsuleCollider>();
    }

    public void ToggleState(bool state) {
        objectsHit.Clear();
        triggerEnabled = state;
        col.enabled = state;
    }

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Enemy") && !objectsHit.Contains(other.transform.root.gameObject)) {
            objectsHit.Add(other.transform.root.gameObject);
            hitEffects.EnemyHit(other);
        }
    }
}
