using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Projectile")]
public class ProjectileScriptable : ScriptableObject
{
    public bool usesGravity;
    public float gravityMultiplier;
    public float projectileLifeTime;
    public bool destroyedOnImpact;
    public int collisionsBeforeDestroyed;
    [Space] [Tooltip("Hits objects that are on selected layers:")]
    public LayerMask hitLayers;
    public Vector3 projectileScaleMin;
    public Vector3 projectileScaleMax;
    public float manaCostInitial;
    public float manaCostOngoing;
    public float chargeTimeMin;
    public float chargeTimeMax;
    public float shootForceUpMultiplier;
    public float shootForceForwardMultiplier;
    [Tooltip("Leave this 1 if no brakes wanted, lower number = faster brake.")]
    public Vector2 shootForceMinMax;
    public float brakesPerc = 1;
    public float brakeTargetVeloSqrd = 10;
    [Header ("Applied to player while channeling:")]
    public Vector2 moveSpeedMultiplierMinMax;
    public Vector2 rotateSpeedMultiplierMinMax;
    public float accelModReturnTime;
    public float rotateModReturnTime;
    public Vector2 timeScaleMinMax = new(0.2f, 0.9f);

    [Header("Impact Explosion")]
    public bool explodesOnImpact = true;
    public bool explodeOnLifetimeEnd = true;
    public float explosionRadius = 5f;
    public Vector2 damageMinMax = new(0.5f, 1f);
    public Vector2 poiseDmgMinMax = new(0.5f, 1f);
    public Vector2 kbForceMinMax = new(1f, 2f);

    [Header("Impact AOE")]
    public bool leaveAOEOnImpact = true;
    public bool leaveAOEOnLifetimeEnd = true; 
    public float aoeRadius = 5f;
    public float aoeLifetime = 3f;
    public float tickInterval = 0.3f;
    public float tickDmg = 1f;
    public float poiseDmgPerTick = 0.3f;
    public float kbForcePerTick = 0.5f;

}
