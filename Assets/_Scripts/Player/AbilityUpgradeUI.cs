using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityUpgradeUI : MonoBehaviour {

    private UpgradeType currentUpgradeType = UpgradeType.NULL;
    public void SetCurrentUpgradeType(UpgradeType type) {
        currentUpgradeType = type;
    }

    public void UpgradeAttack() {
        Singleton.instance.PlayerUpgradeHolder.UpgradeAbility(currentUpgradeType, UpgradeableAbility.ATTACK);
    }
    public void UpgradeSpecial() {
        Singleton.instance.PlayerUpgradeHolder.UpgradeAbility(currentUpgradeType, UpgradeableAbility.SPECIAL);
    }
    public void UpgradeJump() {
        Singleton.instance.PlayerUpgradeHolder.UpgradeAbility(currentUpgradeType, UpgradeableAbility.JUMP);
    }
    public void UpgradeRoll() {
        Singleton.instance.PlayerUpgradeHolder.UpgradeAbility(currentUpgradeType, UpgradeableAbility.ROLL);
    }
    public void UpgradeTongue() {
        Singleton.instance.PlayerUpgradeHolder.UpgradeAbility(currentUpgradeType, UpgradeableAbility.TONGUE);
    }
    public void UpgradeShout() {
        Singleton.instance.PlayerUpgradeHolder.UpgradeAbility(currentUpgradeType, UpgradeableAbility.SHOUT);
    }
    public void UpgradeThrow() {
        Singleton.instance.PlayerUpgradeHolder.UpgradeAbility(currentUpgradeType, UpgradeableAbility.THROW);
    }
}
