using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStamina : MonoBehaviour
{
    [SerializeField] private float regenRate_combat = 0.1f;
    [SerializeField] private float regenRate_safe = 1;
    [SerializeField] private int stepsUntilRegen = 40;
    [SerializeField] private int stepsUntilDecay = 20;
    [SerializeField] private int stepsUntilDisappearWhenFull = 3;
    [SerializeField] private float StaminaFlashDuration = 0.2f;

    private PlayerController control;
    private float currentStamina = 1;
    private float currentDrain = 1;
    private GameObject background;
    private Image staminaBar;
    private Image drainBar;
    
    private int stepsSinceDrain;
    private int stepsSinceStaminaFull;


    void Start()
    {
        control = GetComponent<PlayerController>();
        var staminaCanvas = GameObject.Find("Canvas_UI").transform.Find("PlayerStamina");
        background = staminaCanvas.GetChild(0).gameObject;
        staminaBar = staminaCanvas.GetChild(2).GetComponent<Image>();
        drainBar = staminaCanvas.GetChild(1).GetComponent<Image>();
    }

    void FixedUpdate() 
    {
        if (currentStamina < 1)
        {
            if (background.activeSelf == false)
            {
                ToggleBars(true);
            }
            RegenStamina();
        }
        else
        {
            if (background.activeSelf)
            {
                if (stepsSinceStaminaFull >= stepsUntilDisappearWhenFull)
                {
                    ToggleBars(false);
                }
                else
                {
                    stepsSinceStaminaFull++;
                }
            }
        }

        if (currentDrain > 0)
            DecayDrain();
        stepsSinceDrain++;
    }

    public void FlashStaminaBar()
    {
        StopAllCoroutines();
        StartCoroutine(FlashStamina());
    }
    IEnumerator FlashStamina()
    {
        staminaBar.color = Color.red;
        yield return Helpers.GetWait(StaminaFlashDuration);
        staminaBar.color = Color.green;
    }

    public void GainStamina(float perc)
    {
        currentStamina += perc;
        currentStamina = Mathf.Min(1, currentStamina);
        RefreshStaminaBar();
        if (currentDrain < currentStamina)
        {
            currentDrain = currentStamina;
            RefreshDrainBar();
        }
    }

    public bool HasStamina(float perc)
    {
        if (currentStamina < perc * 0.5f)
        {
            return false;
        }
        return true;
    }
    public bool TryDrainStamina(float perc)
    {
        if (currentStamina < perc * 0.5f) 
        {
            return false;
        }

        stepsSinceDrain = 0;
        stepsSinceStaminaFull = 0;
        regenStarted = false;
        if (currentStamina > currentDrain)
        {
            currentDrain = currentStamina;
            RefreshDrainBar();
        }
        
        currentStamina -= perc;
        RefreshStaminaBar();
        return true;
    }

    bool regenStarted = false;
    void RegenStamina()
    {
        if (stepsSinceDrain <= stepsUntilRegen)
        {
            return;
        }

        if (control.PlayerGrounded)
        {
            regenStarted = true;
        }
        if (regenStarted)
        {
            if (Singleton.instance.GameState.state == State.SAFE)
            {
                currentStamina += Time.deltaTime * regenRate_safe;
            }
            else if (Singleton.instance.GameState.state == State.COMBAT)
            {
                currentStamina += Time.deltaTime * regenRate_combat;
            }

        }

        if (currentStamina >= 1)
        {
            currentStamina = 1;
        }
        RefreshStaminaBar();
    }


    void DecayDrain()
    {
        if (stepsSinceDrain > stepsUntilDecay)
        {
            if (currentDrain > 0.01f)
            {
                currentDrain -= currentDrain * 0.1f;
            }
            else
            {
                currentDrain = 0;
            }
            RefreshDrainBar();
        }
    }

    void ToggleBars(bool state)
    {
        staminaBar.gameObject.SetActive(state);
        drainBar.gameObject.SetActive(state);
        background.gameObject.SetActive(state);
    }

    void RefreshStaminaBar()
    {
        staminaBar.fillAmount = currentStamina;
    }
    void RefreshDrainBar()
    {
        drainBar.fillAmount = currentDrain;
    }
}
