using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PlayerAttackSpawner : MonoBehaviour
{

    private GameObject attackObject;
    private IObjectPool<GameObject> attackPool;
    public void ClearPool() { attackPool.Clear(); }

    // Start is called before the first frame update
    void Awake()
    {
        attackObject = Resources.Load("PlayerAttacks/AttackInstance") as GameObject;
        attackPool = new ObjectPool<GameObject>(CreateAttack, OnTakeAttackFromPool, OnReturnAttackToPool);
    }

    public AttackInstance SpawnAttack(PlayerAttackScriptable attackScriptable, Transform model)
    {
        var obj = attackPool.Get();
        obj.GetComponent<AttackInstance>().Init(attackScriptable, model);
        return obj.GetComponent<AttackInstance>();
    }
    public AttackInstance SpawnAttack(Vector3 offsetFromPlr, Quaternion rot, 
        float width, float length, float growSpeed, float flySpeed, float slashLifeTime, float spawnDelay, 
        float damage, float poiseDmg, float kbForce, float damageDelay, Vector2 damageInstanceInterval, 
        float manaRegenPerHit, float staminaRegenedPerHit, NumberType attackType)
    {
        var obj = attackPool.Get();
        obj.GetComponent<AttackInstance>().Init(offsetFromPlr, rot, width, length, growSpeed, flySpeed, slashLifeTime, 
            spawnDelay, damage, poiseDmg, kbForce, damageDelay, damageInstanceInterval, manaRegenPerHit, staminaRegenedPerHit, true, attackType);
        return obj.GetComponent<AttackInstance>();
    }

    GameObject CreateAttack()
    {
        var instance = Instantiate(attackObject);
        instance.GetComponent<AttackInstance>().SetPool(attackPool);
        return instance;
    }
    void OnTakeAttackFromPool(GameObject attack)
    {
        attack.SetActive(true);
    }
    void OnReturnAttackToPool(GameObject attack)
    {
        attack.SetActive(false);
    }
}
