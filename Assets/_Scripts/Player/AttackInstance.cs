using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class AttackInstance : MonoBehaviour
{
    private float carryablePushForce = 600;

    private IObjectPool<GameObject> pool;
    public void SetPool(IObjectPool<GameObject> pool) => this.pool = pool;
    private Transform slashObj;
    private List<GameObject> objectsHit = new List<GameObject>();
    private List<GameObject> objectsHitApplied = new List<GameObject>();
    private AttackHitEffects hitEffects = null;
    private float t;
    private float manaRegenAmount;
    private float staminaRegenAmount;
    private NumberType attackType;
    private float damage;
    private Elements element;
    private float poiseDmg;
    private float kbForce;
    private float damageDelay;
    private Vector2 damageInstanceIntervalMinMax;
    private Vector3 playerPosAtSpawn;
    private PlayerAttackScriptable script;
    private bool delayedHits;
    private float delayInterval;
    private float interval;

    void Awake()
    {
        slashObj = transform.GetChild(0);
        if (hitEffects == null)
        {
            hitEffects = Singleton.instance.Player.GetComponent<AttackHitEffects>();
        }
    }

    public void Init(PlayerAttackScriptable script, Transform model)
    {
        this.script = script;
        this.delayedHits = script.useDamageDelay;
        this.attackType = script.attackType;
        this.damage = script.damage;
        this.poiseDmg = script.poiseDmg;
        this.kbForce = script.kbForce;
        this.damageDelay = script.damageDelay;
        this.damageInstanceIntervalMinMax = script.damageInstanceIntervalMinMax;
        this.staminaRegenAmount = script.staminaRegenedPerHit;
        this.manaRegenAmount = script.manaRegenedPerHit;
        playerPosAtSpawn = model.parent.position;

        objectsHit.Clear();
        objectsHitApplied.Clear();
        if (delayedHits)
            StartCoroutine(DelayedHits(script.lifeTime));
        else
            StartCoroutine(LifeTime(script.lifeTime));
        
        StartCoroutine(SlashCoroutine(script.spawnOffset, Quaternion.LookRotation(model.forward, Vector3.up) * Quaternion.Euler(script.rotation),
            script.width, script.length, script.growSpeed, script.flySpeed, script.lifeTime, script.spawnDelay));
    }

    public void Init(PlayerSpecialScriptable attackScript, Transform model, float perc)
    {
        float dmgWithCrit = Mathf.Lerp(attackScript.damageMinMax.x, attackScript.damageMinMax.y, perc);
        dmgWithCrit = attackType == NumberType.crit ? dmgWithCrit * attackScript.critMultiplier : dmgWithCrit;
        float lifeTime = Mathf.Lerp(attackScript.lifeTimeMinMax.x, attackScript.lifeTimeMinMax.y, perc);
        this.element = attackScript.attackElement;
        this.delayedHits = attackScript.useDamageDelay;
        this.attackType = attackScript.damageNumberType;
        this.damage = dmgWithCrit;
        this.poiseDmg = Mathf.Lerp(attackScript.poiseDmgMinMax.x, attackScript.poiseDmgMinMax.y, perc);
        this.kbForce = Mathf.Lerp(attackScript.kbForceMinMax.x, attackScript.kbForceMinMax.y, perc);
        this.damageDelay = Mathf.Lerp(attackScript.damageDelayMinMax.x, attackScript.damageDelayMinMax.y, perc);
        this.damageInstanceIntervalMinMax = attackScript.damageInstanceIntervalMinMax;
        this.staminaRegenAmount = Mathf.Lerp(attackScript.staminaRegenedPerHitMinMax.x, attackScript.staminaRegenedPerHitMinMax.y, perc);
        this.manaRegenAmount = Mathf.Lerp(attackScript.manaRegenedPerHitMinMax.x, attackScript.manaRegenedPerHitMinMax.y, perc);
        playerPosAtSpawn = model.parent.position;
        objectsHit.Clear();
        objectsHitApplied.Clear();
        if (delayedHits)
            StartCoroutine(DelayedHits(lifeTime));
        else
            StartCoroutine(LifeTime(lifeTime));

        StartCoroutine(SlashCoroutine(
            attackScript.spawnOffset,
            Quaternion.LookRotation(model.forward, Vector3.up) * Quaternion.Euler(attackScript.spawnRotation),
            Mathf.Lerp(attackScript.widthMinMax.x, attackScript.widthMinMax.y, perc),
            Mathf.Lerp(attackScript.lengthMinMax.x, attackScript.lengthMinMax.y, perc),
            Mathf.Lerp(attackScript.growSpeedMinMax.x, attackScript.growSpeedMinMax.y, perc),
            Mathf.Lerp(attackScript.flySpeedMinMax.x, attackScript.flySpeedMinMax.y, perc),
            lifeTime, 
            Mathf.Lerp(attackScript.spawnDelayMinMax.x, attackScript.spawnDelayMinMax.y, perc)));
    }

    public void AddToObjectsHit(GameObject obj)
    {
        if (objectsHit.Contains(obj) || objectsHitApplied.Contains(obj))
        {
            return;
        }

        if (delayedHits == false || obj.CompareTag("Carryable"))
        {
            objectsHitApplied.Add(obj);
            HitObject(obj);
        }
        else
        {
            objectsHit.Add(obj);
        }
    }

    void HitObject(GameObject obj)
    {
        Singleton.instance.PlayerStamina.GainStamina(staminaRegenAmount);
        Singleton.instance.PlayerMana.RegenMana(manaRegenAmount);
        if (obj.CompareTag("Enemy"))
        {
            obj.GetComponent<Enemy_LeekHurt>().TakeDmg(playerPosAtSpawn, hitEffects.GetActiveUpgrades(), damage, poiseDmg, kbForce);
        }
        else
        {
            PushCarryable(obj);
        }
        Singleton.instance.VFXManager.SpawnDamageNumbers(playerPosAtSpawn, obj.transform.position + Vector3.up, Vector3.up * 6, damage * 100, attackType);
        //Singleton.instance.VFXManager.SpawnVFX(VFXType.LEEK_IMPACT, obj.transform.position);
    }

    void PushCarryable(GameObject obj) {
        var rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce((obj.transform.position - playerPosAtSpawn).normalized * damage * carryablePushForce, ForceMode.Impulse);
        }
    }

    IEnumerator DelayedHits (float lifetime)
    {
        float delayTimer = 0;
        if (damageDelay > 0)
        {
            delayTimer = damageDelay;
            yield return Helpers.GetWait(damageDelay);
        }
        while (delayTimer < lifetime || (objectsHit.Count > 0))
        {
            if  (objectsHit.Count > 0)
            {
                objectsHitApplied.Add(objectsHit[0]);
                HitObject(objectsHit[0]);
                objectsHit.RemoveAt(0);
                if (damageInstanceIntervalMinMax.SqrMagnitude() > 0)
                {
                    interval = Random.Range(damageInstanceIntervalMinMax.x * 1.00f, damageInstanceIntervalMinMax.y * 1.00f);
                    delayTimer += interval;
                    yield return Helpers.GetWait(interval);
                }
            }
            delayTimer += Time.deltaTime;
            yield return null;
        }
        Deactivate();
    }

    void Deactivate()
    {
        objectsHit.Clear();
        slashObj.gameObject.SetActive(false);
        if (pool != null)
        {
            pool.Release(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator LifeTime(float slashLifeTime)
    {
        yield return Helpers.GetWait(1 + slashLifeTime);
        Deactivate();
    }

    IEnumerator SlashCoroutine(Vector3 offsetFromPlr, Quaternion rot, float width, float length, float growSpeed, float flySpeed, float slashLifeTime, float spawnDelay)
    {
        t = 0;
        yield return Helpers.GetWait(spawnDelay);
        transform.position = Singleton.instance.Player.GetChild(0).TransformPoint(offsetFromPlr);
        slashObj.transform.localScale = new Vector3(1, width, length);
        slashObj.transform.rotation = rot;
        slashObj.gameObject.SetActive(true);
        Vector3 playerVelo = Singleton.instance.Player.GetComponent<Rigidbody>().velocity * 0.5f;
        while (t < slashLifeTime)
        {
            
            transform.position += slashObj.transform.forward * flySpeed * Time.deltaTime;
            transform.position += playerVelo * Time.deltaTime;
            slashObj.localScale = new Vector3(
                slashObj.localScale.x + Time.deltaTime * growSpeed,
                slashObj.localScale.y + Time.deltaTime * growSpeed,
                slashObj.localScale.z + Time.deltaTime * growSpeed);
            t += Time.deltaTime;
            yield return null;
        }
        slashObj.gameObject.SetActive(false);
    }

    void Hit(Collider other)
    {
        if ((other.CompareTag("Enemy") || other.CompareTag("Carryable")) && other.isTrigger == false)
        {
            AddToObjectsHit(other.transform.root.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other) 
    {
        Hit(other);
    }
    private void OnTriggerStay(Collider other)
    {
        Hit(other);
    }
}
