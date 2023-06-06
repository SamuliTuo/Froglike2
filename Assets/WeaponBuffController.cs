using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBuffController : MonoBehaviour
{
    public PlayerWeaponBuffScriptable activeBuff = null;
    public PlayerWeaponBuffScriptable GetCurrentBuff() { return activeBuff; }

    [SerializeField] private float accelModReturnTime = 0.4f;
    [SerializeField] private GameObject buffEffectObj = null;

    private PlayerController control;
    private PlayerRotate rotate;
    private Animator anim;
    private Coroutine buffCoroutine;
    private float t;


    void Start()
    {
        control = GetComponentInParent<PlayerController>();
        rotate = transform.parent.GetComponentInChildren<PlayerRotate>();
        anim = transform.parent.GetComponentInChildren<Animator>();
    }


    public void ApplyBuff(PlayerWeaponBuffScriptable script)
    {
        
        if (buffCoroutine != null)
            StopCoroutine(buffCoroutine);

        buffCoroutine = StartCoroutine(Buff(script));
    }

    IEnumerator Buff(PlayerWeaponBuffScriptable script)
    {
        t = 0;
        activateT = 0;
        anim.CrossFade("mouth_buffWeapon", 0.15f, 0);
        control.state = PlayerStates.BUFFING_WEAPON;

        while (t < script.animationLength)
        {
            rotate.SetRotateSpdMod(0);
            control.moveSpeedMod = 0.1f;
            ActivateBuff(script);
            t += Time.deltaTime;
            yield return null;
        }

        t = 0;
        control.moveSpeedMod = 1;
        control.state = PlayerStates.NORMAL;
        rotate.InitRotateSpdModReturn(accelModReturnTime);
        control.InitAccelerationModReturn(accelModReturnTime, false);

        while (t < script.buffDuration)
        {
            ActivateBuff(script);
            t += Time.deltaTime;
            yield return null;
        }
        buffEffectObj.SetActive(false);
        activeBuff = null;
    }

    float activateT = 0;
    void ActivateBuff(PlayerWeaponBuffScriptable script)
    {
        if (activeBuff == null && activateT > script.timeBeforeBuffStarts)
        {
            activeBuff = script;
            buffEffectObj.SetActive(true);
        }
        else
        {
            activateT += Time.deltaTime;
        }
    }

    public void InterruptWeaponBuff()
    {
        if (buffCoroutine != null && activeBuff == null)
        {
            control.moveSpeedMod = 1;
            rotate.InitRotateSpdModReturn(accelModReturnTime);
            control.InitAccelerationModReturn(accelModReturnTime, false);
            buffEffectObj.SetActive(false);
            StopCoroutine(buffCoroutine);
            activeBuff = null;
            buffCoroutine = null;
        }
    }
}
