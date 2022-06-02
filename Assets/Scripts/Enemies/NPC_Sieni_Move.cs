using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC_Sieni_Move : MonoBehaviour {

    Animator anim;
    public UnityEngine.AI.NavMeshAgent agent;
    NPC_Attack_Basic combat;
    public float followDist = 5f;
    string state = "idle";

    void Start() {
        this.agent = GetComponent<UnityEngine.AI.NavMeshAgent>(); 
        this.combat = GetComponent<NPC_Attack_Basic>();
        this.anim = GetComponentInChildren<Animator>();
    }

    public void Idle() {
        if(state != "idle") {
            //anim.Play("npc_shromSmall|idle_base", 0);
            state = "idle";
        }
    }

    public void NavTarget(Vector3 pos) {
        agent.SetDestination(pos);
    }

    public void Stop() {
        agent.isStopped = true;
    }

    public void Go() {
        agent.isStopped = false;
    }

    public void WalkAnim() {
        if(state != "walk") {
            state = "walk";
            //anim.Play("npc_shromSmall|walk", 0);
        }
    }

    public void AggroAnim() {
        if(state != "aggro") {
            state = "aggro";
            //anim.Play("npc_shromSmall|alert_noticePlayer", 0 );
        }
    }
    
    /*
    public void FollowDist(GameObject target) {
        float calcDist = Vector3.Distance(target.transform.position, transform.position);
        if(calcDist > followDist) {
            agent.SetDestination(combat.targetObj.transform.position);
        }
    }*/
}
