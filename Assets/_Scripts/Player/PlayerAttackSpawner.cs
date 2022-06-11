using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PlayerAttackSpawner : MonoBehaviour
{

    private GameObject attackObject;
    private IObjectPool<GameObject> attackPool;
    public void ResetPool() { attackPool.Clear(); }

    // Start is called before the first frame update
    void Awake()
    {
        attackObject = Resources.Load("PlayerAttacks/AttackInstance") as GameObject;
        attackPool = new ObjectPool<GameObject>(CreateAttack, OnTakeAttackFromPool, OnReturnAttackToPool);
    }

    public AttackInstance SpawnAttack(Vector3 pos, Quaternion rot, 
        float width, float length, float growSpeed, float flySpeed, float slashLifeTime, float spawnDelay, 
        float damage, float manaRegenPerHit)
    {
        var obj = attackPool.Get();
        obj.transform.position = Singleton.instance.Player.position;
        obj.GetComponent<AttackInstance>().Init(pos, rot, width, length, growSpeed, flySpeed, slashLifeTime, 
            spawnDelay, damage, manaRegenPerHit);
        obj.transform.position = pos;
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
