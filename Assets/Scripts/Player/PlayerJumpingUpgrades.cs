using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpingUpgrades : MonoBehaviour {

    private PlayerAbilityUpgradeHolder upgrades;

    void Awake() {
        upgrades = GetComponent<PlayerAbilityUpgradeHolder>();
    }

    public void JumpUpgrades() {
        for (int i = 0; i < upgrades.activeUpgrades.Count; i++) {
            switch (upgrades.activeUpgrades[i]) {
                case UpgradeType.FIRE:
                    Singleton.instance.DamageInstanceSpawner.SpawnDamageInstance(
                        transform.position - Vector3.up, new Vector3(1.5f, 1.5f, 1.5f), transform.root.GetChild(0).rotation, 3, 0.2f, 0.2f, 0.1f);
                    break;
                case UpgradeType.WATER:
                    break;
                case UpgradeType.GROUND:
                    break;
                case UpgradeType.AIR:
                    break;
                case UpgradeType.SPARK:
                    break;
                default:
                    break;
            }
        }
    }

    public void WalljumpUpgrades(Vector3 wallNormal, Vector3 contactPoint) {
        for (int i = 0; i < upgrades.activeUpgrades.Count; i++) {
            switch (upgrades.activeUpgrades[i]) {
                case UpgradeType.FIRE:
                    Singleton.instance.DamageInstanceSpawner.SpawnDamageInstance(
                        contactPoint, new Vector3(2.5f, 2.5f, 1), Quaternion.LookRotation(wallNormal), 3, 0.2f, 0.2f, 0.1f);
                    break;
                case UpgradeType.WATER:
                    break;
                case UpgradeType.GROUND:
                    break;
                case UpgradeType.AIR:
                    break;
                case UpgradeType.SPARK:
                    break;
                default:
                    break;
            }
        }
    }
}
