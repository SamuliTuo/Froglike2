using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHurtboxer : MonoBehaviour {

    private bool hurtboxActive = false;
    private Collider myCol;

    void Start() {
        myCol = GetComponent<Collider>();
    }

    public void SetHurtboxState(bool state) {
        myCol.isTrigger = state;
        hurtboxActive = state;
    }

    void OnTriggerEnter(Collider other) {
        if (hurtboxActive && other.CompareTag("Player") && !other.isTrigger) {
            var dir = other.transform.root.position - transform.root.GetChild(0).position;
            dir.y *= 0;
            other.transform.root.position += dir.normalized;
            Singleton.instance.PlayerHurt.Hurt(transform.root.position, 1);
        }
    }
}