using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum UpgradeType { NULL,
    FIRE, WATER, GROUND, AIR, SPARK,

    STEAM,          // fire + water
    LAVA,           // fire + ground
    WILDFIRE,       // fire + air ( = flamethrower )
    LIGHTNING,      // fire + spark ( = Lina ultimate )
    MUD,            // water + ground
    ICE,            // water + air
    CHAIN_LIGHTNING,// water + spark
    SAND,           // ground + air
    EARTHQUAKE,     // ground + spark
    FAST_AS_FUCK_BOIIII_LIGHTNING_ATTACK, // air + spark (kimetsu no yaiba keltahiuksinen äiä)

    // steam + lata
    // steam + wildfire
    // steam + lightning
    // steam + mud
    HAIL,           // steam + ice
    // steam + chain_lightning
    // steam + sand
    // steam + earthquake
    // steam + fastAsfuck

    // lava + wildfire
    // lava + lightning
    // lava + mud
    // lava + ice
    // lava + chain_ligh
    GLASS,          // lava + sand
    VOLCANO,        // lava + Earthquake
    // lava + sonic

    // wildfire + lightning
    // wildfire + mud
    // wildfire + ice
    // wildfire + chain_lightn
    // wildfire + sand
    // wildfire + Earthquake
    // wildfire + sanic

    // mud + ice
    // mud + chain_light
    // mud + sand
    MUDFLOOD,       // mud + Earthquake  (mutavyöry)
    // mud + sanic

    // ice + chain lightning
    // ice + sand
    // ice + eqrthquake
    // ice + sonic

    // chain lightning + sand
    // chain lightning + Earthquake
    // chain lightning + sanic

    // earthQuake + sonic the hedgebog


}

public enum UpgradeableAbility { NULL, ATTACK, SPECIAL, JUMP, ROLL, TONGUE, SHOUT, THROW }

public class PlayerAbilityUpgrader : MonoBehaviour {

    [SerializeField] private GameObject UIholder = null;
    [SerializeField] private PlayerAbilityUpgradeHolder slot_attack = null;
    [SerializeField] private PlayerAbilityUpgradeHolder slot_special = null;
    [SerializeField] private PlayerAbilityUpgradeHolder slot_jump = null;
    [SerializeField] private PlayerAbilityUpgradeHolder slot_roll = null;
    [SerializeField] private PlayerAbilityUpgradeHolder slot_tongue = null;
    [SerializeField] private PlayerAbilityUpgradeHolder slot_shout = null;
    [SerializeField] private PlayerAbilityUpgradeHolder slot_throw = null;

    private HideCursor cursor;
    private PlayerStates lastState;
    private int uses = 0;

    private void Awake()
    {
        cursor = Helpers.Cam.GetComponentInParent<HideCursor>();
    }

    public void OpenAbilityUpgradeUI(UpgradeType type, int uses) {
        cursor.ToggleCursor(true);
        this.uses = uses;
        Time.timeScale = 0;
        lastState = Singleton.instance.Player.GetComponent<PlayerController>().state;
        Singleton.instance.Player.GetComponent<PlayerController>().state = PlayerStates.NOT_IN_CONTROL;
        UIholder.gameObject.SetActive(true);
        UIholder.GetComponent<AbilityUpgradeUI>().SetCurrentUpgradeType(type);
    }
    public void CloseAbilityUpgradeUI() {
        cursor.ToggleCursor(false);
        UIholder.gameObject.SetActive(false);
        Time.timeScale = 1;
        Singleton.instance.Player.GetComponent<PlayerController>().state = lastState;
    }

    public void UpgradeAbility(UpgradeType type, UpgradeableAbility slot) {
        bool allowed;
        switch (slot) {
            case UpgradeableAbility.ATTACK:
                allowed = slot_attack.TryToUpgradeAbility(type);
                break;
            case UpgradeableAbility.SPECIAL:
                allowed = slot_special.TryToUpgradeAbility(type);
                break;
            case UpgradeableAbility.JUMP:
                allowed = slot_jump.TryToUpgradeAbility(type);
                break;
            case UpgradeableAbility.ROLL:
                allowed = slot_roll.TryToUpgradeAbility(type);
                break;
            case UpgradeableAbility.TONGUE:
                allowed = slot_tongue.TryToUpgradeAbility(type);
                break;
            case UpgradeableAbility.SHOUT:
                allowed = slot_shout.TryToUpgradeAbility(type);
                break;
            case UpgradeableAbility.THROW:
                allowed = slot_throw.TryToUpgradeAbility(type);
                break;
            default:
                allowed = false;
                break;
        }
        if (allowed) {
            uses--;
            if (uses <= 0) {
                CloseAbilityUpgradeUI();
            }
        }
    }
}
