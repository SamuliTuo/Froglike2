using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/PlayerSpecialAttack")]
public class PlayerSpecialScriptable : ScriptableObject {

    public Elements attackElement = Elements.NULL;
    public NumberType damageNumberType = NumberType.normal;
    public Vector2 damageMinMax = Vector2.one;
    public float critMultiplier = 1.5f;
    public bool useDamageDelay = false;
    public Vector2 damageDelayMinMax = Vector2.zero;
    public Vector2 damageInstanceIntervalMinMax = Vector2.zero;
    public Vector2 poiseDmgMinMax = Vector2.one;
    public Vector2 kbForceMinMax = Vector2.one;

    [Tooltip("Name of the animator-state that has the correct attack animation")]
    public string animatorStateName = "";

    public Vector2 manaRegenedPerHitMinMax = Vector2.one;
    public Vector2 staminaRegenedPerHitMinMax = Vector2.one;

    public Vector2 timeInvulnerableMinMax = Vector2.one;
    public float maxChargeDuration = 1.3f;
    public float initialStaminaCost = 0.31f;
    public float chargingStaminaCost = 0.7f;
    public float timeBeforeChargeStarts = 0.2f;
    public Vector2 timeScalePercMinMax = Vector2.one;
    public Vector2 stepForceMinMax = Vector2.one;
    public float maxChargeRotationMult = 0.5f;
    public Vector2 hitDurationMinMax = Vector2.one;
    public float hitboxLifetime = 0.5f;

    public float animationSpeed = 1;
    [Tooltip("At what perc of the animation-file will the animation start"), Range(0, 0.99f)]
    public float animationStartPerc = 0;
    [Tooltip("At what % of the attack animation will the next attack start if queued."), Range(0, 1)]
    public float nextAttackInitPerc = 1;


    [Header("Swing effect orientation")]
    public Vector2 widthMinMax = Vector2.one;
    public Vector2 lengthMinMax = Vector2.one;
    public Vector2 growSpeedMinMax = Vector2.one;
    public Vector2 flySpeedMinMax = Vector2.one;
    public Vector2 lifeTimeMinMax = Vector2.one;
    public Vector2 spawnDelayMinMax = Vector2.one;

    public Vector3 spawnOffset;
    public Vector3 spawnRotation;
}
