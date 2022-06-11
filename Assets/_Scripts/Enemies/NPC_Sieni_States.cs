using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_Sieni_States : MonoBehaviour {

    public UnityEngine.AI.NavMeshAgent agent;
    Vector3 target;

    void Start() {
        agent = gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
    }
}
