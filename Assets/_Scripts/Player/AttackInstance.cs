using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class AttackInstance : MonoBehaviour
{
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
    private float poiseDmg;
    private float kbForce;
    private float damageDelay;
    private Vector2 damageInstanceIntervalMinMax;
    private Vector3 playerPosAtSpawn;
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
        
        StartCoroutine(SlashCoroutine(
            script.spawnOffset,
            Quaternion.LookRotation(model.forward, Vector3.up) * Quaternion.Euler(script.rotation),
            script.width, script.length, 
            script.growSpeed, script.flySpeed, 
            script.lifeTime, script.spawnDelay));
    }
    public void Init(
        Vector3 offsetFromPlr, Quaternion rot, 
        float width, float length, float growSpeed, float flySpeed, float slashLifeTime, float spawnDelay, 
        float damage, float poiseDmg, float kbForce, float damageDelay, 
        Vector2 damageInstanceIntervalMinMax, float manaRegenAmount, float staminaRegenedPerHit,
        bool delayedHits, NumberType attackType)
    {
        this.attackType = attackType;
        this.delayedHits = delayedHits;
        this.damage = damage;
        this.poiseDmg = poiseDmg;
        this.kbForce = kbForce;
        this.damageDelay = damageDelay;
        this.damageInstanceIntervalMinMax = damageInstanceIntervalMinMax;
        this.staminaRegenAmount = staminaRegenedPerHit;
        this.manaRegenAmount = manaRegenAmount;
        playerPosAtSpawn = Singleton.instance.Player.position;

        objectsHit.Clear();
        objectsHitApplied.Clear();
        if (delayedHits)
            StartCoroutine(DelayedHits(slashLifeTime));
        else
            StartCoroutine(LifeTime(slashLifeTime));

        StartCoroutine(SlashCoroutine(
            offsetFromPlr, rot,
            width, length,
            growSpeed, flySpeed,
            slashLifeTime, spawnDelay));
    }

    public void AddToObjectsHit(GameObject obj)
    {
        if (objectsHit.Contains(obj) || objectsHitApplied.Contains(obj))
        {
            return;
        }

        if (delayedHits)
        {
            objectsHit.Add(obj);
        }
        else
        {
            objectsHitApplied.Add(obj);
            HitObject(obj);
        }
    }

    void HitObject(GameObject obj)
    {
        Singleton.instance.PlayerStamina.GainStamina(staminaRegenAmount);
        Singleton.instance.PlayerMana.RegenMana(manaRegenAmount);
        obj.GetComponent<Enemy_LeekHurt>().TakeDmg(playerPosAtSpawn, hitEffects.GetActiveUpgrades(), damage, poiseDmg, kbForce);
        //Singleton.instance.VFXManager.SpawnVFX(VFXType.LEEK_IMPACT, obj.transform.position);
        Singleton.instance.VFXManager.SpawnDamageNumbers(playerPosAtSpawn, obj.transform.position + Vector3.up, Vector3.up * 6, damage * 100, attackType);
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
        if (other.CompareTag("Enemy") && other.isTrigger == false)
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
