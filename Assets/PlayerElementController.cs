using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public enum Elements { NULL, GRASS, FIRE, SPARK, AIR }

public class PlayerElementController : MonoBehaviour
{

    public Elements currentElement = Elements.NULL;
    public bool debug_unlockAllElements;

    private PlayerController control;
    private PlayerWeaponBuffScriptable currentBuff;
    private WeaponBuffController buffControl;
    private bool hasGrass = false;
    private bool hasFire = false;
    private bool hasSpark = false;
    private bool hasAir = false;
    private float elementsCount;


    void Start()
    {
        control = GetComponentInParent<PlayerController>();
        elementsCount = Enum.GetValues(typeof(Elements)).Length;
        buffControl = GetComponent<WeaponBuffController>();
        currentBuff = Resources.Load("PlayerAttacks/Fire/weaponBuff_fire") as PlayerWeaponBuffScriptable;
        UnlockElements();
    }

    public void ChangeElementLast()
    {
        Array values = Enum.GetValues(typeof(Elements));
        foreach (var val in values)
        {
            print(val);
        }
    }
    public void ChangeElementNext()
    {

    }
    public void ApplyElementOnWeapon()
    {
        if (currentElement != Elements.NULL)
        {
            if (control.state == PlayerStates.NORMAL)
            {
                buffControl.ApplyBuff(currentBuff);
            }
        }
    }


    void UnlockElements() {
        if (debug_unlockAllElements)
        {
            hasGrass = true;
            hasFire = true;
            hasSpark = true;
            hasAir = true;
            currentElement = Elements.GRASS;
        }
    }
}
