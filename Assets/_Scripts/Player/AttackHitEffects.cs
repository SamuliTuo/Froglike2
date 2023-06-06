using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackHitEffects : MonoBehaviour {

    private List<UpgradeType> activeUpgrades = new List<UpgradeType>();
    public void SetActiveUpgrades(PlayerWeaponBuffScriptable script) {
        this.activeUpgrades = activeUpgrades;
    }
    public List<UpgradeType> GetActiveUpgrades()
    {
        return activeUpgrades;
    }
}
