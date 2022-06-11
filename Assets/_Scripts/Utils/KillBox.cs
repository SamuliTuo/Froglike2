using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillBox : MonoBehaviour {

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            other.GetComponent<PlayerTeleports>().TeleportToLastRespawn();
        }
    }
}
