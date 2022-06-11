using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAbilityUpgradeHolder : MonoBehaviour {

    public UpgradeType slot1 = UpgradeType.NULL;
    public UpgradeType slot2 = UpgradeType.NULL;
    public UpgradeType slot3 = UpgradeType.NULL;
    public List<UpgradeType> activeUpgrades = new List<UpgradeType>();

    [SerializeField] private Image slot1_image = null;
    [SerializeField] private Image slot2_image = null;
    [SerializeField] private Image slot3_image = null;

    private void Start() { UpdateList(); }

    public bool TryToUpgradeAbility(UpgradeType type) {
        if (slot1 == UpgradeType.NULL) {
            slot1 = type;
            slot1_image.color = TypeToColor(type);
            UpdateList();
            return true;
        }
        else if (slot2 == UpgradeType.NULL) {
            slot2 = type;
            slot2_image.color = TypeToColor(type);
            UpdateList();
            return true;
        }
        else if (slot3 == UpgradeType.NULL) {
            slot3 = type;
            slot3_image.color = TypeToColor(type);
            UpdateList();
            return true;
        }
        return false;
    }

    void UpdateList() {
        activeUpgrades.Clear();
        if (slot1 != UpgradeType.NULL) {
            activeUpgrades.Add(slot1);
        }
        if (slot2 != UpgradeType.NULL) {
            activeUpgrades.Add(slot2);
        }
        if (slot3 != UpgradeType.NULL) {
            activeUpgrades.Add(slot3);
        }
    }

    // Make nice images for all upgrade types later
    Color TypeToColor(UpgradeType type) {
        Color r;
        switch (type) {
            case UpgradeType.FIRE:
                r = Color.red;
                break;
            case UpgradeType.WATER:
                r = Color.blue;
                break;
            case UpgradeType.GROUND:
                r = Color.black;
                break;
            case UpgradeType.AIR:
                r = Color.white;
                break;
            case UpgradeType.SPARK:
                r = Color.yellow;
                break;
                /*
            case UpgradeType.STEAM:
                break;
            case UpgradeType.LAVA:
                break;
            case UpgradeType.WILDFIRE:
                break;
            case UpgradeType.LIGHTNING:
                break;
            case UpgradeType.MUD:
                break;
            case UpgradeType.ICE:
                break;
            case UpgradeType.CHAIN_LIGHTNING:
                break;
            case UpgradeType.SAND:
                break;
                */
            default:
                r = Color.white;
                break;
        }
        return r;
    }
}
