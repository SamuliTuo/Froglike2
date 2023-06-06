using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum QueuedAction { NULL, ATTACK, SPECIAL, ROLL, JUMP, MOUTH, TONGUE, CROUCH, }

public class PlayerInputQueue : MonoBehaviour
{
    private PlayerController control;
    private PlayerGravity gravity;
    private PlayerStamina stamina;
    private PlayerAttacks attack;
    private PlayerSpecialAttack special;
    private PlayerRolling roll;
    private PlayerJumping jump;
    private PlayerMouthController mouth;
    private TongueController tongue;
    private PlayerCrawl crouch;
    private PlayerElementController elements;


    void Start()
    {
        control = GetComponent<PlayerController>();
        gravity = GetComponent<PlayerGravity>();
        stamina = GetComponent<PlayerStamina>();
        attack = GetComponent<PlayerAttacks>();
        special = GetComponent<PlayerSpecialAttack>();
        roll = GetComponent<PlayerRolling>();
        jump = GetComponent<PlayerJumping>();
        mouth = GetComponent<PlayerMouthController>();
        tongue = GetComponent<TongueController>();
        crouch = GetComponent<PlayerCrawl>();
        elements = GetComponentInChildren<PlayerElementController>();
    }

    public void InitQueuedAction()
    {
        if (control.IsQueuedAction(QueuedAction.ATTACK))
        {
            attack.InitAttack();
        }
        else if (control.IsQueuedAction(QueuedAction.SPECIAL) && stamina.HasStamina(special.attackScript.initialStaminaCost))
        {
            if (Random.Range(0, 2) == 0)
                special.StartBaseballSamurai(PlayerStates.SAMURAI);
            else
                special.StartBaseballSamurai(PlayerStates.BASEBALL);
        }
        else if (control.IsQueuedAction(QueuedAction.ROLL))
        {
            roll.InitRoll();
        }
        else if (control.IsQueuedAction(QueuedAction.JUMP))
        {
            jump.playerJumpPressed = true;
            gravity.StopJumpCoroutines();
            control.state = PlayerStates.NORMAL;
        }
        else if (control.IsQueuedAction(QueuedAction.MOUTH))
        {
            control.state = PlayerStates.NORMAL;
            mouth.InitFireball();
        }
        else if (control.IsQueuedAction(QueuedAction.TONGUE))
        {
            control.state = PlayerStates.NORMAL;
            tongue.InitTonguePull();
        }
        else if (control.IsQueuedAction(QueuedAction.CROUCH))
        {
            crouch.InitCrawlOnStuckUnder();
        }
        else if (control.IsQueuedAction(QueuedAction.NULL))
        {
            control.ResetPlayer();
            control.state = PlayerStates.NORMAL;
        }
    }



    void OnAttack()
    {
        control.SetQueuedAction(QueuedAction.ATTACK);
    }
    void OnSpecial(InputValue value)
    {
        var pressed = value.isPressed;
        if (pressed == false) return;
        control.SetQueuedAction(QueuedAction.SPECIAL);
    }
    void OnJump(InputValue value)
    {
        var pressed = value.isPressed;
        if (pressed == false) return;
        control.SetQueuedAction(QueuedAction.JUMP);
    }
    void OnRoll()
    {
        control.SetQueuedAction(QueuedAction.ROLL);
    }
    void OnMouth(InputValue value)
    {
        var pressed = value.isPressed;
        if (pressed == false) return;
        control.SetQueuedAction(QueuedAction.MOUTH);
    }
    void OnTongue()
    {
        control.SetQueuedAction(QueuedAction.TONGUE);
    }

    void OnChangeElementLast() => elements.ChangeElementLast();
    void OnChangeElementNext() => elements.ChangeElementNext();
    void OnApplyElementOnWeapon() => elements.ApplyElementOnWeapon();
}
