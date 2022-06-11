using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackHitEffects : MonoBehaviour {

    private List<UpgradeType> activeUpgrades = new List<UpgradeType>();
    public void SetActiveUpgrades(List<UpgradeType> activeUpgrades) {
        this.activeUpgrades = activeUpgrades;
    }


    public void EnemyHit(Collider enemy) {
        enemy.transform.root.GetComponent<Enemy_LeekHurt>().TakeDmg(gameObject, activeUpgrades);
        Singleton.instance.VFXManager.SpawnVFX(VFXType.LEEK_IMPACT, enemy.transform.position);
    }
}
