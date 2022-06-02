using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VisualScripting;

public enum EnemySizes {
    NULL,
    SMALL,
    MEDIUM,
    LARGE
}

public class Enemy_TongueInteraction : MonoBehaviour {

    public EnemySizes enemySize = EnemySizes.NULL;
    public Transform tongueTargetTransform = null;
    public float tonguePullSpeed = 1f;

    [SerializeField] private float pullOffsetFromPlayer = 2f;

    private NPC_Sieni_CombatStats _combat;
    private Vector3 startPos, endPos;
    private UnityEngine.AI.NavMeshAgent agent;
    private Transform player;


    void Start() {
        _combat = GetComponent<NPC_Sieni_CombatStats>();
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }

    public bool TongueInteract(Transform player) {
        if (!_combat.GetCanBeInterrupted()) {
            return false;
        }
        this.player = player;
        CustomEvent.Trigger(gameObject, "GoToTonguePull");
        //t = 0f;
        startPos = transform.position;
        //tongueScript.InitTongueGraphics(startPos + Vector3.up);

        if (enemySize == EnemySizes.SMALL) {
            agent.enabled = false;
        }
        else if (enemySize == EnemySizes.MEDIUM) {
            agent.ResetPath();
            agent.isStopped = true;
        }
        return true;
    }

    public Vector3 LerpEnemyPull(float t) {
        if (enemySize == EnemySizes.SMALL) {
            float perc = t * t * t;
            Vector3 dir = (transform.position - player.position).normalized;
            endPos = player.position + dir * pullOffsetFromPlayer;
            transform.position = Vector3.Lerp(startPos, endPos, perc);
            return tongueTargetTransform.position;
        }

        else if (enemySize == EnemySizes.MEDIUM) {
            if (t < 0.3f) {
                return tongueTargetTransform.position;
            }
            else {
                float perc = (t - 0.3f) / 0.7f;
                perc *= perc;
                return Vector3.Lerp(tongueTargetTransform.position, player.position, perc); ;
            }
        }

        else if (enemySize == EnemySizes.LARGE) {
            return Vector3.zero;
        }
        return Vector3.zero;
    }

    public void EndEnemyPull() {
        agent.enabled = true;
        agent.ResetPath();
        Variables.Object(gameObject).Set("Target", player.gameObject);
        CustomEvent.Trigger(gameObject, "BackToIdle");
    }

    public void SetValues(EnemySizes size) {
        this.enemySize = size;
    }
}
