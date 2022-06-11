    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMana : MonoBehaviour
{
    [SerializeField] private float drainStartPerc = 0.3f;
    [SerializeField] private float drainRate = 0.4f;
    [SerializeField] private int stepsUntilFadeWhenFull = 3;
    [SerializeField] private float manaFlashDuration = 0.2f;

    private PlayerController control;
    private float currentMana = 1;
    private float currentDrain = 1;
    private GameObject background;
    private Image manaBar;
    private Image drainBar;


    void Start()
    {
        control = GetComponent<PlayerController>();
        var manaCanvas = GameObject.Find("Canvas_UI").transform.Find("PlayerMana");
        background = manaCanvas.GetChild(0).gameObject;
        manaBar = manaCanvas.GetChild(2).GetComponent<Image>();
        drainBar = manaCanvas.GetChild(1).GetComponent<Image>();
    }

    public void RegenMana(float amount)
    {
        if (currentMana < 1)
        {
            currentMana += amount;
            if (currentMana > 1)
            {
                currentMana = 1;
            }
            RefreshManaBar();
        }
    }

    public bool TryUseMana(float amount)
    {
        if (currentMana >= amount * 0.6f)
        {
            if (currentDrain < currentMana)
            {
                currentDrain = currentMana;
                RefreshDrainBar();
            }
            currentMana -= amount;
            if (currentMana < 0)
            {
                currentMana = 0;
            }
            StartDrain();
            RefreshManaBar();
            return true;
        }
        return false;
    }

    public void FlashManaBar()
    {
        StopAllCoroutines();
        StartCoroutine(FlashMana());
    }
    IEnumerator FlashMana()
    {
        manaBar.color = Color.white;
        yield return Helpers.GetWait(manaFlashDuration);
        manaBar.color = Color.blue;
    }

    [SerializeField] private float drainLerpRate = 1;
    private float t, perc, startDrainAmount;
    private Coroutine drainCoroutine = null;
    
    void StartDrain()
    {
        if (drainCoroutine != null)
        {
            if (t < drainStartPerc)
            {
                t = 0;
            }
            else
            {
                StopCoroutine(drainCoroutine);
                drainCoroutine = StartCoroutine(Drain(currentDrain));
            }
        }
        else
        {
            drainCoroutine = StartCoroutine(Drain(currentMana));
        }
    }

    public void HoldDrain()
    {
        t = 0;
    }

    IEnumerator Drain(float startValue)
    {
        t = perc = 0;
        startDrainAmount = startValue;
        while (t < 1)
        {
            perc = Mathf.Sin(t * Mathf.PI * 0.5f);
            if (perc > drainStartPerc)
            {
                currentDrain = Mathf.Lerp(startDrainAmount, currentMana, perc);
                RefreshDrainBar();
            }
            t += Time.deltaTime * drainLerpRate;
            yield return null;
        }
        currentDrain = currentMana;
        RefreshDrainBar();
        drainCoroutine = null;
    }

    void ToggleBars(bool state)
    {
        manaBar.gameObject.SetActive(state);
        drainBar.gameObject.SetActive(state);
        background.gameObject.SetActive(state);
    }

    void RefreshManaBar()
    {
        manaBar.fillAmount = currentMana;
    }
    void RefreshDrainBar()
    {
        drainBar.fillAmount = currentDrain;
    }
}
