using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeGem : MonoBehaviour {

    [SerializeField] private UpgradeType gemType = UpgradeType.NULL;
    [SerializeField] private int uses = 1;
    private bool looted = false;
    
    // CALL ME FROM SOMEWHERE!
    public void LootMe() {
        if (!looted) {
            Singleton.instance.PlayerUpgradeHolder.OpenAbilityUpgradeUI(gemType, uses);
            looted = true;
        }
        Destroy(this.gameObject);
    }
}
