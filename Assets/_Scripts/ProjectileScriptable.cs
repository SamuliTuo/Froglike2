using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Projectile")]
public class ProjectileScriptable : ScriptableObject
{
    public bool usesGravity;
    public float projectileLifeTime;
    public bool destroyedOnImpact;
    public int collisionsBeforeDestroyed;
    [Space] [Tooltip("Hits objects that are on selected layers:")]
    public LayerMask hitLayers;
    public Vector3 projectileScaleMin;
    public Vector3 projectileScaleMax;
    public float manaCostInitial;
    public float manaCostOngoing;
    public float chargeTime;
    public Vector2 shootForceMinMax;
    public Vector2 moveSpeedMultiplierMinMax;
    public Vector2 rotateSpeedMultiplierMinMax;
    

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
