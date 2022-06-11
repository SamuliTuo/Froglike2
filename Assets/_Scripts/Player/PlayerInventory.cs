using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInventory : MonoBehaviour {

    public int gems = 0;

    [SerializeField] private TextMeshProUGUI gemCounter = null;
    

    public void AddItem(LootType type) {
        if (type == LootType.GEMSTONE_PURPLE) {
            gems += 5;
        }
        else if (type == LootType.GEMSTONE_RED) {
            gems += 30;
        }
        RefreshGemCount();
    }

    void RefreshGemCount() {
        gemCounter.text = gems.ToString();
    }
}
