using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordTrigger : MonoBehaviour {

    [HideInInspector] public bool triggerEnabled = false;

    private AttackHitEffects hitEffects;
    private CapsuleCollider col;
    private AttackInstance currentAttack;
    //private List<GameObject> objectsHit = new List<GameObject>();

    void Start() {
        hitEffects = GetComponentInParent<AttackHitEffects>();
        col = GetComponent<CapsuleCollider>();
    }

    public void ColliderOn(AttackInstance attackInstance) {
        currentAttack = attackInstance;
        triggerEnabled = true;
        col.enabled = true;
    }
    public void ColliderOff() {
        currentAttack = null;
        triggerEnabled = false;
        col.enabled = false;
    }

    void OnTriggerEnter(Collider other) {
        if (other.isTrigger == false && 
            other.CompareTag("Enemy") && 
            currentAttack != null) 
        {
            currentAttack.AddToObjectsHit(other.transform.root.gameObject);
        }
    }
}
