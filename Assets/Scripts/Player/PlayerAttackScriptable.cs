using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/PlayerAttack")]
public class PlayerAttackScriptable : ScriptableObject {

    [Tooltip("Name of the animator-state that has the correct attack animation")]
    public string animatorStateName = "";

    [Tooltip("Leave 0 if using animation duration")]
    public float attackDuration = 0;

    public float animationSpeed = 1;

    [Tooltip("At what perc of the animation-file will the animation start"), Range(0, 0.99f)]
    public float animationStartPerc = 0;

    [Header ("Next attack:")]
    [Tooltip("Can player combo another attack after this one? Leave empty if no combos.")]
    public PlayerAttackScriptable nextAttack = null;

    [Tooltip("At what % of the attack animation will the next attack start if queued."), Range(0, 1)]
    public float nextAttackInitPerc = 1;

    [Header("No target")]
    [Tooltip("How fast will the player take a step.")]
    public float stepForce = 0;

    [Tooltip("How long will the step take.")]
    public float stepDuration = 0;

    [Header("Has target")]
    [Tooltip("At what percentage will the attack-homing reach it's destination."), Range(0, 1)]
    public float attackHomingEndPerc = 0.5f;

    [Header("Give back movement:")]
    [Tooltip("At what point of animation will the moveSpeed start returning to player."), Range(0, 1)]
    public float moveReturnPerc = 1;

    [Tooltip("How long it will take to return full moveSpeed to player"), Min(0)]
    public float moveReturnDuration = 0;

    [Header("Give back rotation:")]
    [Tooltip("At what point of animation will the rotation start returning to player."), Range(0, 1)]
    public float rotReturnPerc = 1;

    [Tooltip("How long it will take to return full rotation to player"), Min(0)]
    public float rotReturnDuration = 0;

    [Header("Hitboxes")]
    [Tooltip("At what point of attack will the sword-hitboxes activate"), Range(0.0f, 1.0f)]
    public float weaponColActivatePerc = 0.25f;

    [Tooltip("At what point of attack will the sword-hitboxes de-activate"), Range(0.0f, 1.0f)]
    public float weaponColDeactivatePerc = 0.75f;

    [Header("Swing effect orientation")]
    public float width = 1;
    public float length = 1;
    public float growSpeed = 1;
    public float flySpeed = 1;
    public float lifeTime = 0.1f;
    public float spawnDelay = 0.1f;
    public Vector3 spawnOffset;
    public Vector3 rotation;
}
