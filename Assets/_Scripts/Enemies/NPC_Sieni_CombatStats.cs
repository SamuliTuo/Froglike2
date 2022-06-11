using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting;

public class NPC_Sieni_CombatStats : MonoBehaviour {

    [HideInInspector] public float health;
    [HideInInspector] public float hyperArmor;

    [SerializeField] private float hpBarPositionOffsetUp = 1f;
    [SerializeField] private float maxHealth = 3f;
    [SerializeField] private float maxHyperArmor = 1f;
    [SerializeField] private float hyperArmorRegenRate = 1f;

    private List<EnemyHurtboxer> hurtboxers = new List<EnemyHurtboxer>();
    private HPBarInstance hpBar = null;
    private bool canBeInterrupted = true;

    public void SetCanBeInterrupted(bool state) { canBeInterrupted = state; }
    public bool GetCanBeInterrupted() { return canBeInterrupted; }

    void Start() {
        health = maxHealth;
        ResetHyperArmor();
        RefreshHealth();
        GetBodyHurtboxers();
    }

    void Update() {
        RegenHyperArmor();
    }

    public void SetHurtboxersState(bool state) {
        foreach (var hb in hurtboxers) { hb.SetHurtboxState(state); }
    }
    void GetBodyHurtboxers() {
        foreach (var hb in transform.GetChild(0).GetComponentsInChildren<EnemyHurtboxer>()) {
            hurtboxers.Add(hb);
        }
    }

    public void ResetHyperArmor() {
        hyperArmor = maxHyperArmor;
    }
    void RegenHyperArmor() {
        if (hyperArmor < maxHyperArmor) {
            hyperArmor += hyperArmorRegenRate * Time.deltaTime;
            if (hyperArmor > maxHyperArmor) {
                hyperArmor = maxHyperArmor;
            }
        }
    }

    public void AddHealth(float value) {
        health += value;
        RefreshHealth();
        UpdateHPBar();
    }
    void RefreshHealth() {
        if (health > maxHealth) {
            health = maxHealth;
        }
        if (health <= 0) {
            //GeneralSFX.current.ShroomBasicDeath(transform.position);
            if (hpBar != null) {
                hpBar.Deactivate();
            }
            CustomEvent.Trigger(gameObject, "Die");
            return;
        }
    }
    float HealthPercent() {
        return health / maxHealth;
    }

    void UpdateHPBar() {
        if (hpBar == null) {
            hpBar = Singleton.instance.EnemyHPBars.SpawnHPBar().GetComponent<HPBarInstance>();
            hpBar.Init(transform.GetChild(0).GetChild(0).GetChild(0), this, hpBarPositionOffsetUp);
        }
        hpBar.SetBarValue(HealthPercent());
    }

    public void DeactivateHPBar() {
        hpBar = null;
    }
}
