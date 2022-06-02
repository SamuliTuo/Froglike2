using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.AI;

public class Enemy_LeekHurt : MonoBehaviour {

    [HideInInspector] public NavMeshAgent agent;

    private NPC_Sieni_CombatStats combatStats;
    private Enemy_ActiveEffects effects;
    private Variables variables;
    private Vector3 kbSourcePos, startPos, endPos, kbDir;
    private float t, damage, poiseDmg, kbForce;
    private float agentOriginalSpeed, agentOriginalAcceleration;


    void Awake() {
        combatStats = GetComponent<NPC_Sieni_CombatStats>();
        effects = GetComponent<Enemy_ActiveEffects>();
        variables = GetComponent<Variables>();
        agent = GetComponent<NavMeshAgent>();
    }

    public void TakeDmg(GameObject player, List<UpgradeType> modifiers = null, float damage = 1, float poiseDmg = 1, float kbForce = 1) {
        Variables.Object(gameObject).Set("Target", player);
        TakeDmg(player.transform.position, modifiers, damage, poiseDmg, kbForce);
    }
    public void TakeDmg(Vector3 kbSourcePos, List<UpgradeType> modifiers = null, float damage = 1, float poiseDmg = 1, float kbForce = 1) {
        this.damage = damage;
        this.poiseDmg = poiseDmg;
        this.kbForce = kbForce;
        this.kbSourcePos = kbSourcePos;
        kbDir = (transform.position - kbSourcePos).normalized;

        if (modifiers != null) {
            for (int i = 0; i < modifiers.Count; i++) {
                effects.AddEffect(modifiers[i], Quaternion.LookRotation(new Vector3(kbDir.x, transform.position.y, kbDir.z)));
            }
            effects.Activate();
        }

        combatStats.AddHealth(-this.damage);
        CheckForKnockback();
    }

    void CheckForKnockback() {
        combatStats.hyperArmor -= poiseDmg;
        if (combatStats.hyperArmor < 0) {// && combatStats.GetCanBeInterrupted()) {
            combatStats.ResetHyperArmor();
            CustomEvent.Trigger(gameObject, "GoToHitEffects");
        }
        else {
            //poink! small kb or not at all? like minimal hitstun? or just hitStop for camera?
        }
    }

    public void AddDamage(float damage) {
        this.damage += damage;
    }
    public void AddPoiseDmg(float poiseDmg) {
        this.poiseDmg += poiseDmg;
    }
    public void AddKbForce(float kbForce) {
        this.kbForce += kbForce;
    }

    /*
    public void StartKnockback() {
        t = 0;
        startPos = transform.position;
        var dir = transform.position - kbSourcePos;
        dir.y *= 0;
        dir = dir.normalized;
        //ParticleEffects.current.PlayCloudRing(transform.position);
        transform.LookAt(new Vector3(kbSourcePos.x, transform.position.y, kbSourcePos.z));
        endPos = startPos + (dir * kbForce);
        agent.enabled = false;
    }*/

    public void StartKnockback() {
        t = 0;
        agentOriginalAcceleration = agent.acceleration;
        agentOriginalSpeed = agent.speed;
        agent.updateRotation = false;
        agent.acceleration = 1000;
        NavMeshHit hit;
        if (agent.Raycast(transform.position + kbDir * kbForce, out hit)) {
            agent.SetDestination(hit.position);
        }
        else {
            agent.SetDestination(transform.position + kbDir * kbForce);
        }
    }

    public bool LerpKnockBack() {
        if (t < 1) {
            //if (agent.velocity.sqrMagnitude > 0.5f) {
            transform.rotation = Quaternion.LookRotation(new Vector3(-kbDir.x, transform.position.y, -kbDir.z));
            //}
            agent.speed = Mathf.Lerp(3 * kbForce, 0, t);
            t += Time.deltaTime;
            return false;
        }
        else {
            agent.ResetPath();
            agent.speed = agentOriginalSpeed;
            agent.updateRotation = true;
            transform.rotation = Quaternion.LookRotation(new Vector3(-kbDir.x, transform.position.y, -kbDir.z));
            agent.acceleration = agentOriginalAcceleration;
            return true;
        }
    }

    public bool AmIDying() {
        if (combatStats.health <= 0) {
            return true;
        }
        else { return false; }
    }
}
