using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHP : MonoBehaviour {

    [SerializeField] private Image hpBar = null;
    [SerializeField] private float maxHP = 10;

    private float hp;

    void Start() {
        hpBar = GameObject.Find("Canvas_UI").transform.Find("PlayerHP").GetChild(1).GetComponent<Image>();
        hp = maxHP;
    }

    public bool AddHPAndCheckIfStillAlive(float amount) {
        hp += amount;
        if (hp > maxHP) {
            hp = maxHP;
        }
        else if (hp <= 0) {
            hp = 0;
            RefreshHPBar();
            return false;
        }
        RefreshHPBar();
        return true;
    }

    void RefreshHPBar() {
        hpBar.fillAmount = hp / maxHP;
    }
}
