using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialTrigger : MonoBehaviour {

    [HideInInspector] public bool triggerEnabled = false;

    private AttackHitEffects hitEffects;
    private Collider col1;
    private Collider col2;
    private AttackInstance currentAttack;
    //private List<GameObject> objectsHit = new List<GameObject>();

    void Start() {
        hitEffects = GetComponentInParent<AttackHitEffects>();
        col1 = GetComponent<Collider>();
        col2 = GetComponent<Collider>();
    }

    public void ColliderOn(AttackInstance attackInstance) {
        currentAttack = attackInstance;
        triggerEnabled = true;
        col1.enabled = true;
        col2.enabled = true;
    }
    public void ColliderOff() {
        currentAttack = null;
        triggerEnabled = false;
        col1.enabled = false;
        col2.enabled = false;
    }

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Enemy") && 
            currentAttack != null && 
            other.isTrigger == false) 
        {
            currentAttack.AddToObjectsHit(other.transform.root.gameObject);
        }
    }
}
