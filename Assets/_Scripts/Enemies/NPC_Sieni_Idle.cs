using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

public class NPC_Sieni_Idle : MonoBehaviour {

    public UnityEngine.AI.NavMeshAgent agent;
    NPC_Attack_Basic combat;
    NPC_Movement move;
    SphereCollider deAggroCol;
    float deAggroColRadiusSquared;

    void Start() {
        this.agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        this.combat = GetComponent<NPC_Attack_Basic>();
        this.move = GetComponent<NPC_Movement>();
        deAggroCol = transform.GetChild(2).GetComponent<SphereCollider>();
        SetAggroColRadiusSquared();
    }

    public void Idle() {
        //move.go.idle
    }
    
    public bool CheckIfPlayerInAggroRange() {
        var target = Variables.Object(gameObject).Get("Target");
        if (target != null) {
            float distSquared = Vector3.SqrMagnitude(transform.position - target.ConvertTo<GameObject>().transform.position);
            if (distSquared < deAggroColRadiusSquared) {
                return true;
            }
        }
        return false;
    }

    void SetAggroColRadiusSquared() {
        deAggroColRadiusSquared = deAggroCol.radius * deAggroCol.radius;
    }
}
