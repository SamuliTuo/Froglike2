using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Die : MonoBehaviour {

    [HideInInspector] public UnityEngine.AI.NavMeshAgent agent;

    [SerializeField] private LootInstanceSettings lootsSpawnedOnDeath = null;

    private float t = 0;
    private float delay;
    private float startMinimize;
    private float percRange;
    private List<Collider> cols = new List<Collider>();
    private NPC_Sieni_CombatStats stats;

    void Awake() {
        ListObjectColliders();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        stats = GetComponent<NPC_Sieni_CombatStats>();
    }

    public void StartDeath(float delay, float startMinimize) {
        this.delay = delay;
        this.startMinimize = startMinimize;
        percRange = delay - startMinimize;
        t = 0;
        foreach (var col in cols) {
            col.enabled = false;
        }
        if (agent.isOnNavMesh) {
            agent.isStopped = true;
        }
        //TongueAimer.current.RemoveTongueTarget(transform.parent);
    }

    public void Die() {
        t += Time.deltaTime;
        if (t >= startMinimize && t < delay) {
            float perc = (t - startMinimize) / percRange;
            perc = Mathf.Sin(perc * Mathf.PI * 0.5f);
            float scale = Mathf.Lerp(1, 0, perc);
            transform.localScale = new Vector3(scale, scale, scale);
        }
        else if (t >= delay) {
            Singleton.instance.GameEvents.EnemyDeath(this.gameObject);
            if (lootsSpawnedOnDeath != null) {
                Singleton.instance.LootSpawner.SpawnLoot(transform.position + Vector3.up * 0.3f, lootsSpawnedOnDeath);
            }
            //ParticleEffects.current.PlayWallJumpVFX(transform.parent.position, Vector3.up);
            Destroy(this.gameObject);
        }
    }

    void ListObjectColliders() {
        foreach (var item in GetComponentsInChildren<Collider>()) {
            cols.Add(item);
        }
    }
}
