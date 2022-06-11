using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRollingUpgrades : MonoBehaviour {

    [SerializeField] private float upgradeEffectProcInterval = 0.2f;
    [SerializeField] private float removeFromListDelay = 0.5f;

    private List<Transform> objectsHit = new List<Transform>();
    private Transform currentHit;
    private PlayerAbilityUpgradeHolder rollUpgrades;
    private PlayerRolling roll;
    private Transform model;
    private float t, dmg, poiseDmg, kbForce;

    void Awake() {
        rollUpgrades = GetComponent<PlayerAbilityUpgradeHolder>();
        roll = GetComponentInParent<PlayerRolling>();
        model = transform.root.GetChild(0);
    }

    public void StartRoll() {
        t = upgradeEffectProcInterval * 0.5f;
    }

    public void RollEffectsSpawnAOEs() {
        if (t >= upgradeEffectProcInterval) {
            t = 0;
            for (int i = 0; i < rollUpgrades.activeUpgrades.Count; i++) {
                switch (rollUpgrades.activeUpgrades[i]) {
                    case UpgradeType.FIRE:
                        Singleton.instance.DamageInstanceSpawner.SpawnDamageInstance_Sphere(
                            transform.position - Vector3.up * 0.5f, model.rotation, 1, 1, 0.2f, 0.2f, 0.2f);
                        break;
                    case UpgradeType.WATER:
                        Singleton.instance.DamageInstanceSpawner.SpawnDamageInstance_Sphere(
                            transform.position - Vector3.up * 0.5f, model.rotation, 1, 1, 0.2f, 0.2f, 0.2f);
                        break;
                    case UpgradeType.AIR:
                        roll.rollingSpeed += roll.rollingSpeed * 0.1f;
                        break;
                    case UpgradeType.SPARK:
                        Singleton.instance.DamageInstanceSpawner.SpawnDamageInstance_Sphere(
                            transform.position - Vector3.up * 0.5f, model.rotation, 1, 1, 0.2f, 0.2f, 0.2f);
                        break;
                    default:
                        break;
                }
            }
        }
        t += Time.deltaTime;
    }

    public void RollEffectsCollisions(Collision col) {
        if (rollUpgrades.activeUpgrades.Count > 0 && col.gameObject.CompareTag("Enemy")) {
            currentHit = col.transform.root;
            if (!objectsHit.Contains(currentHit)) {
                objectsHit.Add(currentHit);
                kbForce = poiseDmg = dmg = 0;
                for (int i = 0; i < rollUpgrades.activeUpgrades.Count; i++) {
                    switch (rollUpgrades.activeUpgrades[i]) {
                        case UpgradeType.FIRE:
                            print("poop");
                            break;
                        case UpgradeType.WATER:
                            print("poop");
                            break;
                        case UpgradeType.GROUND:
                            poiseDmg += 3;
                            break;
                        case UpgradeType.AIR:
                            kbForce += 4;
                            break;
                        case UpgradeType.SPARK:
                            print("poop");
                            break;
                        default:
                            break;
                    }
                }
                currentHit.GetComponent<Enemy_LeekHurt>().TakeDmg(gameObject, null, dmg, poiseDmg, kbForce);
                StartCoroutine(RemoveFromListAfterDelay(col.transform.root));
            }
        }
    }

    IEnumerator RemoveFromListAfterDelay(Transform tran) {
        yield return Helpers.GetWait(removeFromListDelay);
        objectsHit.Remove(tran);
    }
}
