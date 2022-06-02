using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_ActiveEffects : MonoBehaviour {

    [SerializeField] private float fireInstanceTimeAdded = 1f;
    [SerializeField] private float damageTakenFromFirePerSecond = 1f;

    private NPC_Sieni_CombatStats stats;
    private Enemy_LeekHurt hurt;
    private Quaternion effectRotation;
    private Coroutine fireActive = null;
    private float fireT = 0;
    private float waterAmount = 0;


    private void Start() {
        stats = GetComponent<NPC_Sieni_CombatStats>();
        hurt = GetComponent<Enemy_LeekHurt>();
    }

    public void AddEffect(UpgradeType effect, Quaternion effectRotation) {
        this.effectRotation = effectRotation;
        switch (effect) {
            case UpgradeType.FIRE:
                FireEffect();
                break;
            case UpgradeType.WATER:
                WaterEffect();
                break;
            case UpgradeType.GROUND:
                hurt.AddPoiseDmg(2);
                break;
            case UpgradeType.AIR:
                hurt.AddKbForce(3);
                break;
            case UpgradeType.SPARK: break;
            case UpgradeType.STEAM: break;
            case UpgradeType.LAVA: break;
            case UpgradeType.WILDFIRE: break;
            case UpgradeType.LIGHTNING: break;
            case UpgradeType.MUD: break;
            case UpgradeType.ICE: break;
            case UpgradeType.CHAIN_LIGHTNING: break;
            case UpgradeType.SAND: break;
            case UpgradeType.EARTHQUAKE: break;
            case UpgradeType.FAST_AS_FUCK_BOIIII_LIGHTNING_ATTACK: break;
            case UpgradeType.HAIL: break;
            case UpgradeType.GLASS: break;
            case UpgradeType.VOLCANO: break;
            case UpgradeType.MUDFLOOD: break;
            default: break;
        }
    }

    public void Activate() {
        WaterActivate();
    }

    // Fire :
    void FireEffect() {
        fireT += fireInstanceTimeAdded;
        if (fireT > 10) {
            fireT = 10;
        }
        if (fireActive == null) {
            fireActive = StartCoroutine(FireCoroutine());
        }
    }
    IEnumerator FireCoroutine() {
        while (fireT > 0) {
            fireT -= Time.deltaTime;
            stats.AddHealth(-damageTakenFromFirePerSecond * Time.deltaTime);
            yield return null;
        }
        fireActive = null;
    }
    // Water :
    void WaterEffect() {
        waterAmount++;
    }
    void WaterActivate() {
        if (waterAmount > 0) {
            Singleton.instance.DamageInstanceSpawner.SpawnDamageInstance(
                transform.position, new Vector3(2 * waterAmount, 1, 2 * waterAmount), effectRotation, waterAmount, 0.5f, 0.1f * waterAmount, 0.1f * waterAmount);
        }
        waterAmount = 0;
    }
}
