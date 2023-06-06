using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum ChargePhase { PHASE_01, PHASE_02, PHASE_03 }
public class PlayerMouthController : MonoBehaviour
{
    [SerializeField] private float offsetUp = 0;
    [SerializeField] private float offsetForward = 0;
    [SerializeField] private ProjectileScriptable slot1_phase1 = null;
    [SerializeField] private ProjectileScriptable slot1_phase2 = null;
    [SerializeField] private float afterShootingWaitTime = 0.25f;

    private Image meter_phase01;
    private Image meter_phase02;
    private GameObject bg_phase01;
    private GameObject bg_phase02;
    private ProjectileScriptable currProjectile;
    private ChargePhase currentPhase;
    private Transform inputSpace;
    private PlayerController control;
    private PlayerMana mana;
    private Animator anim;
    private PlayerRotate rotate;
    private GameObject projectile;
    private Transform model;
    private bool buttonHeld = false;
    private float t, perc, maxCharge;


    void Start()
    {
        var o = GameObject.Find("Canvas_UI/spellChargeMeter").transform;
        meter_phase01 = o.GetChild(1).GetChild(1).GetComponent<Image>();
        meter_phase02 = o.GetChild(0).GetChild(1).GetComponent<Image>();
        bg_phase01 = o.GetChild(1).GetChild(0).gameObject;
        bg_phase02 = o.GetChild(0).GetChild(0).gameObject;
        projectile = Resources.Load("Projectiles/projectile_fire_01") as GameObject;
        control = GetComponent<PlayerController>();
        mana = GetComponent<PlayerMana>();
        rotate = GetComponentInChildren<PlayerRotate>();
        model = transform.GetChild(0);
        anim = GetComponentInChildren<Animator>();
    }

    void OnMouth(InputValue value)
    {
        buttonHeld = value.isPressed;
        if (!buttonHeld)
            return;

        if (control.state == PlayerStates.NORMAL && mana.TryUseMana(slot1_phase1.manaCostInitial))
        {
            InitFireball();
        }
    }

    public void InitFireball()
    {
        if (control.state == PlayerStates.NORMAL && mana.TryUseMana(slot1_phase1.manaCostInitial) )
        {
            control.ResetQueuedAction();
            if (inputSpace == null)
            {
                inputSpace = control.GetPlrInputSpace();
            }
            control.state = PlayerStates.MOUTH;
            if (control.GetInput().sqrMagnitude > control.deadzoneSquared)
            {
                model.rotation = Quaternion.LookRotation(control.GetInput(), Vector3.up);
            }
            anim.Play("mouth_charge", 0, 0);
            currProjectile = slot1_phase1;
            currentPhase = ChargePhase.PHASE_01;
            t = perc = 0;
            StartCoroutine(Fireball());
        }
    }

    IEnumerator Fireball()
    {
        if (currProjectile == null)
            yield break;

        maxCharge = slot1_phase1.chargeTimeMax + slot1_phase2.chargeTimeMax;
        while (currentPhase == ChargePhase.PHASE_01)
        {
            if (buttonHeld || t < slot1_phase1.chargeTimeMin)
            {
                if (t < slot1_phase1.chargeTimeMax)
                {
                    UpdatePhase(currProjectile);
                    t += Time.unscaledDeltaTime;
                }
                else
                {
                    currentPhase = ChargePhase.PHASE_02;
                    currProjectile = slot1_phase2;
                }
                yield return null;
            }
            else
            {
                EndAndShoot();
                yield break;
            }
        }
        while (currentPhase == ChargePhase.PHASE_02)
        {
            if (buttonHeld || t < slot1_phase1.chargeTimeMax + slot1_phase2.chargeTimeMin)
            {
                if (t < slot1_phase2.chargeTimeMax)
                {
                    UpdatePhase(currProjectile);
                    t += Time.unscaledDeltaTime;
                }
                else
                {
                    EndAndShoot();
                    yield break;
                }
                yield return null;
            }
            else
            {
                EndAndShoot();
                yield break;
            }
        }
        EndAndShoot();
    }

    void UpdatePhase(ProjectileScriptable slot)
    {
        mana.TryUseMana(slot.manaCostOngoing * Time.deltaTime);
        perc = t / slot.chargeTimeMax;
        Time.timeScale = Mathf.Lerp(slot.timeScaleMinMax.y, slot.timeScaleMinMax.x, perc);
        perc = Mathf.Sin(perc * Mathf.PI * 0.5f);
        FillMeter();
        rotate.SetRotateSpdMod(
            Mathf.Lerp(slot.rotateSpeedMultiplierMinMax.x, slot.rotateSpeedMultiplierMinMax.y, perc));
        control.moveSpeedMod =
            Mathf.Lerp(slot.moveSpeedMultiplierMinMax.x, slot.moveSpeedMultiplierMinMax.y, perc);
    }

    void FillMeter()
    {
        switch (currentPhase)
        {
            case ChargePhase.PHASE_01:
                if (bg_phase01.gameObject.activeSelf == false)
                    bg_phase01.gameObject.SetActive(true);
                meter_phase01.fillAmount = perc;
                SetMeter(meter_phase01, bg_phase01, perc);
                break;
            case ChargePhase.PHASE_02:
                if (bg_phase02.gameObject.activeSelf == false)
                    bg_phase02.gameObject.SetActive(true);
                meter_phase02.fillAmount = perc;
                SetMeter(meter_phase02, bg_phase02, perc);
                break;
            default:
                break;
        }
    }

    void SetMeter(Image meter, GameObject bg, float value)
    {
        meter.fillAmount = value;
        if (meter.fillAmount <= 0)
        {
            bg.gameObject.SetActive(false);
        }
    }

        

    void EndAndShoot()
    {
        StopAllCoroutines();
        anim.Play("mouth_shoot", 0, 0);
        Time.timeScale = 1;
        Vector3 dir;
        if (inputSpace && control.GetInput().sqrMagnitude > control.deadzoneSquared)
        {
            dir = (control.GetInput() * currProjectile.shootForceForwardMultiplier +
                Vector3.up * currProjectile.shootForceUpMultiplier).normalized;
        }
        else
        {
            dir = (model.forward * currProjectile.shootForceForwardMultiplier + 
                Vector3.up * currProjectile.shootForceUpMultiplier).normalized;
        }
        dir *= Mathf.Lerp(currProjectile.shootForceMinMax.x, currProjectile.shootForceMinMax.y, perc);
        var clone = Instantiate(projectile,
                transform.position + (Vector3.up * offsetUp) + (model.forward * offsetForward),
                Quaternion.identity);
        clone.transform.localScale = Vector3.Lerp(
            currProjectile.projectileScaleMin, currProjectile.projectileScaleMax, perc);
        clone.GetComponent<ProjectileController>().InitProjectile(currProjectile, perc, dir);
        rotate.InitRotateSpdModReturn(currProjectile.rotateModReturnTime);
        control.moveSpeedMod = 1;
        control.InitAccelerationModReturn(currProjectile.accelModReturnTime, false);
        StartCoroutine(EndWait());
        SetMeter(meter_phase01, bg_phase01, 0);
        SetMeter(meter_phase02, bg_phase02, 0);
    }


    IEnumerator EndWait()
    {
        yield return Helpers.GetWait(afterShootingWaitTime);
        control.InitQueuedAction();
        //control.state = PlayerStates.NORMAL;
    }
}
